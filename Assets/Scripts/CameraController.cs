using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    [SerializeField]
    private bool lockCursor = false;
    [SerializeField] 
    private float mouseSensitivity = 10f;
    [SerializeField] 
    private Transform target;
    [SerializeField] 
    private float distanceFromTarget = 2f;
    [SerializeField] 
    private Vector2 pitchMinMax = new Vector2(-40, 85);
    [SerializeField] 
    private float rotationSmoothTime = 0.12f;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;
    float yaw;
    float pitch;
                       
    void Start() {

        if (lockCursor) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }


    }

    void LateUpdate() {
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;
        transform.position = target.position - transform.forward * distanceFromTarget;
    }
}
