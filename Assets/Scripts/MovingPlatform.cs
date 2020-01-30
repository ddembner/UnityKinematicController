using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour {

    [SerializeField] private Vector3 rotateDir;
    [SerializeField] private float rotateSpeed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {

        transform.Rotate(rotateDir * rotateSpeed);
    }

}
