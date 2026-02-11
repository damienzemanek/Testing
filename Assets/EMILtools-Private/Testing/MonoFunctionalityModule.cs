
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
    public abstract void SetupModule();
    protected virtual void ExecuteTemplateCall(float dt) { }
    public abstract void Bind();
    public abstract void Unbind();
}