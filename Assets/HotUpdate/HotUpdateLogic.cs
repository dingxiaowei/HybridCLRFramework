using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotUpdateLogic : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(Test());
    }

    IEnumerator Test()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("携程等待1帧测试");
        yield return new WaitForSeconds(2);
        Debug.Log("携程等待2秒测试");
    }

    // Update is called once per frame
    void Update()
    {
        //if (Time.frameCount % 500 == 0)
        //{
        //    Debug.Log($"HotUpdateLogic,Ticket curFrameId:{Time.frameCount}");
        //}
    }
}
