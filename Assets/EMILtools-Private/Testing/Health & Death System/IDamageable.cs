public interface IDamageable
{
    public enum DeathType
    {
        NOT_DEAD,
        Regular,
    }

    public enum DamageLocation
    {
        NOT_BEING_DAMAGED,
        Body,
    }
    
    public enum DamageType
    {
        Bullet,
    }
    
    public struct DamageInfo
    {
        public int dmg;
        public DamageLocation location;
        public DamageType type;
    }
    
    public void TakeDamage(DamageInfo info);
}