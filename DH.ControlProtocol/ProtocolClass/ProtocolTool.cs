namespace DH.ControlProtocol.ProtocolClass;

public class ProtocolTool : IProtocol
{
    public void BuildWorkStatusPacket(Span<byte> OrderPacket, byte OrderType, ushort DataLength = 0x00, ushort PacketSequence = 0x00)
    {
        OrderPacket[0] = (byte)CommandType.HeadFront;

        OrderPacket[1] = (byte)CommandType.HeadBack;

        OrderPacket[2] = (byte)CommandType.ReadOrder;

        OrderPacket[3] = (byte)CommandType.WorkStatus;

        OrderPacket[4] = 0x00;

        BinaryPrimitives.TryWriteUInt16BigEndian(OrderPacket[5..7], DataLength);

        BinaryPrimitives.TryWriteUInt16BigEndian(OrderPacket[7..9], 0x00);

        BinaryPrimitives.TryWriteUInt16BigEndian(OrderPacket[9..11], PacketSequence);

        OrderPacket[^1] = (byte)(256 - OrderPacket[0..^1].CheckSum() & 0xFF);
    }
}
