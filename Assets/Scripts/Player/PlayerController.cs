using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("Player Options")]
    [SerializeField]
    private float playerHeight = 2f;
    [SerializeField] 
    private float movementSpeed;
    [SerializeField]
    private bool smoothSlope = true;
    [SerializeField]
    private float smoothSlopeSpeed = 1f;
    
    //Player Move Vectors
    private Vector3 velocity;
    private Vector3 move;
    private Vector3 vel;

    [Header("References")]
    [SerializeField]
    private Transform playerCam;
    [SerializeField]
    private Collider mainCol;
    [SerializeField]
    private SphereCollider sphereCol;
    [SerializeField]
    private CapsuleCollider capsuleCol;
    [SerializeField]
    Rigidbody rb;

    [Header("Gravity/Ground Options")]
    //Gravity Variables
    [SerializeField]
    private float gravity = 3f;
    private bool grounded;
    private float currentGravity = 0f;
    [SerializeField]
    private float maxGravity = -100f;
    private Vector3 liftPoint = new Vector3(0f, 1.2f, 0f);
    private RaycastHit groundHit;
    [SerializeField]
    private Vector3 groundCheckPoint = new Vector3(0f, -0.87f, 0f);
    [SerializeField]
    private float sphereCheckRadius = 0.5f;
    

    [Header("LayerMask")]
    [SerializeField]
    private LayerMask notPlayerMask;



    void Start() {
        
    }

    void Update() {
        Gravity();
        SimpleMove();
        FinalMove();
        GroundCheck();
        CollisionCheck();

        Debug.Log(rb.detectCollisions);
    }

    private void SimpleMove() {

        float horizontal = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");

        move = new Vector3(horizontal, 0f, forward);
    }

    private void FinalMove() {

        Vector3 camForward = playerCam.forward;
        Vector3 camRight = playerCam.right;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        camForward.y = 0f;
        camRight.y = 0f;

        //velocity = transform.TransformDirection(velocity);
        velocity = new Vector3(move.x, velocity.y, move.z) * movementSpeed;
        velocity = velocity.x * camRight + velocity.z * camForward;
        velocity.y = currentGravity;
        velocity += vel;
        transform.position += velocity * Time.deltaTime;

        if(velocity != Vector3.zero) {
            Vector3 velRot = velocity.normalized;
            velRot.y = 0f;
            
            if(velRot != Vector3.zero) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velRot, transform.up), 0.2f);
            }
            
            //transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
        }

        velocity = Vector3.zero;

    }

    private void Gravity() {

        if (grounded == false) {
            currentGravity -= gravity * Time.deltaTime;
            currentGravity = Mathf.Clamp(currentGravity, -maxGravity, maxGravity);
        }
        else {
            currentGravity = 0f;
        }
    }

    private void GroundCheck() {

        Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
        RaycastHit tempHit = new RaycastHit();

        if (Physics.SphereCast(ray, 0.17f, out tempHit, 20f, notPlayerMask)) {
            GroundConfirm(tempHit);
        }
        else {
            grounded = false;
        }
    }

    private void GroundConfirm(RaycastHit tempHit) {

        //float currentSlope = Vector3.Angle(tempHit.normal, Vector3.up);
        Collider[] col = new Collider[3];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundCheckPoint), sphereCheckRadius, col, notPlayerMask);

        grounded = false;

        for(int i = 0; i < num; i++) {

            if (col[i].isTrigger) {
                continue;
            }

            if(col[i].transform == tempHit.transform) {
                groundHit = tempHit;
                grounded = true;

                if (!smoothSlope) {
                    transform.position = new Vector3(transform.position.x, (groundHit.point.y + playerHeight / 2), transform.position.z);
                }
                else {
                    transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, (groundHit.point.y + playerHeight / 2), transform.position.z), smoothSlopeSpeed * Time.deltaTime);
                }

                break;
            }
        }

        if(num <= 1 && tempHit.distance <= 3.1f) {
            if(col[0] != null) {
                Ray ray = new Ray(transform.TransformPoint(liftPoint), Vector3.down);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit, 3.1f, notPlayerMask)) {
                    if(hit.transform != col[0].transform) {
                        grounded = false;
                        return;
                    }
                }
            }
        }
    }

    private void CollisionCheck() {

        Collider[] overlaps = new Collider[4];
        RaycastHit[] hits = new RaycastHit[4];
        //int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(sphereCol.center), sphereCol.radius, overlaps, notPlayerMask, QueryTriggerInteraction.UseGlobal);
        float distanceToPoint = capsuleCol.height / 2 - capsuleCol.radius;
        Vector3 point1 = capsuleCol.center + Vector3.up * distanceToPoint;
        Vector3 point2 = capsuleCol.center - Vector3.up * distanceToPoint;
        float radius = capsuleCol.radius * 1f;
        int num = Physics.OverlapCapsuleNonAlloc(point1 + transform.position, point2 + transform.position, radius, overlaps, notPlayerMask, QueryTriggerInteraction.UseGlobal);
        
        int num2 = Physics.CapsuleCastNonAlloc(point1 + transform.position, point2 + transform.position, radius, transform.forward, hits, movementSpeed, notPlayerMask, QueryTriggerInteraction.UseGlobal);
        Debug.Log(num);

        for(int i = 0; i < overlaps.Length; i++) {
            if(overlaps[i] != null) {
                Debug.Log(i + ": " + overlaps[i].name);
            }
        }

        

        for(int i = 0; i < num; i++) {

            if (overlaps[i].isTrigger) {
                continue;
            }

            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if(Physics.ComputePenetration(capsuleCol, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist)) {

                Vector3 penetrationVec = dir * dist;
                Vector3 velocityProjected = Vector3.Project(velocity, -dir);
                transform.position = transform.position + penetrationVec;
                vel -= velocityProjected;
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckPoint), sphereCheckRadius);
    }

    private void CanMove(Vector3 dir) {

        RaycastHit[] raycastHits = new RaycastHit[4];
        float distanceToPoint = capsuleCol.height / 2 - capsuleCol.radius;
        Vector3 point1 = transform.position + capsuleCol.center + Vector3.up * distanceToPoint;
        Vector3 point2 = transform.position + capsuleCol.center - Vector3.up * distanceToPoint;
        float radius = capsuleCol.radius * 0.95f;
        float castDistance = 0.5f;
        int num = Physics.CapsuleCastNonAlloc(point1, point2, radius, dir, raycastHits, castDistance, notPlayerMask);

        if(num == 0) {
            Debug.Log("You can move");
        }
        else {
            Debug.Log("Don't move");
        }
    }



}
