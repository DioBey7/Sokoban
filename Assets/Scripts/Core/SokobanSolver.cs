using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class SokobanSolver
{
    private struct StateHash : IEquatable<StateHash>
    {
        public readonly Vector2Int Player;
        public readonly int BoxHash;

        public StateHash(Vector2Int player, HashSet<Vector2Int> boxes)
        {
            Player = player;
            int hash = 17;
            var sorted = boxes.OrderBy(b => b.x).ThenBy(b => b.y);

            foreach (var b in sorted)
            {
                hash = unchecked(hash * 31 + b.GetHashCode());
            }
            BoxHash = hash;
        }

        public bool Equals(StateHash other)
        {
            return Player == other.Player && BoxHash == other.BoxHash;
        }

        public override int GetHashCode()
        {
            return unchecked(Player.GetHashCode() ^ BoxHash);
        }
    }

    public static List<Vector2Int> Solve(GameState initialState)
    {
        Queue<Tuple<GameState, List<Vector2Int>>> queue = new Queue<Tuple<GameState, List<Vector2Int>>>();
        HashSet<StateHash> visited = new HashSet<StateHash>();
        Vector2Int[] dirs = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };

        queue.Enqueue(new Tuple<GameState, List<Vector2Int>>(initialState.Clone(), new List<Vector2Int>()));
        visited.Add(new StateHash(initialState.Player, initialState.Boxes));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            GameState state = current.Item1;
            List<Vector2Int> path = current.Item2;

            if (state.IsSolved()) return path;

            foreach (var dir in dirs)
            {
                GameState nextState = state.Clone();

                if (nextState.TryMove(dir))
                {
                    StateHash hash = new StateHash(nextState.Player, nextState.Boxes);

                    if (!visited.Contains(hash))
                    {
                        visited.Add(hash);
                        List<Vector2Int> nextPath = new List<Vector2Int>(path) { dir };
                        queue.Enqueue(new Tuple<GameState, List<Vector2Int>>(nextState, nextPath));
                    }
                }
            }
        }
        return null;
    }
}