public class EntitySpawner<T> where T : Entity
{
    IEntityFactory<T> entityFactory;
    ISpawnPointStrategy spawnPointStrategy;
    
    public EntitySpawner(IEntityFactory<T> _entityFactory, ISpawnPointStrategy _spawnPointStrategy)
    {
        entityFactory = _entityFactory;
        spawnPointStrategy = _spawnPointStrategy;
    }
    
    public T Spawn()
    {
        return entityFactory.Create(spawnPointStrategy.NextSpawnPoint());
    }
}