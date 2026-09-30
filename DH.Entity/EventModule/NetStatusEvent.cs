namespace DH.Entity.EventModule;

public record NetStatusEvent
(
    NetStatusType ChangeType,

    bool NetKeep = false
);

public enum NetStatusType
{
    IPChanged,
    NetKeepChanged
}
