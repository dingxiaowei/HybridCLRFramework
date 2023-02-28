
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class MessageAttribute : BaseAttribute
{
    public int Opcode { get; }
    public MessageAttribute(int opcode)
    {
        this.Opcode = opcode;
    }

    //public static IEnumerable<Type> GetMessageTypes(Assembly assembly)
    //{
    //    return (
    //            from type in assembly.GetAllTypes()
    //            where type.IsDefined(typeof(MessageAttribute))
    //            select type)
    //            .ToList();
    //}
}
