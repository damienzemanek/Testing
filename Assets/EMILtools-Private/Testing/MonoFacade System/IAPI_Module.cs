public interface IAPI_Module { }

public interface IAPI_Dependant<T> : IAPI_Module
{
    void SendDependencies(T dependencies) => GrabDependancies(dependencies);
    protected void GrabDependancies(T context);

}

public interface IAPI_Spawn : IAPI_Module
{
    public void Spawn();
}