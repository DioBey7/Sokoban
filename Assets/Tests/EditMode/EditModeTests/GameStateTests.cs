using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class GameStateTests
{
    private GameState CreateState(params string[] rows)
    {
        int height = rows.Length;
        int width = rows[0].Length;
        CellType[,] grid = new CellType[width, height];
        Vector2Int player = Vector2Int.zero;
        HashSet<Vector2Int> boxes = new HashSet<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = rows[y][x];
                grid[x, y] = CellType.Floor;

                if (c == '#') grid[x, y] = CellType.Wall;
                else if (c == '.') grid[x, y] = CellType.Goal;
                else if (c == '@') player = new Vector2Int(x, y);
                else if (c == '$') boxes.Add(new Vector2Int(x, y));
                else if (c == '*')
                {
                    grid[x, y] = CellType.Goal;
                    boxes.Add(new Vector2Int(x, y));
                }
                else if (c == '+')
                {
                    grid[x, y] = CellType.Goal;
                    player = new Vector2Int(x, y);
                }
            }
        }
        return new GameState(grid, player, boxes);
    }

    [Test]
    public void Move_EmptyFloor_PlayerMoves()
    {
        GameState state = CreateState("#@ #");

        bool result = state.TryMove(Directions.Right);

        Assert.IsTrue(result);
        Assert.AreEqual(new Vector2Int(2, 0), state.Player);
        Assert.AreEqual(1, state.MoveCount);
    }

    [Test]
    public void Move_IntoWall_FailsAndCountersDoNotIncrease()
    {
        GameState state = CreateState("#@#");

        bool result = state.TryMove(Directions.Right);

        Assert.IsFalse(result);
        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
        Assert.AreEqual(0, state.MoveCount);
    }

    [Test]
    public void Push_BoxToEmptyFloor_MovesBoth()
    {
        GameState state = CreateState("#@$ #");

        bool result = state.TryMove(Directions.Right);

        Assert.IsTrue(result);
        Assert.AreEqual(new Vector2Int(2, 0), state.Player);
        Assert.IsTrue(state.Boxes.Contains(new Vector2Int(3, 0)));
        Assert.AreEqual(1, state.PushCount);
    }

    [Test]
    public void Push_BoxIntoWall_Fails()
    {
        GameState state = CreateState("#@$#");

        bool result = state.TryMove(Directions.Right);

        Assert.IsFalse(result);
        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
        Assert.IsTrue(state.Boxes.Contains(new Vector2Int(2, 0)));
    }

    [Test]
    public void Push_BoxIntoBox_Fails()
    {
        GameState state = CreateState("#@$$ #");

        bool result = state.TryMove(Directions.Right);

        Assert.IsFalse(result);
        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
    }

    [Test]
    public void Push_BoxToGoal_IsSolvedReturnsTrue()
    {
        GameState state = CreateState("#@$.#");

        state.TryMove(Directions.Right);

        Assert.IsTrue(state.IsSolved());
    }

    [Test]
    public void Undo_AfterMove_RevertsState()
    {
        GameState state = CreateState("#@ #");

        state.TryMove(Directions.Right);
        state.TryUndo(out MoveRecord record, out Vector2Int prevPos);

        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
        Assert.AreEqual(0, state.MoveCount);
    }

    [Test]
    public void Undo_EmptyHistory_IgnoredQuietly()
    {
        GameState state = CreateState("#@ #");

        bool canUndo = state.TryUndo(out MoveRecord record, out Vector2Int prevPos);

        Assert.IsFalse(canUndo);
        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
    }

    [Test]
    public void ResetState_RestoresInitialCondition()
    {
        GameState state = CreateState("#@$ #");
        Vector2Int initialPlayer = state.Player;
        HashSet<Vector2Int> initialBoxes = new HashSet<Vector2Int>(state.Boxes);

        state.TryMove(Directions.Right);
        state.ResetState(initialPlayer, initialBoxes);

        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
        Assert.IsTrue(state.Boxes.Contains(new Vector2Int(2, 0)));
        Assert.AreEqual(0, state.MoveCount);
        Assert.AreEqual(0, state.PushCount);
    }

    [Test]
    public void Scenario7_PushBoxOffGoal_IsSolvedIsFalseAndGoalRemains()
    {
        GameState state = CreateState("#@* #");
        state.TryMove(Directions.Right);
        Assert.IsFalse(state.IsSolved());
        Assert.AreEqual(CellType.Goal, state.StaticGrid[2, 0]);
    }

    [Test]
    public void Scenario8_PlayerStartsOnGoal_PreservesGoal()
    {
        GameState state = CreateState("#+$ #");
        Assert.AreEqual(new Vector2Int(1, 0), state.Player);
        Assert.AreEqual(CellType.Goal, state.StaticGrid[1, 0]);
    }

    [Test]
    public void Scenario13_MoveOutOfBounds_ReturnsFalseNoException()
    {
        GameState state = CreateState("@");
        bool result = state.TryMove(Directions.Right);
        Assert.IsFalse(result);
        Assert.AreEqual(new Vector2Int(0, 0), state.Player);
    }
}