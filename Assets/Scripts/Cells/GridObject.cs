using UnityEngine;

public enum ObjectCategory
{
    StaticEnvironment,
    PushableBox
}

public abstract class GridObject : MonoBehaviour
{
    public abstract ObjectCategory category { get; }
}