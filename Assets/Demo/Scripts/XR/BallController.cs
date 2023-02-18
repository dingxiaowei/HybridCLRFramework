using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class BallController : MonoBehaviour
{
    public XRController rightController;
    public float speed = 2.5f;
    public XRInputManager xrInputController;
    private void Start()
    {
        xrInputController.OnRightPrimary2DAxisValueEvent += OnRightPrimary2DAxisValue;
    }

    //void Update()
    //{
    //    Vector2 result;
    //    var success = rightController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out result);
    //    if (success)
    //    {
    //        var position = transform.position;
    //        transform.position = new Vector3(position.x + speed * result.x * Time.deltaTime, position.y, position.z + speed * result.y * Time.deltaTime);
    //    }
    //}

    void OnRightPrimary2DAxisValue(Vector2 result)
    {
        var position = transform.position;
        transform.position = new Vector3(position.x + speed * result.x * Time.deltaTime, position.y, position.z + speed * result.y * Time.deltaTime);
    }
}
