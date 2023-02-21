using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInputManager : MonoBehaviour
{
    public static XRInputManager Instance;

    //����ҡ��
    public XRController LeftController;
    public XRController RightController;

    public Action<Vector2> OnLeftPrimary2DAxisValueEvent;
    public Action<Vector2> OnRightPrimary2DAxisValueEvent;
    public Action<float> OnLeftGripEvent;
    public Action<float> OnRightGripEvent;
    public Action<float> OnLeftTriggerEvent;
    public Action<float> OnRightTriggerEvent;
    public Action OnAButtonClick;
    public Action OnAButtonLongClick;
    public Action OnBButtonClick;
    public Action OnBButtonLongClick;
    public Action OnXButtonClick;
    public Action OnXButtonLongClick;
    public Action OnYButtonClick;
    public Action OnYButtonLongClick;
    //���ó�����ť�ļ��ʱ��
    public float LongPressTime = 0.2f;
    private Vector2 normalLeft2DAxisValue = Vector2.zero;
    private Vector2 normalRight2DAxisValue = Vector2.zero;
    private bool normalADownState = false;
    private float normalAPressTime = 0;
    private bool normalBDownState = false;
    private float normalBPressTime = 0;
    private bool normalXDownState = false;
    private float normalXPressTime = 0;
    private bool normalYDownState = false;
    private float normalYPressTime = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (LeftController == null)
        {
            Debug.LogError("��ҡ��û������");
        }
        if (RightController == null)
        {
            Debug.LogError("��ҡ��û������");
        }
    }

    private void Update()
    {
        //ҡ��
        Vector2 leftResult;
        var leftXRControllerSuccess = LeftController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftResult);
        if (leftXRControllerSuccess)
        {
            if (normalLeft2DAxisValue != Vector2.zero || leftResult != Vector2.zero)
            {
                Debug.Log("������ҡ��:" + leftResult);
                normalLeft2DAxisValue = leftResult;
                OnLeftPrimary2DAxisValueEvent?.Invoke(leftResult);
            }
        }
        Vector2 rightResult;
        var rightXRControllerSuccess = RightController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightResult);
        if (rightXRControllerSuccess)
        {
            if (normalRight2DAxisValue != Vector2.zero || rightResult != Vector2.zero)
            {
                Debug.Log("������ҡ��:" + rightResult);
                normalRight2DAxisValue = rightResult;
                OnRightPrimary2DAxisValueEvent?.Invoke(rightResult);
            }
        }
        //��ť����
        bool isADown;
        if (RightController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isADown) && isADown)
        {
            if (!normalADownState)
            {
                Debug.Log("��������������A");
                OnAButtonClick?.Invoke();
                normalAPressTime = Time.realtimeSinceStartup;
            }
            if (normalAPressTime != 0 && (Time.realtimeSinceStartup - normalAPressTime) > LongPressTime)
            {
                Debug.Log("��������A");
                OnAButtonLongClick?.Invoke();
            }
            normalADownState = true;
        }
        else
        {
            normalADownState = false;
            normalAPressTime = 0;
        }

        bool isBDown;
        if (RightController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out isBDown) && isBDown)
        {
            if (!normalBDownState)
            {
                Debug.Log("��������������B");
                OnBButtonClick?.Invoke();
                normalBPressTime = Time.realtimeSinceStartup;
            }
            if (normalBPressTime != 0 && (Time.realtimeSinceStartup - normalBPressTime) > LongPressTime)
            {
                Debug.Log("��������B");
                OnBButtonLongClick?.Invoke();
            }
            normalBDownState = true;
        }
        else
        {
            normalBDownState = false;
            normalBPressTime = 0;
        }
        bool isXDown;
        if (LeftController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isXDown) && isXDown)
        {
            if (!normalXDownState)
            {
                Debug.Log("��������������X");
                OnXButtonClick?.Invoke();
                normalXPressTime = Time.realtimeSinceStartup;
            }
            if (normalXPressTime != 0 && (Time.realtimeSinceStartup - normalXPressTime) > LongPressTime)
            {
                Debug.Log("��������X");
                OnXButtonLongClick?.Invoke();
            }
            normalXDownState = true;
        }
        else
        {
            normalXDownState = false;
            normalXPressTime = 0;
        }
        bool isYDown;
        if (LeftController.inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out isYDown) && isYDown)
        {
            if (!normalYDownState)
            {
                Debug.Log("��������������Y");
                OnYButtonClick?.Invoke();
                normalYPressTime = Time.realtimeSinceStartup;
            }
            if (normalYPressTime != 0 && (Time.realtimeSinceStartup - normalYPressTime) > LongPressTime)
            {
                Debug.Log("��������Y");
                OnYButtonLongClick?.Invoke();
            }
            normalYDownState = true;
        }
        else
        {
            normalYDownState = false;
            normalYPressTime = 0;
        }
        //�����
        LeftController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float leftGripValue);
        if (leftGripValue > 0)
        {
            Debug.Log($"����հ�Grip����:{leftGripValue}");
            OnLeftGripEvent?.Invoke(leftGripValue);
        }
        RightController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float rightGripValue);
        if (rightGripValue > 0)
        {
            Debug.Log($"�ұ��հ�Grip����:{rightGripValue}");
            OnRightGripEvent?.Invoke(rightGripValue);
        }
        //Trigger��
        LeftController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue);
        if (leftTriggerValue > 0)
        {
            Debug.Log($"���Trigger����:{leftGripValue}");
            OnLeftTriggerEvent?.Invoke(leftGripValue);
        }
        RightController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue);
        if (rightTriggerValue > 0)
        {
            Debug.Log($"�ұ�Trigger����:{rightTriggerValue}");
            OnRightTriggerEvent?.Invoke(rightTriggerValue);
        }

        //InputHelpers.IsPressed(LeftController.inputDevice, InputHelpers.Button.Grip, out bool isPressed, 0.8f);
        //if (isPressed)
        //{
        //    Debug.LogError($"�ڶ��ַ�������ȥ����0.8f");
        //}
    }
}
