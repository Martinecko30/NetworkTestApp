namespace NetworkTestApp.Shared;

[Flags]
public enum MessageType : byte
{
    None = 0,
    Broadcast = 1
}