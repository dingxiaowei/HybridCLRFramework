using Google.Protobuf;
using System;

namespace ServerDemo
{
    public static class ProtobufHelper
    {
        public static byte[] ToBytes(object message)
        {
            return ((IMessage)message).ToByteArray();
        }

        public static object FromBytes(Type type, byte[] bytes, int index, int count)
        {
            object message = Activator.CreateInstance(type);
            ((IMessage)message).MergeFrom(bytes, index, count);
            return message;
        }

        public static object FromBytes(object instance, byte[] bytes, int index, int count)
        {
            object message = instance;
            ((IMessage)message).MergeFrom(bytes, index, count);
            return message;
        }
    }
}
