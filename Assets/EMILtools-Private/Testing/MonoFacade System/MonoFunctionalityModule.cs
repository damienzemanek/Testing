using Sirenix.OdinInspector;
using UnityEngine;

public interface IModule { }

public interface IModuleTick { }

public interface UPDATE : IModuleTick
{
    void OnUpdateTick(float dt);
}

public interface FIXEDUPDATE : IModuleTick
{
    void OnFixedTick(float dt);
}

public interface LATEUPDATE : IModuleTick
{
    void LateTick(float dt);
}

public abstract class MonoFunctionalityModule : IModule
{
    [Title("$Name"), PropertyOrder(-1)]
    [ShowInInspector] public string Name => "Module: " + this.GetType().Name;
    public abstract void SetupModule();
    protected virtual void ExecuteTemplateCall(float dt) { }
    public abstract void Bind();
    public abstract void Unbind();
}