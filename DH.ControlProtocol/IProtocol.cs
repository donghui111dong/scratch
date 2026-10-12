namespace DH.ControlProtocol;

public interface IProtocol
{
    void BuildWorkStatusPacket(Span<byte> OrderPacket, byte OrderType, ushort DataLength = 0x00, ushort PacketSequence = 0x00);
}
