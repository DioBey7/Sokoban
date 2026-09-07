using UnityEngine;

public class GoalGridObject : GridObject
{
    public override ObjectCategory category => ObjectCategory.StaticEnvironment;
    public ObjectColor color = ObjectColor.Blue;
}