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

        //��ߵİ������ֵ
        /*
        rightController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float floatValue);
        if (floatValue > 0.8f)
        {
            Debug.LogError($"�հѼ����´���0.8f  ʵ��ֵ�Ƕ���:{floatValue}");
        }

        InputHelpers.IsPressed(rightController.inputDevice, InputHelpers.Button.Grip, out bool isPressed, 0.8f);
        if (isPressed)
        {
            Debug.LogError($"�ڶ��ַ�������ȥ����0.8f");
        }

        InputHelpers.IsPressed(rightController.inputDevice, InputHelpers.Button.PrimaryButton, out bool isButtonDown);
        if (isButtonDown)
        {
            Debug.LogError("��ťA����");
        }
        */
        //��ť����
        //bool isDown; //��¼�Ƿ�������ť
        //if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isDown) && isDown)
        //{
        //    Debug.Log("��������������A");
        //}

        //bool isBDown; //��¼�Ƿ����˸���ť
        //if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out isBDown) && isBDown)
        //{
        //    Debug.Log("���������˸���B");
        //}
    }
}
