using System;
public enum MessageTypes : short
{
    ConnectionRequest = Int16.MaxValue - 1,
    KeepAlive = Int16.MaxValue,
}
