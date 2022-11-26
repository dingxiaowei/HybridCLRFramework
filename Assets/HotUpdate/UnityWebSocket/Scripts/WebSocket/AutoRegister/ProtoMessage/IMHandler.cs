using System;

public interface IMHandler
{
    void Handle(object message);
    Type GetMessageType();
}