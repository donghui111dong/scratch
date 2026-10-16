namespace DH.ControlProtocol.ProtocolClass;

public enum CommandType : byte
{
    HeadFront = 0xEB,

    HeadBack = 0x90,

    ReadOrder = 0x52,

    WriteOrder = 0x57,

    WorkStatus = 0x0C,

    StartHD = 0x04,

    StopHD = 0x05,

    ReadData = 0x5F,

    ExceOver = default
}
