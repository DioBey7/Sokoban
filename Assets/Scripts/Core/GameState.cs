using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public CellType[,] StaticGrid { get; private set; }
    public Vector2Int Player { get; private set; }
    public HashSet<Vector2Int> Boxes { get; private set; }
    public int MoveCount { get; private set; }
    public int PushCount { get; private set; }

    private Stack<MoveRecord> history;

    public GameState(CellType[,] grid, Vector2Int player, HashSet<Vector2Int> boxes)
    {
        StaticGrid = grid;
        Player = player;
        Boxes = new HashSet<Vector2Int>(boxes);
        history = new Stack<MoveRecord>();
        MoveCount = 0;
        PushCount = 0;
    }

    public bool TryMove(Vector2Int dir)
    {
        Vector2Int target = Player + dir;

        if (!InBounds(target) || StaticGrid[target.x, target.y] == CellType.Wall)
            return false;

        if (Boxes.Contains(target))
        {
            Vector2Int pushTarget = target + dir;

            if (!InBounds(pushTarget) || StaticGrid[pushTarget.x, pushTarget.y] == CellType.Wall)
                return false;

            if (Boxes.Contains(pushTarget))
                return false;

            Boxes.Remove(target);
            Boxes.Add(pushTarget);
            Player = target;
            PushCount++;
            MoveCount++;
            history.Push(new MoveRecord(dir, true));
            return true;
        }

        Player = target;
        MoveCount++;
        history.Push(new MoveRecord(dir, false));
        return true;
    }

    public bool TryUndo(out MoveRecord record, out Vector2Int prevPlayer)
    {
        record = default;
        prevPlayer = Vector2Int.zero;

        if (history.Count == 0) return false;

        record = history.Pop();
        prevPlayer = Player - record.dir;

        if (record.pushed)
        {
            Vector2Int boxCurrent = Player + record.dir;
            Boxes.Remove(boxCurrent);
            Boxes.Add(prevPlayer);
            PushCount--;
        }

        Player = prevPlayer;
        MoveCount--;
        return true;
    }

    public void ResetState(Vector2Int initialPlayer, HashSet<Vector2Int> initialBoxes)
    {
        Player = initialPlayer;
        Boxes.Clear();

        foreach (var box in initialBoxes)
        {
            Boxes.Add(box);
        }

        history.Clear();
        MoveCount = 0;
        PushCount = 0;
    }

    public bool IsSolved()
    {
        foreach (var box in Boxes)
        {
            if (StaticGrid[box.x, box.y] != CellType.Goal)
                return false;
        }
        return true;
    }

    public bool IsOnGoal(Vector2Int pos)
    {
        return StaticGrid[pos.x, pos.y] == CellType.Goal;
    }

    private bool InBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < StaticGrid.GetLength(0) && pos.y >= 0 && pos.y < StaticGrid.GetLength(1);
    }

    public bool IsDeadlocked()
    {
        Vector2Int up = Directions.Up;
        Vector2Int down = Directions.Down;
        Vector2Int left = Directions.Left;
        Vector2Int right = Directions.Right;

        foreach (var box in Boxes)
        {
            if (IsOnGoal(box)) continue;

            bool wUp = IsWall(box + up);
            bool wDown = IsWall(box + down);
            bool wLeft = IsWall(box + left);
            bool wRight = IsWall(box + right);

            if ((wUp || wDown) && (wLeft || wRight))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsWall(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= StaticGrid.GetLength(0) || pos.y < 0 || pos.y >= StaticGrid.GetLength(1)) return true;
        return StaticGrid[pos.x, pos.y] == CellType.Wall;
    }

    public HashSet<Vector2Int> GetReachableCells()
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Vector2Int[] dirs = { Directions.Down, Directions.Up, Directions.Left, Directions.Right };

        queue.Enqueue(Player);
        visited.Add(Player);

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int next = curr + dir;

                if (next.x < 0 || next.x >= StaticGrid.GetLength(0) || next.y < 0 || next.y >= StaticGrid.GetLength(1)) continue;
                if (StaticGrid[next.x, next.y] == CellType.Wall) continue;
                if (Boxes.Contains(next)) continue;
                if (visited.Contains(next)) continue;

                visited.Add(next);
                queue.Enqueue(next);
            }
        }
        return visited;
    }

    public GameState Clone()
    {
        GameState clone = new GameState(StaticGrid, Player, new HashSet<Vector2Int>(Boxes));
        clone.MoveCount = this.MoveCount;
        clone.PushCount = this.PushCount;
        return clone;
    }
}