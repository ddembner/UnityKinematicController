using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomKinematicController : MonoBehaviour {

    [Header("Movement Options")]
    [SerializeField] private float movementSpeed = 10f;
    private Vector3 velocity;

    [Header("Gravity options")]
    [SerializeField] private bool toggleGravity = true;
    [SerializeField] private float gravityValue = -5;

    [Header("References")]
    [SerializeField] private Transform _camera;

    private bool grounded = false;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        Gravity();
        Move();
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
        velocity = (horizontal * camRight + velocity.y * Vector3.up + forward * camForward) * movementSpeed;

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

        if (!grounded && toggleGravity) {
            velocity.y += gravityValue;
        }
        else {
            velocity.y = 0f;
        }
    }

    private void GroundCheck() {

    }

}
