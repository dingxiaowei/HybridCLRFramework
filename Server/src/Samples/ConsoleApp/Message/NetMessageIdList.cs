using System;
using System.Collections.Generic;
using ServerDemo;

namespace Protoc
{
    public sealed partial class NetMessage
    {
        public void Reset()
        {
            this.Content = Google.Protobuf.ByteString.Empty;
            this.Type = 0;
        }
    }
    public sealed partial class Person
    {
        public void Reset() { }
    }
}

public class NetMessageIdList : Singleton<NetMessageIdList>
{
    private Dictionary<int, Type> mIdToType = new Dictionary<int, Type>();
    private Dictionary<Type, int> mTypeToId = new Dictionary<Type, int>();
    public NetMessageIdList()
    {
        mIdToType[1] = typeof(Protoc.Person);
        mTypeToId[typeof(Protoc.Person)] = 1;
    }

    //public static Type MsgIdToType(int id)
    //{
    //    Type t = null;
    //    sInstance.mIdToType.TryGetValue(id, out t);
    //    return t;
    //}

    //public static int TypeToMsgId(Type type)
    //{
    //    int msgId = 0;
    //    sInstance.mTypeToId.TryGetValue(type, out msgId);
    //    return msgId;
    //}
}