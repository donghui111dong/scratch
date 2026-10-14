namespace DH.Entity.ConModule;

public record PacketBytesForBsr
(
    IMemoryOwner<byte> MemoryOwner,

    SocketReceiveMessageFromResult Message
)
{
    public PacketBytesForBsr() : this(default!, default) { }
}
