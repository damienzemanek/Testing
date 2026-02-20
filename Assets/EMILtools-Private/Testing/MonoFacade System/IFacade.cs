public interface IFacade
{
    public FacadeInterfaceContext context { get; set; }
    
}

public readonly struct FacadeInterfaceContext
{
    public readonly IBlackboard Blackboard;
}