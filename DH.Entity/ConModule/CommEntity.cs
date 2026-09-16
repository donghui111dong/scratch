namespace DH.Entity.ConModule;

public class CommEntity
{
    public string LocalIP { get; set; } = default!;

    public string EncoderAdapter { get; set; } = default!;

    public int MaxRetryTimeout { get; set; }

    public bool ErasureMemory { get; set; }

    public int ExecInterval { get; set; }

    public int ExecTimeout { get; set; }
}
