using UnityEngine;

public class StaticGridObject : GridObject
{
    public override ObjectCategory category => ObjectCategory.StaticEnvironment;
    public StaticElement staticElement = StaticElement.Wall;
}