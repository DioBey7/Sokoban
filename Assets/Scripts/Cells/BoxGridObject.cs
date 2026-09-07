using UnityEngine;

public class BoxGridObject : GridObject
{
    public override ObjectCategory category => ObjectCategory.PushableBox;
    public BoxData boxData = new BoxData(BoxType.Normal, ObjectColor.Blue, -1);
}