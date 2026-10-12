namespace DH.ControlProtocol;

public static class SpanForByteExtensions
{
    public static byte CheckSum(this Span<byte> PacketData)
    {
        byte Value = 0x00;

        foreach (byte SumItem in PacketData)
        {
            Value += SumItem;
        }

        return Value;
    }
}
