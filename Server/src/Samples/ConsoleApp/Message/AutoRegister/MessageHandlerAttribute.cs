
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class MessageHandlerAttribute : BaseAttribute
{
    public MessageHandlerAttribute() { }

    //public static IEnumerable<Type> GetMessageHandlerTypes(Assembly assembly)
    //{
    //    return (
    //            from type in assembly.GetAllTypes()
    //            where type.IsDefined(typeof(MessageHandlerAttribute))
    //            select type)
    //            .ToList();
    //    //List<Type> typeList = new List<Type>();
    //    //var types = assembly.GetAllTypes();
    //    //var messageHandlerType = typeof(MessageHandlerAttribute);
    //    //foreach (var type in types)
    //    //{
    //    //    if (type.IsDefined(messageHandlerType, false))
    //    //        typeList.Add(type);
    //    //}
    //    //return typeList;
    //}
}
