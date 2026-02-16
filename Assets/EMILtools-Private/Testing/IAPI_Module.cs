public interface IAPI_Module { }

public interface IAPI_Dependant<T>
{
    void SendDependencies(T dependencies) => GrabDependancies(dependencies);
    protected void GrabDependancies(T context);

}