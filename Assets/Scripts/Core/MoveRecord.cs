public struct MoveRecord
{
    public UnityEngine.Vector2Int dir;
    public bool pushed;

    public MoveRecord(UnityEngine.Vector2Int dir, bool pushed)
    {
        this.dir = dir;
        this.pushed = pushed;
    }
}