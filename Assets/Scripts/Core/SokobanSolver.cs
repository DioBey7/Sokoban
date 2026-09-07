using System;
using System.Collections.Generic;
using UnityEngine;

public static class SokobanSolver
{
    private struct BoxCompact : IComparable<BoxCompact>
    {
        public int X, Y, Durability, Color;

        public int CompareTo(BoxCompact other)
        {
            if (X != other.X) return X.CompareTo(other.X);
            return Y.CompareTo(other.Y);
        }
    }

    private struct StateWrapper : IEquatable<StateWrapper>
    {
        public readonly Vector2Int Player;
        public readonly Vector2Int LastDir;
        public readonly BoxCompact[] Boxes;
        private readonly int hash;

        public StateWrapper(Vector2Int player, Vector2Int lastDir, Dictionary<Vector2Int, BoxData> boxDict, bool isOnIce)
        {
            Player = player;
            LastDir = isOnIce ? lastDir : Vector2Int.zero;
            Boxes = new BoxCompact[boxDict.Count];

            int i = 0;
            foreach (var kvp in boxDict)
            {
                Boxes[i++] = new BoxCompact { X = kvp.Key.x, Y = kvp.Key.y, Durability = kvp.Value.Durability, Color = (int)kvp.Value.Color };
            }
            Array.Sort(Boxes);

            int h = 17;
            h = unchecked(h * 31 + Player.x);
            h = unchecked(h * 31 + Player.y);
            h = unchecked(h * 31 + LastDir.x);
            h = unchecked(h * 31 + LastDir.y);

            for (int j = 0; j < Boxes.Length; j++)
            {
                h = unchecked(h * 31 + Boxes[j].X);
                h = unchecked(h * 31 + Boxes[j].Y);
                h = unchecked(h * 31 + Boxes[j].Color);
                h = unchecked(h * 31 + Boxes[j].Durability);
            }
            hash = h;
        }

        public bool Equals(StateWrapper other)
        {
            if (Player != other.Player || LastDir != other.LastDir || Boxes.Length != other.Boxes.Length) return false;

            for (int i = 0; i < Boxes.Length; i++)
            {
                if (Boxes[i].X != other.Boxes[i].X || Boxes[i].Y != other.Boxes[i].Y || Boxes[i].Color != other.Boxes[i].Color || Boxes[i].Durability != other.Boxes[i].Durability) return false;
            }
            return true;
        }

        public override int GetHashCode() => hash;
    }

    private struct SearchNode
    {
        public GameState State;
        public List<Vector2Int> Path;
        public Vector2Int LastDir;
        public int Cost;
    }

    private class MinHeap
    {
        private SearchNode[] elements;
        public int Count { get; private set; }

        public MinHeap(int capacity = 200000)
        {
            elements = new SearchNode[capacity];
        }

        public void Push(SearchNode item)
        {
            if (Count == elements.Length) Array.Resize(ref elements, elements.Length * 2);
            elements[Count] = item;
            int ci = Count;
            Count++;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (elements[ci].Cost >= elements[pi].Cost) break;
                var tmp = elements[ci]; elements[ci] = elements[pi]; elements[pi] = tmp;
                ci = pi;
            }
        }

