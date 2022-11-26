public struct MessageInfo
{
    public int Opcode { get; }
    public object Message { get; }

    public MessageInfo(int opcode, object message)
    {
        this.Opcode = opcode;
        this.Message = message;
    }
}

