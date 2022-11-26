using Google.Protobuf;
using System;
using System.IO;

public static class ProtobufHelper
{
    public static byte[] ToBytes(object message)
    {
        return ((Google.Protobuf.IMessage)message).ToByteArray();
    }

    public static void ToStream(object message, MemoryStream stream)
    {
        ((Google.Protobuf.IMessage)message).WriteTo(stream);
    }

    public static object FromBytes(Type type, byte[] bytes, int index, int count)
    {
        object message = Activator.CreateInstance(type);
        return FromBytes(message, bytes, index, count);
    }

    public static object FromBytes(object instance, byte[] bytes, int index, int count)
    {
        object message = instance;
        ((Google.Protobuf.IMessage)message).MergeFrom(bytes, index, count);
        return message;
    }

    public static object FromStream(Type type, MemoryStream stream)
    {
        object message = Activator.CreateInstance(type);
        return FromStream(message, stream);
    }

    public static object FromStream(object message, MemoryStream stream)
    {
        ((Google.Protobuf.IMessage)message).MergeFrom(stream.GetBuffer(), (int)stream.Position, (int)(stream.Length) - (int)(stream.Position));
        return message;
    }
}