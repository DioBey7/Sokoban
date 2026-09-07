using System.Collections.Generic;
using UnityEngine;

public enum MoveResult
{
    Success,
    Blocked,
    PortalRejected
}

public struct PlayerMoveRecord
{
    public Vector2Int StartPos;
    public Vector2Int EndPos;
    public bool IsTeleported;
    public Vector2Int PortalEntry;
    public bool IsPortalRejected;
}

public struct BoxMoveRecord
{
    public bool IsPushed;
    public Vector2Int StartPos;
    public Vector2Int EndPos;
    public BoxData Snapshot;
    public bool IsBroken;
    public bool IsTeleported;
    public Vector2Int PortalEntry;
    public bool IsPortalRejected;
}

public struct MoveRecord
{
    public PlayerMoveRecord Player;
    public BoxMoveRecord Box;
}

public class GameState
{
    public StaticElement[,] StaticGrid { get; private set; }
    public Dictionary<Vector2Int, BoxData> Boxes { get; private set; }
    public Dictionary<Vector2Int, ObjectColor> Goals { get; private set; }
    public Dictionary<Vector2Int, int> Doors { get; private set; }
    public Dictionary<Vector2Int, int> Switches { get; private set; }
    public Dictionary<Vector2Int, int> Portals { get; private set; }

    public Vector2Int Player { get; private set; }
    public int MoveCount { get; private set; }
    public int PushCount { get; private set; }

    private Stack<MoveRecord> undoStack;

    public GameState(StaticElement[,] staticGrid, Dictionary<Vector2Int, BoxData> initialBoxes, Vector2Int startPlayer, Dictionary<Vector2Int, ObjectColor> goals, Dictionary<Vector2Int, int> doors, Dictionary<Vector2Int, int> switches, Dictionary<Vector2Int, int> portals)
    {
        StaticGrid = staticGrid;
        Boxes = new Dictionary<Vector2Int, BoxData>(initialBoxes);
        Goals = new Dictionary<Vector2Int, ObjectColor>(goals);
        Doors = new Dictionary<Vector2Int, int>(doors);
        Switches = new Dictionary<Vector2Int, int>(switches);
        Portals = new Dictionary<Vector2Int, int>(portals);
        Player = startPlayer;
        undoStack = new Stack<MoveRecord>();
    }

    private GameState(StaticElement[,] staticGrid, Dictionary<Vector2Int, BoxData> boxes, Vector2Int player, Dictionary<Vector2Int, ObjectColor> goals, Dictionary<Vector2Int, int> doors, Dictionary<Vector2Int, int> switches, Dictionary<Vector2Int, int> portals, int moveCount, int pushCount)
    {
        StaticGrid = staticGrid;
        Boxes = boxes;
        Goals = goals;
        Doors = doors;
        Switches = switches;
        Portals = portals;
        Player = player;
        MoveCount = moveCount;
        PushCount = pushCount;
        undoStack = new Stack<MoveRecord>();
    }

    private Vector2Int GetPortalExit(Vector2Int entryPos, int linkID)
    {
        foreach (var kvp in Portals)
        {
            if (kvp.Value == linkID && kvp.Key != entryPos)
            {
                return kvp.Key;
            }
        }
        return entryPos;
    }

