using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Assert = NUnit.Framework.Assert;

public class ScannerTests
{
    private GameObject root;
    private float c;

    [SetUp]
    public void Setup()
    {
        root = new GameObject("LevelRoot");
        c = GridGeometry.CELL_SIZE;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(root);
    }

    private void AddGridObject(CellKind kind, Vector3 localPos)
    {
        GameObject obj = new GameObject(kind.ToString());
        obj.transform.SetParent(root.transform);
        obj.transform.localPosition = localPos;
        GridObject go = obj.AddComponent<GridObject>();
        go.kind = kind;
    }

    [Test]
    public void Scenario14_RootOffCenter_MatrixCorrect()
    {
        root.transform.position = new Vector3(250f, -100f, 50f);

        AddGridObject(CellKind.Player, new Vector3(1 * c, -1 * c, 0f));
        AddGridObject(CellKind.Box, new Vector3(2 * c, -1 * c, 0f));
        AddGridObject(CellKind.Goal, new Vector3(2 * c, -1 * c, 0f));

        LevelDataPayload payload = LevelScanner.Scan(root.transform);

        Assert.IsNotNull(payload);
        Assert.AreEqual(new Vector2Int(0, 0), payload.State.Player);
        Assert.IsTrue(payload.State.Boxes.Contains(new Vector2Int(1, 0)));
    }

    [Test]
    public void Scenario15_VerticalAxisInversion_HighestYIsRowZero()
    {
        LogAssert.ignoreFailingMessages = true;

        AddGridObject(CellKind.Wall, new Vector3(0f, 0f, 0f));
        AddGridObject(CellKind.Player, new Vector3(0f, -1 * c, 0f));

        LevelDataPayload payload = LevelScanner.Scan(root.transform);

        Assert.IsNotNull(payload);
        Assert.AreEqual(new Vector2Int(0, 1), payload.State.Player);
    }

    [Test]
    public void Scenario16_TwoPlayers_ValidationFailsReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        AddGridObject(CellKind.Player, new Vector3(0f, 0f, 0f));
        AddGridObject(CellKind.Player, new Vector3(1 * c, 0f, 0f));
        AddGridObject(CellKind.Box, new Vector3(2 * c, 0f, 0f));
        AddGridObject(CellKind.Goal, new Vector3(3 * c, 0f, 0f));

        LevelDataPayload payload = LevelScanner.Scan(root.transform);

        Assert.IsNull(payload);
    }

    [Test]
    public void Scenario17_BoxGoalMismatch_ValidationFailsReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        AddGridObject(CellKind.Player, new Vector3(0f, 0f, 0f));
        AddGridObject(CellKind.Box, new Vector3(1 * c, 0f, 0f));

        LevelDataPayload payload = LevelScanner.Scan(root.transform);

        Assert.IsNull(payload);
    }

    [Test]
    public void Scenario18_UnsnappedObject_ValidationFailsReturnsNull()
    {
        LogAssert.ignoreFailingMessages = true;

        AddGridObject(CellKind.Player, new Vector3(1.5f * c, 0f, 0f));
        AddGridObject(CellKind.Box, new Vector3(2 * c, 0f, 0f));
        AddGridObject(CellKind.Goal, new Vector3(3 * c, 0f, 0f));

        LevelDataPayload payload = LevelScanner.Scan(root.transform);

        Assert.IsNull(payload);
    }

    [Test]
    public void Scenario19_BoxStartingOnGoal_CorrectlyParsed()
    {
        AddGridObject(CellKind.Player, new Vector3(0f, 0f, 0f));
        AddGridObject(CellKind.Goal, new Vector3(1 * c, 0f, 0f));
        AddGridObject(CellKind.Box, new Vector3(1 * c, 0f, 0f));

        LevelDataPayload payload = LevelScanner.Scan(root.transform);

        Assert.IsNotNull(payload);
        Assert.IsTrue(payload.State.Boxes.Contains(new Vector2Int(1, 0)));
        Assert.IsTrue(payload.State.IsOnGoal(new Vector2Int(1, 0)));
    }
}