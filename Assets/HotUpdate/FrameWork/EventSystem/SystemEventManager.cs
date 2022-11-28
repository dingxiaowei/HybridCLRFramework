using Google.Protobuf;
using System.Collections;
using System.Collections.Generic;

public class SystemEventBase : System.EventArgs
{
    public EventType EventType;
    public int ErrorCode = 0;
    public string ErrorMsg = string.Empty;
}

public class DisconnectEvent : SystemEventBase
{
    public bool IsManuallyDisconnect = false;
    public int ServerType = 0;
}

public class PlayAudioEvent : SystemEventBase
{
    public byte[] audioBytes;
}

public class SystemEventManager : ManagerBase<SystemEventManager>
{
    private Queue<SystemEventBase> mEventQueue = new Queue<SystemEventBase>();
    private Dictionary<int, System.Action<SystemEventBase>> mEventMap = new Dictionary<int, System.Action<SystemEventBase>>();

    public override void Update()
    {
        UpdateEventQueue();
    }

    public void RegisterEvent(EventType eventType, System.Action<SystemEventBase> eventListener)
    {
        int eventT = (int)eventType;
        if (mEventMap.ContainsKey(eventT))
        {
            mEventMap[eventT] += eventListener;
        }
        else
        {
            mEventMap.Add(eventT, eventListener);
        }
    }

    public void UnregisterEvent(EventType eventType, System.Action<SystemEventBase> eventListener)
    {
        int eventT = (int)eventType;
        if (mEventMap.ContainsKey(eventT))
        {
            mEventMap[eventT] -= eventListener;
        }
    }

    protected void UpdateEventQueue()
    {
        while (mEventQueue.Count > 0)
        {
            SystemEventBase evb = mEventQueue.Dequeue();
            System.Action<SystemEventBase> eventListeners;
            if (mEventMap.TryGetValue((int)evb.EventType, out eventListeners))
            {
                if (eventListeners != null)
                    eventListeners(evb);
            }
        }
    }

    public void RaiseEvent(EventType eventType, SystemEventBase eventArg)
    {
        mEventQueue.Enqueue(eventArg);
    }
}
