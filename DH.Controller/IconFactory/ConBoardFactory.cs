namespace DH.Controller.IconFactory;

public class ConBoardFactory(ILogger<ConBoard> Logger,
                             IProtocol IProto,
                             ICommandPublisher ComPublisher) : IConBoardFactory
{
    public IConBoard CreateIConBoard(CommEntity CommPar, ControlEntity ControlPar)
    {
        return new ConBoard(Logger, IProto, CommPar, ControlPar, ComPublisher);
    }
}
