namespace DH.ControlProtocol.ProtocolClass;

public enum CommandType : byte
{
    HeadFront = 0xEB,

    HeadBack = 0x90,

    ReadOrder = 0x52,

    WriteOrder = 0x57,

    WorkStatus = 0x0C
}
