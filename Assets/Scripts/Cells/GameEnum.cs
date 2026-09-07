[System.Serializable]
public enum StaticElement
{
    Empty = 0,
    Wall,
    Goal,
    Ice,
    Switch,
    Door
}

[System.Serializable]
public enum BoxType
{
    Normal,
    Fragile
}

[System.Serializable]
public enum ObjectColor
{
    None = 0,
    Brown,
    Orange,
    Red,
    Blue,
    Green,
    Yellow
}

[System.Serializable] 
public struct BoxData
{
    public BoxType Type;
    public ObjectColor Color;
    public int Durability;

    public BoxData(BoxType type, ObjectColor color, int durability)
    {
        Type = type;
        Color = color;
        Durability = durability;
    }
}