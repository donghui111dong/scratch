namespace DH.Entity.ConModule;

public class ConEntity
{
    public CommEntity Comm { get; set; } = default!;

    public List<ControlEntity> Control { get; set; } = default!;
}
