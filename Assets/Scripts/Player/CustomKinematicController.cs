using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicController : MonoBehaviour {

    [Header("Player options")]
    [SerializeField] LayerMask allButPlayer;

    [Header("Movement Options")]
    [SerializeField] private float movementSpeed = 10f;
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

    // Update is called once per frame
    void Update() {
        Gravity();
        Move();
        GroundCheck();
    }

    private void Move() {

        float horizontal = Input.GetAxis("Horizontal");
        float forward = Input.GetAxis("Vertical");

        Vector3 camForward = _camera.forward;
        Vector3 camRight = _camera.right;
        camForward = camForward.normalized;
        camRight = camRight.normalized;

        camForward.y = 0f;
        camRight.y = 0f;
        velocity = horizontal * camRight + velocity.y * Vector3.up + forward * camForward;
        velocity.x *= movementSpeed;
        velocity.z *= movementSpeed;
        //Camera Movement


        transform.position += velocity * Time.deltaTime;

        if (velocity != Vector3.zero) {
            Vector3 velRot = velocity.normalized;
            velRot.y = 0f;

            if (velRot != Vector3.zero) {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velRot, transform.up), 0.2f);
            }

            //transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
        }

        velocity = Vector3.zero;
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

        velocity.y = currentGravity;
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
