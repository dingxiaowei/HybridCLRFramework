using UnrealM;
using System;
using UnityEngine;

public class ActionSequenceHandleExample : MonoBehaviour
{
    private void Start()
    {
        ActionSequenceSystem.Delayer(5, () => Debug.Log("No id delayer"));
        ActionSequenceSystem.Looper(0.2f, 10, false, () => Debug.Log("No id looper"));

        //Notes£ºAn instance must be preserved to manually stop an infinite loop sequence.
        ActionSequenceHandle infiniteSequenceHandle = new ActionSequenceHandle();
        ActionSequenceSystem.Looper(0.2f, -1, false, () => Debug.Log("No id infinite looper"));
        infiniteSequenceHandle.StopSequence();
    }
}