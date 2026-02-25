public interface ITickable { }

public interface UPDATE : ITickable
{
    void UpdateTick();
}

public interface FIXEDUPDATE : ITickable
{
    void FixedTick();
}

public interface LATEUPDATE : ITickable
{
    void LateTick();
}