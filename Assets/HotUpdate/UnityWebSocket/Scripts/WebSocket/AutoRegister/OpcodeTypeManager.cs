using System;
using System.Collections.Generic;

public class OpcodeTypeManager : Singleton<OpcodeTypeManager>
{
    private readonly DoubleMap<int, Type> opcodeTypes = new DoubleMap<int, Type>();
    private readonly Dictionary<int, object> typeMessages = new Dictionary<int, object>();

    public void Init()
    {
        this.opcodeTypes.Clear();
        this.typeMessages.Clear();

        var types = MessageAttribute.GetMessageTypes(Reflection.GetExecutingAssembly());
        foreach (var type in types)
        {
            var attrs = type.GetCustomAttributes(typeof(MessageAttribute), false);
            if (attrs.Length == 0)
                continue;
            var messageAttribute = attrs[0] as MessageAttribute;
            if (messageAttribute == null)
                continue;
            this.opcodeTypes.Add(messageAttribute.Opcode, type);
            this.typeMessages.Add(messageAttribute.Opcode, Activator.CreateInstance(type));
        }
    }

    public int GetOpcode(Type type)
    {
        return this.opcodeTypes.GetKeyByValue(type);
    }

    public Type GetType(int opcode)
    {
        return this.opcodeTypes.GetValueByKey(opcode);
    }

    public object GetInstance(int opcode)
    {
        return this.typeMessages[opcode];
    }

    public void Dispose()
    {
        this.opcodeTypes.Clear();
        this.typeMessages.Clear();
    }
}
