namespace DH.Controller.IconFactory;

public interface IConBoardFactory
{
    IConBoard CreateIConBoard(CommEntity CommPar, ControlEntity ControlPar);
}
