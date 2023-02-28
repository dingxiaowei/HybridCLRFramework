using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ResponseAttribute : Attribute
{
    protected int[] MsgId { get; private set; }
    public ResponseAttribute(params int[] id)
    {
        this.MsgId = id;
    }

    //public static IEnumerable<MethodInfo> GetResponseAttributeMethod(Type type)
    //{
    //    //return from m in GetResponseMethod(type) where m.IsStatic select m; //可以做筛查条件限制
    //    return GetResponseMethod(type);
    //}

    //private static IEnumerable<MethodInfo> GetResponseMethod(Type targetType)
    //{
    //    return
    //        from method in Reflection.GetRuntimeMethods(targetType)
    //        where IsDefined(method)
    //        select method;
    //}

    //private static bool IsDefined(MethodInfo method)
    //{
    //    return Attribute.IsDefined(method, typeof(ResponseAttribute));
    //}

    //public static int[] GetMsgIds(MethodInfo method)
    //{
    //    var attr = method.GetCustomAttribute(typeof(ResponseAttribute));
    //    if (attr != null)
    //    {
    //        var msgIds = (attr as ResponseAttribute).MsgId;
    //        return msgIds;
    //    }
    //    return null;
    //}

    //public static IEnumerable<MethodInfo> GetResponseMethod(Assembly assembly)
    //{
    //    return (
    //            from type in assembly.GetAllTypes()
    //            select GetResponseAttributeMethod(type))
    //            .SelectMany(s => s);
    //}
}