        public SearchNode Pop()
        {
            var frontItem = elements[0];
            Count--;
            elements[0] = elements[Count];
            int li = Count - 1;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && elements[rc].Cost < elements[ci].Cost) ci = rc;
                if (elements[pi].Cost <= elements[ci].Cost) break;
                var tmp = elements[pi]; elements[pi] = elements[ci]; elements[ci] = tmp;
                pi = ci;
            }
            return frontItem;
        }
    }

    public static List<Vector2Int> Solve(GameState initialState)
    {
        MinHeap queue = new MinHeap();
        HashSet<StateWrapper> visited = new HashSet<StateWrapper>();

        Vector2Int[] dirs = { new Vector2Int(0, -1), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

        int startCost = GetHeuristic(initialState);
        queue.Push(new SearchNode { State = initialState.Clone(), Path = new List<Vector2Int>(), LastDir = Vector2Int.zero, Cost = startCost * 1001 });
        visited.Add(new StateWrapper(initialState.Player, Vector2Int.zero, initialState.Boxes, IsOnIce(initialState, initialState.Player)));

        int maxIterations = 500000;
        int iterations = 0;

        while (queue.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            var current = queue.Pop();
            GameState state = current.State;
            List<Vector2Int> path = current.Path;

            if (state.IsSolved()) return path;

            bool onIce = IsOnIce(state, state.Player);
            bool mustSlide = false;

            if (onIce && current.LastDir != Vector2Int.zero)
            {
                GameState testState = state.Clone();
                if (testState.TryMove(current.LastDir, out _) == MoveResult.Success)
                {
                    mustSlide = true;
                }
            }

            Vector2Int[] allowedDirs = mustSlide ? new Vector2Int[] { current.LastDir } : dirs;

            foreach (var dir in allowedDirs)
            {
                GameState nextState = state.Clone();

                if (nextState.TryMove(dir, out MoveRecord record) == MoveResult.Success)
                {
                    if (IsDeadlock(nextState)) continue;

                    bool nextOnIce = IsOnIce(nextState, nextState.Player);
                    StateWrapper wrapper = new StateWrapper(nextState.Player, dir, nextState.Boxes, nextOnIce);

                    if (!visited.Contains(wrapper))
                    {
                        visited.Add(wrapper);
                        List<Vector2Int> nextPath = new List<Vector2Int>(path) { dir };

                        int gCost = nextPath.Count;
                        int hCost = GetHeuristic(nextState);

                        if (hCost >= 999999) continue;

                        queue.Push(new SearchNode { State = nextState, Path = nextPath, LastDir = dir, Cost = gCost * 1000 + hCost * 1001 });
                    }
                }
            }
        }
        return null;
    }

    private static int GetPortalAwareDistance(GameState state, Vector2Int a, Vector2Int b)
    {
        int dist = Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);

        if (state.Portals != null && state.Portals.Count > 0)
        {
            foreach (var p1 in state.Portals)
            {
                foreach (var p2 in state.Portals)
                {
                    if (p1.Value == p2.Value && p1.Key != p2.Key)
                    {
                        int dThroughPortal = Math.Abs(a.x - p1.Key.x) + Math.Abs(a.y - p1.Key.y) + Math.Abs(p2.Key.x - b.x) + Math.Abs(p2.Key.y - b.y);
                        if (dThroughPortal < dist) dist = dThroughPortal;
                    }
                }
            }
        }
        return dist;
    }

    private static int GetHeuristic(GameState state)
    {
        int h = 0;
        int minPlayerTarget = 10000;

        foreach (var goalKvp in state.Goals)
        {
            int minToBox = 10000;
            foreach (var boxKvp in state.Boxes)
            {
                if (goalKvp.Value == ObjectColor.None || goalKvp.Value == boxKvp.Value.Color)
                {
                    int dist = GetPortalAwareDistance(state, goalKvp.Key, boxKvp.Key);
                    if (dist < minToBox) minToBox = dist;
                }
            }
            if (minToBox == 10000) return 999999;
            h += minToBox;
        }

        foreach (var boxKvp in state.Boxes)
        {
            int pDist = GetPortalAwareDistance(state, state.Player, boxKvp.Key);
            if (pDist < minPlayerTarget) minPlayerTarget = pDist;
        }

        if (state.Switches != null)
        {
            foreach (var sw in state.Switches.Keys)
            {
                if (state.Player != sw && !state.Boxes.ContainsKey(sw))
                {
                    int pDist = GetPortalAwareDistance(state, state.Player, sw);
                    if (pDist < minPlayerTarget) minPlayerTarget = pDist;
                }
            }
        }

        if (minPlayerTarget != 10000) h += minPlayerTarget;
        return h;
    }

    private static bool IsOnIce(GameState state, Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= state.StaticGrid.GetLength(0) || pos.y < 0 || pos.y >= state.StaticGrid.GetLength(1)) return false;
        return state.StaticGrid[pos.x, pos.y] == StaticElement.Ice;
    }

    private static bool IsDeadlock(GameState state)
    {
        if (state.Boxes.Count < state.Goals.Count) return true;

        foreach (var kvp in state.Boxes)
        {
            if (kvp.Value.Type != BoxType.Normal) continue;

            Vector2Int pos = kvp.Key;
            bool onSafeSpot = false;

            if (state.Goals.TryGetValue(pos, out ObjectColor gColor))
            {
                if (gColor == ObjectColor.None || gColor == kvp.Value.Color) onSafeSpot = true;
            }

            if (state.Switches != null && state.Switches.ContainsKey(pos)) onSafeSpot = true;
            if (state.Portals != null && state.Portals.ContainsKey(pos)) onSafeSpot = true;

            if (!onSafeSpot)
            {
                bool wallU = IsDeadlockSolid(state, pos.x, pos.y - 1);
                bool wallD = IsDeadlockSolid(state, pos.x, pos.y + 1);
                bool wallL = IsDeadlockSolid(state, pos.x - 1, pos.y);
                bool wallR = IsDeadlockSolid(state, pos.x + 1, pos.y);

                if ((wallU || wallD) && (wallL || wallR))
                {
                    int required = 0;
                    int available = 0;

                    foreach (var g in state.Goals)
                    {
                        if (g.Value == ObjectColor.None || g.Value == kvp.Value.Color) required++;
                    }
                    foreach (var b in state.Boxes)
                    {
                        if (b.Value.Color == kvp.Value.Color) available++;
                    }

                    if (available <= required) return true;
                }
            }
        }

        foreach (var kvp in state.Boxes)
        {
            if (kvp.Value.Type != BoxType.Normal) continue;
            Vector2Int p = kvp.Key;

            if (IsBlockSolid(state, p.x, p.y) && IsBlockSolid(state, p.x + 1, p.y) &&
                IsBlockSolid(state, p.x, p.y + 1) && IsBlockSolid(state, p.x + 1, p.y + 1))
            {
                bool hasUnsolvedBox = false;
                if (IsUnsolvedBox(state, p.x, p.y)) hasUnsolvedBox = true;
                if (!hasUnsolvedBox && IsUnsolvedBox(state, p.x + 1, p.y)) hasUnsolvedBox = true;
                if (!hasUnsolvedBox && IsUnsolvedBox(state, p.x, p.y + 1)) hasUnsolvedBox = true;
                if (!hasUnsolvedBox && IsUnsolvedBox(state, p.x + 1, p.y + 1)) hasUnsolvedBox = true;

                if (hasUnsolvedBox) return true;
            }
        }

        return false;
    }

    private static bool IsDeadlockSolid(GameState state, int x, int y)
    {
        if (x < 0 || x >= state.StaticGrid.GetLength(0) || y < 0 || y >= state.StaticGrid.GetLength(1)) return true;
        return state.StaticGrid[x, y] == StaticElement.Wall;
    }

    private static bool IsBlockSolid(GameState state, int x, int y)
    {
        if (x < 0 || x >= state.StaticGrid.GetLength(0) || y < 0 || y >= state.StaticGrid.GetLength(1)) return true;
        if (state.StaticGrid[x, y] == StaticElement.Wall) return true;

        Vector2Int p = new Vector2Int(x, y);
        if (state.Boxes.TryGetValue(p, out BoxData b) && b.Type == BoxType.Normal) return true;

        return false;
    }

    private static bool IsUnsolvedBox(GameState state, int x, int y)
    {
        Vector2Int p = new Vector2Int(x, y);
        if (state.Boxes.TryGetValue(p, out BoxData b) && b.Type == BoxType.Normal)
        {
            if (state.Goals.TryGetValue(p, out ObjectColor gColor))
            {
                if (gColor == ObjectColor.None || gColor == b.Color) return false;
            }
            if (state.Switches != null && state.Switches.ContainsKey(p)) return false;
            if (state.Portals != null && state.Portals.ContainsKey(p)) return false;

            return true;
        }
        return false;
    }
}