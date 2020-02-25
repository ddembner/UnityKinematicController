using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    [Header("Rotate")]
    public bool shouldRotate = false;
    [SerializeField] private Vector3 rotateDir;
    [SerializeField] private float rotateSpeed;

    [Header("Translate")]
    public bool shouldTranslate = false;
    public float distance = 0f;
    public float speed = 0f;
    public Vector3 dir;
    private float distanceTravelled = 0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {

        if(shouldRotate) transform.Rotate(rotateDir * rotateSpeed);

        if (shouldTranslate) Translate();
    }

    private void Translate() {

        Vector3 oldPos = transform.position;

        if(distanceTravelled >= distance) {
            dir *= -1f;
            distanceTravelled = 0f;
        }

        transform.position += dir * speed * Time.deltaTime;
        distanceTravelled += Vector3.Distance(oldPos, transform.position);
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("Collided with: " + other.name);
    }

}
