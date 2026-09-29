using UnityEngine;

public enum ObjectCategory
{
    StaticEnvironment,
    PushableBox
}

public abstract class GridObject : MonoBehaviour
{
    public CellKind kind;
    public abstract ObjectCategory category { get; }

}