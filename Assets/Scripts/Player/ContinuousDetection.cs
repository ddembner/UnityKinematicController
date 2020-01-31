using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousDetection : MonoBehaviour {

    public CapsuleCollider capsuleCollider;
    public Vector3 velocity = new Vector3(0f, 10f, 0f);
    public LayerMask notPlayerMask;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
            
            if (!DetectCollision(velocity)) {
                transform.position += velocity;
            }
        }
    }

    /*
    * First we have to cast down if we're going to collide against something within the next frame  
    */

    private bool DetectCollision(Vector3 dir) {

        Ray ray = new Ray(transform.position, dir.normalized);
        RaycastHit rayHit;
        if(Physics.SphereCast(ray, capsuleCollider.radius, out rayHit, Mathf.Infinity)) {

            if(dir.magnitude > rayHit.distance) {
                return true;
            }
            else {
                return false;
            }
            
            
        }
        else {
            return false;
        }
    }
}
