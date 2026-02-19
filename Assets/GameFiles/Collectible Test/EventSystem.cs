using System;
using static CollectibleManager;

public abstract class EventSystem<T> where T : Delegate
{
    
}


public class CollectibleEventSystem : EventSystem<Action<CollectibleManager.Collectible, int>>
{
    public static event Action<CollectibleManager.Collectible, int> OnEvent;
    public static event Action<CollectibleManager.Collectible, int> PostEvent;

    public static void RaiseEvent(CollectibleManager.Collectible type, int amount)
    {
        OnEvent?.Invoke(type, amount);
        PostEvent?.Invoke(type, amount);
    }
}