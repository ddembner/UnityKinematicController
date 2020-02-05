using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicController : MonoBehaviour {

    [Header("Player options")]
    [SerializeField] LayerMask allButPlayer;

    [Header("Movement Options")]
    [SerializeField] private float movementSpeed = 10f;
    private Vector3 simpleMove;
    private Vector3 velocity;

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
    [SerializeField] private float groundSphereRadius;

    private bool _grounded = true;
    public bool IsGrounded { get { return _grounded; } }

    // Start is called before the first frame update
    void Start() {
        
    }

    /*
     * Steps for movements
     * 1. Update Gravity
     * 2. Get Inputs
     * 3. Cast Ray to solve for high speed move and reduce move magnitude to equal ray distance
     * 4. Get External Forces (optional forces such as knockback and friction)
     * 5. Update Final Velocity
     * 6. Colllsion Resolver
     * 7. Ground Check
    */

    void Update() {
        Gravity();
        InputMove();
        ContinuousCollisionDetection();
        FinalMove();
        GroundCheck();
        //CollisionResolver();
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

        Vector3 camForward = _camera.forward;
        Vector3 camRight = _camera.right;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        camForward.y = 0f;
        camRight.y = 0f;

        simpleMove = (horizontal * camRight + simpleMove.y * Vector3.up + forward * camForward) * Time.deltaTime;

    }

    private void ContinuousCollisionDetection() {

        Vector3 movement = simpleMove;
        Ray ray = new Ray(transform.position, movement.normalized);
        RaycastHit[] rayHits = new RaycastHit[8];

        float distanceToPoint = capsuleCollider.height / 2 - capsuleCollider.radius;
        Vector3 point1 = (capsuleCollider.center + Vector3.up * distanceToPoint) + transform.position;
        Vector3 point2 = (capsuleCollider.center - Vector3.up * distanceToPoint) + transform.position;
        float radius = capsuleCollider.radius;

        int num = Physics.CapsuleCastNonAlloc(point1, point2, radius, movement.normalized, rayHits, 20f, allButPlayer, QueryTriggerInteraction.UseGlobal);
        Debug.Log(num);
        for(int i = 0; i < num; i++) {

            if (rayHits[i].collider.isTrigger) {
                continue;
            }

            Debug.Log(rayHits[i].distance);
            
        }
    }

    private void FinalMove() {




        velocity = simpleMove;


        transform.position += velocity;

        if (velocity != Vector3.zero) {
            Vector3 velRot = velocity.normalized;
            velRot.y = 0f;

            if (velRot != Vector3.zero) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velRot, transform.up), 0.2f);
            }

            //transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
        }

        simpleMove = Vector3.zero;
        velocity = Vector3.zero;
    }

    private void CollisionResolver() {

        float distanceToPoint = capsuleCollider.height / 2 - capsuleCollider.radius;
        Vector3 point1 = (capsuleCollider.center + Vector3.up * distanceToPoint) + transform.position;
        Vector3 point2 = (capsuleCollider.center - Vector3.up * distanceToPoint) + transform.position;
        float radius = capsuleCollider.radius;

        Collider[] collisions = new Collider[5];
        int numCollision = Physics.OverlapCapsuleNonAlloc(point1, point2, radius, collisions, allButPlayer, QueryTriggerInteraction.UseGlobal);


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

    private void GroundCheck() {

        //TODO: Predict if will hit ground and act accordingly
        /*
        float distanceToPoint = capsuleCollider.height / 2 - capsuleCollider.radius;
        Vector3 point1 = (capsuleCollider.center + Vector3.up * distanceToPoint) + transform.position;
        Vector3 point2 = (capsuleCollider.center - Vector3.up * distanceToPoint) + transform.position;
        float radius = capsuleCollider.radius * 1f;
        if(Physics.CapsuleCast(point1, point2, radius, Vector3.down, 0.1f, allButPlayer)) {
            _grounded = true;
        }
        else {
            _grounded = false;
        }
        */

        //Determine max number of wanted collisions to be stored
        Collider[] groundCollisions = new Collider[3];
        int num = Physics.OverlapSphereNonAlloc(transform.position + groundSphereOffset, groundSphereRadius, groundCollisions, allButPlayer, QueryTriggerInteraction.UseGlobal);
        //From here we can use the number of collisions to determine individually what should happen at this point of the frame.
        
        if(num > 0) {
            _grounded = true;
        }
        else {
            _grounded = false;
        }

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(transform.position + groundSphereOffset, groundSphereRadius);
    }

}
