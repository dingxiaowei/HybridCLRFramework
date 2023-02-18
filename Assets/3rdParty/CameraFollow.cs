using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float speed = 5;
    Vector3 offeset;

    // Start is called before the first frame update
    void Start()
    {
        offeset = transform.position - target.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var targetVect = target.position + offeset;
        var LerpVect = Vector3.Lerp(transform.position, targetVect, Time.deltaTime * speed);
        transform.position = LerpVect;
    }
}
