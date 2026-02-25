public interface IFacade
{
    public IModuleUsabableContext Context { get; set; }
    // public FacadeInterfaceContext context { get; set; }
    //
    // public TBlackboardType Blackboard<TBlackboardType>() where TBlackboardType : IBlackboard
    //     => (TBlackboardType)context.Blackboard;
    // public TConfigType Config<TConfigType>() where TConfigType : IConfig
    //     => (TConfigType)context.Config;
    // public TFunctionalityType Functionality<TFunctionalityType>( ) where TFunctionalityType : IFunctionality
    //     => (TFunctionalityType)context.Functionality;
}

// public struct FacadeInterfaceContext
// {
//     public readonly IBlackboard Blackboard;
//     public readonly IConfig Config;
//     public readonly IFunctionality Functionality;
//     
//     public FacadeInterfaceContext(IBlackboard _blackboard, IConfig _config, IFunctionality _functionality)
//     {
//         Blackboard = _blackboard;
//         Config = _config;
//         Functionality = _functionality;
//     }
// }