    public MoveResult TryMove(Vector2Int dir, out MoveRecord record)
    {
        record = default;
        Vector2Int startPlayer = Player;
        Vector2Int target = Player + dir;

        if (IsObstacle(target)) return MoveResult.Blocked;

        if (Boxes.TryGetValue(target, out BoxData originalBox))
        {
            if (Goals.TryGetValue(target, out ObjectColor currentGoalColor))
            {
                if (currentGoalColor == ObjectColor.None || currentGoalColor == originalBox.Color) return MoveResult.Blocked;
            }

            Vector2Int boxTarget = target + dir;
            bool boxTeleported = false;
            Vector2Int boxPortalEntry = boxTarget;
            bool boxPortalRejected = false;

            if (Portals.TryGetValue(boxTarget, out int boxPortalID))
            {
                Vector2Int exit = GetPortalExit(boxTarget, boxPortalID);
                if (exit != boxTarget)
                {
                    Vector2Int expectedExit = exit + dir;

                    if (IsObstacle(expectedExit) || Boxes.ContainsKey(expectedExit) || expectedExit == target || expectedExit == startPlayer)
                    {
                        boxPortalRejected = true;
                    }
                    else
                    {
                        boxTarget = expectedExit;
                        boxTeleported = true;
                    }
                }
            }

            if (boxPortalRejected)
            {
                record = new MoveRecord
                {
                    Player = new PlayerMoveRecord { StartPos = startPlayer, EndPos = startPlayer },
                    Box = new BoxMoveRecord { IsPushed = true, StartPos = target, EndPos = target, IsPortalRejected = true, PortalEntry = boxPortalEntry }
                };
                return MoveResult.PortalRejected;
            }

            if (IsObstacle(boxTarget) || Boxes.ContainsKey(boxTarget)) return MoveResult.Blocked;

            bool boxBroke = false;
            BoxData modifiedBox = originalBox;
            Boxes.Remove(target);

            if (modifiedBox.Type == BoxType.Fragile)
            {
                modifiedBox.Durability--;
                if (modifiedBox.Durability <= 0) boxBroke = true;
            }

            if (!boxBroke) Boxes.Add(boxTarget, modifiedBox);

            Vector2Int playerTarget = target;
            bool playerTeleported = false;
            Vector2Int playerPortalEntry = playerTarget;

            Player = playerTarget;
            MoveCount++;
            PushCount++;

            record = new MoveRecord
            {
                Player = new PlayerMoveRecord { StartPos = startPlayer, EndPos = playerTarget, IsTeleported = playerTeleported, PortalEntry = playerPortalEntry },
                Box = new BoxMoveRecord { IsPushed = true, StartPos = target, EndPos = boxTarget, Snapshot = originalBox, IsBroken = boxBroke, IsTeleported = boxTeleported, PortalEntry = boxPortalEntry }
            };

            undoStack.Push(record);
            return MoveResult.Success;
        }

        Vector2Int pTarget = target;
        bool pTeleported = false;
        Vector2Int pPortalEntry = pTarget;
        bool pPortalRejected = false;

        if (Portals.TryGetValue(pTarget, out int pPortalID))
        {
            Vector2Int exit = GetPortalExit(pTarget, pPortalID);
            if (exit != pTarget)
            {
                Vector2Int expectedExit = exit + dir;

                if (IsObstacle(expectedExit) || Boxes.ContainsKey(expectedExit))
                {
                    pPortalRejected = true;
                }
                else
                {
                    pTarget = expectedExit;
                    pTeleported = true;
                }
            }
        }

        if (pPortalRejected)
        {
            record = new MoveRecord
            {
                Player = new PlayerMoveRecord { StartPos = startPlayer, EndPos = startPlayer, IsPortalRejected = true, PortalEntry = pPortalEntry },
                Box = new BoxMoveRecord { IsPushed = false }
            };
            return MoveResult.PortalRejected;
        }

        if (IsObstacle(pTarget) || Boxes.ContainsKey(pTarget)) return MoveResult.Blocked;

        Player = pTarget;
        MoveCount++;

        record = new MoveRecord
        {
            Player = new PlayerMoveRecord { StartPos = startPlayer, EndPos = pTarget, IsTeleported = pTeleported, PortalEntry = pPortalEntry },
            Box = new BoxMoveRecord { IsPushed = false }
        };

        undoStack.Push(record);
        return MoveResult.Success;
    }

    public bool TryUndo(out MoveRecord record)
    {
        record = default;
        if (undoStack.Count == 0) return false;

        record = undoStack.Pop();
        Player = record.Player.StartPos;
        MoveCount--;

        if (record.Box.IsPushed)
        {
            if (!record.Box.IsBroken) Boxes.Remove(record.Box.EndPos);
            Boxes.Add(record.Box.StartPos, record.Box.Snapshot);
            PushCount--;
        }

        return true;
    }

    public bool IsDoorOpen(Vector2Int doorPos)
    {
        if (!Doors.TryGetValue(doorPos, out int doorID)) return false;

        foreach (var kvp in Switches)
        {
            if (kvp.Value == doorID)
            {
                Vector2Int switchPos = kvp.Key;
                if (Player == switchPos || Boxes.ContainsKey(switchPos)) return true;
            }
        }
        return false;
    }

    private bool IsObstacle(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= StaticGrid.GetLength(0) || pos.y < 0 || pos.y >= StaticGrid.GetLength(1)) return true;
        StaticElement element = StaticGrid[pos.x, pos.y];
        if (element == StaticElement.Wall) return true;
        if (element == StaticElement.Door && !IsDoorOpen(pos)) return true;

        return false;
    }

    public GameState Clone()
    {
        Dictionary<Vector2Int, BoxData> clonedBoxes = new Dictionary<Vector2Int, BoxData>(Boxes);
        return new GameState(StaticGrid, clonedBoxes, Player, Goals, Doors, Switches, Portals, MoveCount, PushCount);
    }

    public bool IsSolved()
    {
        if (Goals.Count == 0) return false;

        foreach (var goal in Goals)
        {
            if (!Boxes.TryGetValue(goal.Key, out BoxData boxOnGoal)) return false;
            if (goal.Value != ObjectColor.None && boxOnGoal.Color != goal.Value) return false;
        }
        return true;
    }
}