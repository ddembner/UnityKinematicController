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

    private bool _grounded = false;
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
        CheckFastMove();
        FinalMove();
        GroundCheck();
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

        simpleMove = (horizontal * camRight + velocity.y * Vector3.up + forward * camForward) * Time.deltaTime;

    }

    private void CheckFastMove() {

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

    private void GroundCheck() {

        //TODO: Predict if will hit ground and act accordingly
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
    }

}
