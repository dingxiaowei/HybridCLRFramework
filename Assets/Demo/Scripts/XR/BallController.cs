using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class BallController : MonoBehaviour
{
    public XRController rightController;

    void Update()
    {
        Vector2 result;
        var success = rightController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out result);
        if (success)
        {
            var position = transform.position;
            transform.position = new Vector3(position.x - result.x * Time.deltaTime, position.y, position.z + result.y * Time.deltaTime);
        }
    }
}
