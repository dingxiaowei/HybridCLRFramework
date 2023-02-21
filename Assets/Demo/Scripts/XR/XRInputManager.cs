using System;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInputManager : MonoBehaviour
{
    public static XRInputManager Instance;

    //左右摇杆
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
    //设置长按按钮的检测时间
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
            Debug.LogError("左摇杆没有设置");
        }
        if (RightController == null)
        {
            Debug.LogError("右摇杆没有设置");
        }
    }

    private void Update()
    {
        //摇杆
        Vector2 leftResult;
        var leftXRControllerSuccess = LeftController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftResult);
        if (leftXRControllerSuccess)
        {
            if (normalLeft2DAxisValue != Vector2.zero || leftResult != Vector2.zero)
            {
                Debug.Log("按下左摇杆:" + leftResult);
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
                Debug.Log("按下右摇杆:" + rightResult);
                normalRight2DAxisValue = rightResult;
                OnRightPrimary2DAxisValueEvent?.Invoke(rightResult);
            }
        }
        //按钮按键
        bool isADown;
        if (RightController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isADown) && isADown)
        {
            if (!normalADownState)
            {
                Debug.Log("按键按下了主键A");
                OnAButtonClick?.Invoke();
                normalAPressTime = Time.realtimeSinceStartup;
            }
            if (normalAPressTime != 0 && (Time.realtimeSinceStartup - normalAPressTime) > LongPressTime)
            {
                Debug.Log("长按主键A");
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
                Debug.Log("按键按下了主键B");
                OnBButtonClick?.Invoke();
                normalBPressTime = Time.realtimeSinceStartup;
            }
            if (normalBPressTime != 0 && (Time.realtimeSinceStartup - normalBPressTime) > LongPressTime)
            {
                Debug.Log("长按主键B");
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
                Debug.Log("按键按下了主键X");
                OnXButtonClick?.Invoke();
                normalXPressTime = Time.realtimeSinceStartup;
            }
            if (normalXPressTime != 0 && (Time.realtimeSinceStartup - normalXPressTime) > LongPressTime)
            {
                Debug.Log("长按主键X");
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
                Debug.Log("按键按下了主键Y");
                OnYButtonClick?.Invoke();
                normalYPressTime = Time.realtimeSinceStartup;
            }
            if (normalYPressTime != 0 && (Time.realtimeSinceStartup - normalYPressTime) > LongPressTime)
            {
                Debug.Log("长按主键Y");
                OnYButtonLongClick?.Invoke();
            }
            normalYDownState = true;
        }
        else
        {
            normalYDownState = false;
            normalYPressTime = 0;
        }
        //扳机键
        LeftController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float leftGripValue);
        if (leftGripValue > 0)
        {
            Debug.Log($"左边握把Grip按下:{leftGripValue}");
            OnLeftGripEvent?.Invoke(leftGripValue);
        }
        RightController.inputDevice.TryGetFeatureValue(CommonUsages.grip, out float rightGripValue);
        if (rightGripValue > 0)
        {
            Debug.Log($"右边握把Grip按下:{rightGripValue}");
            OnRightGripEvent?.Invoke(rightGripValue);
        }
        //Trigger键
        LeftController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float leftTriggerValue);
        if (leftTriggerValue > 0)
        {
            Debug.Log($"左边Trigger按下:{leftGripValue}");
            OnLeftTriggerEvent?.Invoke(leftGripValue);
        }
        RightController.inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float rightTriggerValue);
        if (rightTriggerValue > 0)
        {
            Debug.Log($"右边Trigger按下:{rightTriggerValue}");
            OnRightTriggerEvent?.Invoke(rightTriggerValue);
        }

        //InputHelpers.IsPressed(LeftController.inputDevice, InputHelpers.Button.Grip, out bool isPressed, 0.8f);
        //if (isPressed)
        //{
        //    Debug.LogError($"第二种方法按下去大于0.8f");
        //}
    }
}
