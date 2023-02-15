using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class BallController : MonoBehaviour
{
    public XRController rightController;
    public float speed = 2.5f;

    void Update()
    {
        Vector2 result;
        var success = rightController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out result);
        if (success)
        {
            var position = transform.position;
            transform.position = new Vector3(position.x + speed * result.x * Time.deltaTime, position.y, position.z + speed * result.y * Time.deltaTime);
        }

        //侧边的扳机键的值
        /*
        rightController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float floatValue);
        if (floatValue > 0.8f)
        {
            Debug.LogError($"握把键按下大于0.8f  实际值是多少:{floatValue}");
        }

        InputHelpers.IsPressed(rightController.inputDevice, InputHelpers.Button.Grip, out bool isPressed, 0.8f);
        if (isPressed)
        {
            Debug.LogError($"第二种方法按下去大于0.8f");
        }

        InputHelpers.IsPressed(rightController.inputDevice, InputHelpers.Button.PrimaryButton, out bool isButtonDown);
        if (isButtonDown)
        {
            Debug.LogError("按钮A按下");
        }
        */
        //按钮按键
        //bool isDown; //记录是否按下主按钮
        //if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isDown) && isDown)
        //{
        //    Debug.Log("按键按下了主键A");
        //}

        //bool isBDown; //记录是否按下了副按钮
        //if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out isBDown) && isBDown)
        //{
        //    Debug.Log("按键按下了副键B");
        //}
    }
}
