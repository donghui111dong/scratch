
namespace DH.Controller.IconFactory;

public class ConBoardFactory(ILogger<ConBoard> Logger,
                             IProtocol IProto) : IConBoardFactory
{
    public IConBoard CreateIConBoard(ControlEntity ControlPar)
    {
        return new ConBoard(Logger, IProto, ControlPar);
    }
}
