using Google.Protobuf;
using System;

public abstract class AMHandler<T> : IMHandler where T : class
{
    //protected abstract void Run(T message);
    protected abstract void Run(ByteString content);
    public Type GetMessageType()
    {
        return typeof(T);
    }

    public void Handle(object msg)
    {
        var message = msg as ByteString;
        if (message == null)
        {
            UnityEngine.Debug.LogError($"消息类型转换错误: {msg.GetType().Name} to {typeof(T).Name}");
        }

        try
        {
            this.Run(message);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }
    }
}
