using Protoc;
using ServerDemo;
using System;
using System.Collections.Generic;
using System.Reflection;

public class MessageDispatcher : Singleton<MessageDispatcher>
{
    protected const int MAX_DISPATCH_MESSAGE_COUNT_PER_FRAME = 16;
    protected Dictionary<int, Action<object>> mMessageHandlers = new Dictionary<int, Action<object>>();
    protected Dictionary<int, List<MethodInfo>> mMessageMethods = new Dictionary<int, List<MethodInfo>>();
    protected Queue<NetMessage> mMessageFrontQueue = new Queue<NetMessage>();
    protected Queue<NetMessage> mMessageBackQueue = new Queue<NetMessage>();
    protected Type mResponseAttrType = typeof(ResponseAttribute);


    //    public void ResponseAutoRegister()
    //    {
    //        foreach (var method in ResponseAttribute.GetResponseMethod(Reflection.GetExecutingAssembly()))
    //        {
    //            var msgIds = ResponseAttribute.GetMsgIds(method);
    //            if (msgIds != null)
    //            {
    //                for (int i = 0; i < msgIds.Length; i++)
    //                {
    //                    RegisterMethod(msgIds[i], method);
    //#if DEBUG_NETWORK
    //                    UnityEngine.Debug.Log($"注册消息:{msgIds[i]}  函数名:{method.Name}");
    //#endif
    //                }
    //            }
    //        }
    //    }

    //public void RegisterOnMessageReceived<T>(Action<object> handler) where T : IMessage
    //{
    //    var msgId = NetMessageIdList.TypeToMsgId(typeof(T));
    //    if (msgId != 0)
    //    {
    //        RegisterHandler(msgId, handler);
    //    }
    //}

    //public void UnregisterOnMessageReceived<T>(Action<object> handler) where T : IMessage
    //{
    //    var msgId = NetMessageIdList.TypeToMsgId(typeof(T));
    //    if (msgId != 0)
    //    {
    //        UnregisterHandler(msgId, handler);
    //    }
    //}

    public void ReceiveMessage(NetMessage packet)
    {
        var msgId = packet.Type;
        if (msgId != 0)
        {
            lock (mMessageBackQueue)
            {
                mMessageBackQueue.Enqueue(packet);
            }
        }
    }

    public void DispatchMessages()
    {
        int dispatchedMessageCount = 0;
        while (mMessageFrontQueue.Count > 0)
        {
            NetMessage packet = mMessageFrontQueue.Dequeue();
            DispatchMessage(packet);
            //新的方式
            Handler(packet);
            dispatchedMessageCount++;

            if (dispatchedMessageCount >= MAX_DISPATCH_MESSAGE_COUNT_PER_FRAME)
            {
                return;
            }
        }

        //swap messageQueue
        lock (mMessageBackQueue)
        {
            Queue<NetMessage> temp = mMessageBackQueue;
            mMessageBackQueue = mMessageFrontQueue;
            mMessageFrontQueue = temp;
        }
    }

    private void RegisterMethod(int msgId, MethodInfo method)
    {
        if (method != null)
        {
            if (mMessageMethods.ContainsKey(msgId))
            {
                mMessageMethods[msgId].Add(method);
            }
            else
            {
                var methodList = new List<MethodInfo>();
                methodList.Add(method);
                mMessageMethods.Add(msgId, methodList);
            }
        }
    }

    public void RegisterHandler(int msgId, Action<object> handler)
    {
        if (handler != null)
        {
            if (mMessageHandlers.ContainsKey(msgId))
            {
                mMessageHandlers[msgId] -= handler;
                mMessageHandlers[msgId] += handler;
            }
            else
            {
                mMessageHandlers.Add(msgId, handler);
            }
        }
    }

    protected void UnregisterHandler(int msgId, Action<object> handler)
    {
        if (handler != null)
        {
            if (mMessageHandlers.ContainsKey(msgId))
            {
                mMessageHandlers[msgId] -= handler;
            }
        }
    }

    protected void DispatchMessage(NetMessage packet)
    {
        int msgId = packet.Type;

        Action<object> callbackListeners;
        if (mMessageHandlers.TryGetValue(msgId, out callbackListeners))
        {
            callbackListeners?.Invoke(packet.Content);
        }

        List<MethodInfo> methods;
        if (mMessageMethods.TryGetValue(msgId, out methods))
        {
            foreach (var method in methods)
            {
                var type = method.ReflectedType;
                var obj = Activator.CreateInstance(type);
                method.Invoke(obj, new object[] { packet.Content });
            }
        }
    }

    public void Dispose()
    {
        this.mMessageHandlers.Clear();
        this.mMessageMethods.Clear();
        this.mMessageFrontQueue.Clear();
        this.mMessageBackQueue.Clear();

        this.handlers.Clear();
        //OpcodeTypeManager.sInstance.Dispose();
    }



    //另外一种自动注册的实现方式
    protected readonly Dictionary<int, List<IMHandler>> handlers = new Dictionary<int, List<IMHandler>>();

    void RegisterHandler(int opcode, IMHandler handler)
    {
        if (!this.handlers.ContainsKey(opcode))
        {
            this.handlers.Add(opcode, new List<IMHandler>());
        }
        this.handlers[opcode].Add(handler);
    }

    protected void Handler(NetMessage msg)
    {
        List<IMHandler> actions;
        if (!this.handlers.TryGetValue(msg.Type, out actions))
        {
            Console.WriteLine($"消息 {msg.Type} 没有处理");
            return;
        }

        foreach (IMHandler ev in actions)
        {
            try
            {
                ev.Handle(msg.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    //public void AutoRegistHandlers()
    //{
    //    //OpcodeTypeManager.sInstance.Init();
    //    this.handlers.Clear();

    //    var types = MessageHandlerAttribute.GetMessageHandlerTypes(Reflection.GetExecutingAssembly());
    //    foreach (var type in types)
    //    {
    //        var attrs = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);
    //        if (attrs.Length == 0)
    //            continue;
    //        var imHandler = Activator.CreateInstance(type) as IMHandler;
    //        if (imHandler == null)
    //        {
    //            Console.WriteLine($"message handle {type.Name} 需要继承 IMHandler");
    //            continue;
    //        }
    //        var messageType = imHandler.GetMessageType();
    //        int opcode = OpcodeTypeManager.sInstance.GetOpcode(messageType);
    //        if (opcode != 0)
    //        {
    //            this.RegisterHandler(opcode, imHandler);
    //        }
    //    }
    //}
}