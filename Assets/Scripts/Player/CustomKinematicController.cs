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
    float groundAngle = 0f;
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

    int frame = 0;
    void Update() {
        frame++;
        Gravity();
        InputMove();
        //ContinuousCollisionDetection();
        FinalMove();
        DecreaseForce();
        GroundCheck();
        GetGroundAngle();
        Touch();
        //CollisionResolver();
        //Debug.Log("Frame: " + frame);
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

        simpleMove = simpleMove.normalized * simpleMove.magnitude;    
        
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

    RaycastHit groundHit;
    Vector3 hitLocation;
    private void GetGroundHitLoc() {

        if(Physics.Raycast(transform.position, -transform.up, out groundHit, 2f, allButPlayer)){
            hitLocation = groundHit.point;
        }

        Debug.Log(hitLocation);
    }

    private void GroundCheck() {

        _grounded = false;
        Collider[] groundCollisions = Physics.OverlapSphere(transform.position + groundSphereOffset, groundSphereRadius, allButPlayer, QueryTriggerInteraction.Collide);

        for(int i = 0; i < groundCollisions.Length; i++) {

            //This is where to check if certain colliders should be ignored
            if (groundCollisions[i].isTrigger) continue;
            _grounded = true;
            break;
        }

        GetGroundHitLoc();

    }

    private void GetGroundAngle() {

        //No point in getting angle if not grounded
        if (!_grounded) return;

        RaycastHit rayHit = new RaycastHit();
        if(Physics.Raycast(transform.position, -transform.up * (capsuleCollider.height / 2), out rayHit, allButPlayer)){

            groundAngle = Vector3.Angle(rayHit.normal, transform.up);
            Debug.Log(groundAngle);
        }

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(transform.position + groundSphereOffset, groundSphereRadius);

        Gizmos.color = Color.yellow;
    }

}
