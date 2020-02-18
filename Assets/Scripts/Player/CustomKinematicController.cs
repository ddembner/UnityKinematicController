using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicController : MonoBehaviour {

    [Header("Player options")]
    [SerializeField] LayerMask allButPlayer;

    [Header("Movement Options")]
    [SerializeField] private bool toggleImmediateStop = true;
    [SerializeField] private bool toggleIcePhysics = true;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float iceMoveSpeed = 0.1f;
    [SerializeField] private float stepOffset = 0.2f;
    [SerializeField] private float maxSlope = 45f;
    private Vector3 simpleMove;
    private Vector3 velocity;
    private Vector3 force;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float damp = 0.1f;

    [Header("Gravity options")]
    [SerializeField] private bool toggleGravity = true;
    [SerializeField] private float gravityValue = -5f;
    [SerializeField] private float initialGravityStart = -10f;
    private bool initialGravityFrame = true;
    private float currentGravity = 0f;

    [Header("References")]
    [SerializeField] private Transform _camera;
    [SerializeField] private CapsuleCollider capsuleCollider;

    [Header("Physics Options")]
    [SerializeField] private Vector3 groundSphereOffset;
    [SerializeField] private float groundSphereCastDist = 0.01f;
    [SerializeField] private float groundSphereRadius;
    [SerializeField] private Vector3 touchOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float forceStopThreshold = 0.01f;

    /****************************
     **** PLAYER INFORMATION ****
    ****************************/
    private bool _grounded = true;
    public bool IsGrounded { get { return _grounded; } }

    float distanceToPoint;
    Vector3 point1;
    Vector3 point2;
    float radius;
    Vector3 closestPoint;

    // Start is called before the first frame update
    void Start() {
        distanceToPoint = capsuleCollider.height / 2 - capsuleCollider.radius;
        point1 = (capsuleCollider.center + Vector3.up * distanceToPoint);
        point2 = (capsuleCollider.center - Vector3.up * distanceToPoint);
        radius = capsuleCollider.radius;
    }

    /*
     * Steps for movements
     * 1. Update Gravity
     * 2. Get Inputs
     * 3. Cast Ray to solve for high speed move and reduce move magnitude to equal ray distance
     * 4. Get External Forces (optional forces such as knockback and friction)
     * 5. Update Final Velocity
     * 6. Colllsion Resolver
     * 7. Touch Function
     * 8. Ground Check
    */

    void Update() {
        Gravity();
        InputMove();
        //ContinuousCollisionDetection();
        FinalMove();
        DecreaseForce();
        GroundCheck();
        Touch();
        CollisionResolver();
        Debug.Log(force.magnitude);
    }

    private void Gravity() {

        if (!IsGrounded && toggleGravity && initialGravityFrame) {
            currentGravity = initialGravityStart;
            initialGravityFrame = false;
        }
        else if (!IsGrounded && toggleGravity && !initialGravityFrame) {
            currentGravity += gravityValue * Time.deltaTime;
        }
        else {
            currentGravity = 0f;
            initialGravityFrame = true;
        }

        simpleMove.y = currentGravity;
    }

    private void InputMove() {

        float horizontal = Input.GetAxis("Horizontal") * movementSpeed;
        float forward = Input.GetAxis("Vertical") * movementSpeed;

        if (toggleImmediateStop) {
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) {
                forward = 0f;
            }
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) {
                horizontal = 0f;
            }
        }

        

        Vector3 camForward = _camera.forward;
        Vector3 camRight = _camera.right;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        camForward.y = 0f;
        camRight.y = 0f;

        simpleMove = (horizontal * camRight + simpleMove.y * Vector3.up + forward * camForward);

        //Ice Physics
        if (toggleIcePhysics) {

            if (force.sqrMagnitude < maxForce * maxForce) {
                force += simpleMove.normalized * iceMoveSpeed;
            }
        }

    }

    private void ContinuousCollisionDetection() {

        Vector3 movement = simpleMove;
        Ray ray = new Ray(transform.position, movement.normalized);
        RaycastHit[] rayHits = new RaycastHit[8];

        int num = Physics.CapsuleCastNonAlloc(point1 + transform.position, point2 + transform.position, radius, movement.normalized, rayHits, movement.magnitude, allButPlayer, QueryTriggerInteraction.UseGlobal);

        for(int i = 0; i < num; i++) {

            //if (rayHits[i].collider.isTrigger) {
            //    continue;
            //}

            ////float pointHeight = rayHits[i].point.y - rayHits[i].collider.bounds.size.y;

            ////if(pointHeight < stepOffset) {
            ////    Debug.LogError("poooooooooop");
            ////    simpleMove.y += rayHits[i].point.y;
            ////}
            //if(rayHits[i].distance < 0.0001f) { //Is the number so small that is it worth moving?
            //    simpleMove = Vector3.zero;
            //}
            //else if (simpleMove.magnitude * Time.deltaTime > rayHits[i].distance) {
            //    transform.position = rayHits[i].point + capsuleCollider.center;
            //    //transform.position += capsuleCollider.radius * rayHits[i].normal;
            //    Debug.Log("Capsule Center" + capsuleCollider.center);
            //    Debug.Log("simplemove normal: " + simpleMove.normalized);
            //    Debug.LogError("Oops");
            //    Debug.Log("point normal: " + rayHits[i].point.normalized);
            //    Debug.Log("collision normal: " + rayHits[i].normal);

            //    simpleMove = Vector3.zero;
            //}
            //else {
                //Move normally
                simpleMove = simpleMove.normalized * simpleMove.magnitude;
            //}
            
            
        }
    }

    private void FinalMove() {


        velocity = Vector3.zero;

        velocity = simpleMove * Time.deltaTime;


        transform.position += velocity;

        transform.position += force * Time.deltaTime;

        if (velocity != Vector3.zero) {
            Vector3 velRot = velocity.normalized;
            velRot.y = 0f;

            if (velRot != Vector3.zero) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velRot, transform.up), 0.2f);
            }

            //transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
        }

        simpleMove = Vector3.zero;

    }

    private void DecreaseForce() {

        force = Vector3.Lerp(force, Vector3.zero, damp * Time.deltaTime);

    }

    private void CollisionResolver() {

        //float distanceToPoint = capsuleCollider.height / 2 - capsuleCollider.radius;
        //Vector3 point1 = (capsuleCollider.center + Vector3.up * distanceToPoint) + transform.position;
        //Vector3 point2 = (capsuleCollider.center - Vector3.up * distanceToPoint) + transform.position;
        //float radius = capsuleCollider.radius;

        Collider[] collisions = new Collider[5];
        int numCollision = Physics.OverlapCapsuleNonAlloc(point1 + transform.position, point2 + transform.position, radius, collisions, allButPlayer, QueryTriggerInteraction.UseGlobal);


        for(int i = 0; i < numCollision; i++) {

            //We don't care about trigger collisions 
            if (collisions[i].isTrigger) {
                continue;
            }

            Transform colTransform = collisions[i].transform;
            Vector3 dir;
            float distance;

            if(Physics.ComputePenetration(capsuleCollider, transform.position, transform.rotation, collisions[i], colTransform.position, colTransform.rotation, out dir, out distance)) {

                transform.position += dir * distance;

            }
        }
        
    }

    private void Touch() {

        Collider[] touchColliders;
        touchColliders = Physics.OverlapCapsule(point1 + transform.position + touchOffset, point2 + transform.position + touchOffset, radius + (touchOffset.y * 4), allButPlayer);

    }

    private void GroundCheck() {

        //Determine max number of wanted collisions to be stored
        RaycastHit[] groundCollisions = new RaycastHit[3];
        int num = Physics.SphereCastNonAlloc(transform.position + groundSphereOffset, groundSphereRadius, Vector3.down, groundCollisions, groundSphereCastDist, allButPlayer, QueryTriggerInteraction.UseGlobal);
        //From here we can use the number of collisions to determine individually what should happen at this point of the frame.

        _grounded = false;

        for(int i = 0; i < num; i++) {
            if (groundCollisions[i].collider.isTrigger) {
                continue;
            }
            closestPoint = Physics.ClosestPoint(groundCollisions[i].point, capsuleCollider, transform.position, transform.rotation);
            //transform.position = new Vector3(transform.position.x, (groundCollisions[i].point.y + capsuleCollider.height / 2), transform.position.z);

            //This gets the angle of a slope that the player is standing on
            //float slope = Mathf.Round(Vector3.Angle(transform.up, groundCollisions[i].normal));

            _grounded = true;
            break;
        }

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(transform.position + groundSphereOffset, groundSphereRadius);
        Gizmos.DrawSphere(closestPoint, 0.5f);
    }

}
