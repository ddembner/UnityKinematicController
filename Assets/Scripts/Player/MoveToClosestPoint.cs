using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToClosestPoint : MonoBehaviour {

    Vector3 closestPoint;
    Vector3 clostPointOnCapsule;
    Vector3 pointDir;
    public Transform moveToSphere;
    public LayerMask allButThisObject;

    float distanceToPoint;
    Vector3 point1;
    Vector3 point2;
    float radius;
    public CapsuleCollider capsuleCollider;
    void Start()
    {
        distanceToPoint = capsuleCollider.height / 2 - capsuleCollider.radius;
        point1 = (capsuleCollider.center + Vector3.up * distanceToPoint);
        point2 = (capsuleCollider.center - Vector3.up * distanceToPoint);
        radius = capsuleCollider.radius;
    }

    // Update is called once per frame
    void Update()
    {
        closestPoint = FindClosestPoint();

        if (Input.GetKeyDown(KeyCode.Space)) {

            //transform.position = closestPoint + pointDir * (capsuleCollider.radius + Vector3.Dot(Vector3.Normalize(clostPointOnCapsule - moveToSphere.position), moveToSphere.up));
            Vector3 normalVec = Vector3.Normalize(clostPointOnCapsule - moveToSphere.position);
            float dot = Vector3.Dot(normalVec, moveToSphere.up);
            transform.position = closestPoint + pointDir * (capsuleCollider.radius + (dot * capsuleCollider.radius));
            if(transform.position.x < 0.0000001f || transform.position.x > -0.0000001f) {
                transform.position = new Vector3(0f, transform.position.y, transform.position.z);
            }
        }
        //Debug.Log(Vector3.Dot(Vector3.Normalize(clostPointOnCapsule - moveToSphere.position), moveToSphere.up));
    }

    Vector3 FindClosestPoint() {

        Vector3 dir = moveToSphere.position - transform.position;
        dir = dir.normalized;
        RaycastHit hit;
        if (Physics.CapsuleCast(point1 + transform.position, point2 + transform.position, radius, dir, out hit, Mathf.Infinity, allButThisObject)) {
            pointDir = hit.normal;
            clostPointOnCapsule = capsuleCollider.ClosestPoint(hit.point);
            return hit.point;
        }
        
        return Vector3.zero;
    }

    private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(closestPoint, 0.5f);
        Gizmos.DrawSphere(clostPointOnCapsule, 0.2f);
        Gizmos.DrawLine(closestPoint, closestPoint + pointDir);
    }
}
