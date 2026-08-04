using System.Collections.Generic;
using UnityEngine;

public static class MazeGenerator
{
    public static MazeGridData Generate(int width, int height, int seed = 0)
    {
        MazeGridData grid = new MazeGridData(width, height);
        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);

        Stack<MazeCell> stack = new Stack<MazeCell>();
        MazeCell start = grid.GetCell(0, 0);
        start.GeneratorVisited = true;
        stack.Push(start);

        while (stack.Count > 0)
        {
            MazeCell current = stack.Peek();
            var unvisited = GetUnvisitedNeighbors(grid, current);

            if (unvisited.Count == 0)
            {
                stack.Pop();
                continue;
            }

            var (chosen, direction) = unvisited[rng.Next(unvisited.Count)];
            RemoveWallBetween(current, chosen, direction);
            chosen.GeneratorVisited = true;
            stack.Push(chosen);
        }

        AssignEntryExit(grid, rng);

        return grid;
    }

    private static List<(MazeCell, string)> GetUnvisitedNeighbors(MazeGridData grid, MazeCell cell)
    {
        var result = new List<(MazeCell, string)>();
        TryAdd(grid, cell.X, cell.Z + 1, "N", result);
        TryAdd(grid, cell.X, cell.Z - 1, "S", result);
        TryAdd(grid, cell.X + 1, cell.Z, "E", result);
        TryAdd(grid, cell.X - 1, cell.Z, "W", result);
        return result;
    }

    private static void TryAdd(MazeGridData grid, int x, int z, string dir, List<(MazeCell, string)> list)
    {
        if (!grid.InBounds(x, z)) return;
        MazeCell neighbor = grid.GetCell(x, z);
        if (!neighbor.GeneratorVisited) list.Add((neighbor, dir));
    }

    private static void RemoveWallBetween(MazeCell a, MazeCell b, string direction)
    {
        switch (direction)
        {
            case "N": a.WallNorth = false; b.WallSouth = false; break;
            case "S": a.WallSouth = false; b.WallNorth = false; break;
            case "E": a.WallEast = false; b.WallWest = false; break;
            case "W": a.WallWest = false; b.WallEast = false; break;
        }
    }

    // --- Giriş/Çıkışı birbirinden uzak iki sınır hücresine yerleştirme (double-sweep BFS) ---

    public static void AssignEntryExit(MazeGridData grid, System.Random rng)
    {
        List<Vector2Int> boundaryCells = GetBoundaryCells(grid);
        float minSeparation = Mathf.Max(grid.Width, grid.Height) * 0.6f;

        Vector2Int seed = boundaryCells[rng.Next(boundaryCells.Count)];
        Vector2Int first = FindFarthestBoundaryCell(grid, seed, boundaryCells, 0f);
        Vector2Int second = FindFarthestBoundaryCell(grid, first, boundaryCells, minSeparation);

        grid.StartPosition = first;
        grid.ExitPosition = second;

        grid.GetCell(first.x, first.y).Type = CellType.Start;
        grid.GetCell(second.x, second.y).Type = CellType.Exit;

        OpenBoundaryWall(grid, first);
        OpenBoundaryWall(grid, second);
    }

    // DİKKAT: Buradaki kontrol sırası (S, N, W, E),
    // MazeBuilder.GetBoundaryDoorDirection içindeki sırayla birebir aynı olmak zorunda.
    private static void OpenBoundaryWall(MazeGridData grid, Vector2Int pos)
    {
        MazeCell cell = grid.GetCell(pos.x, pos.y);
        if (cell == null) return;

        if (pos.y == 0) cell.WallSouth = false;
        else if (pos.y == grid.Height - 1) cell.WallNorth = false;
        else if (pos.x == 0) cell.WallWest = false;
        else if (pos.x == grid.Width - 1) cell.WallEast = false;
    }

    private static List<Vector2Int> GetBoundaryCells(MazeGridData grid)
    {
        var result = new List<Vector2Int>();
        for (int x = 0; x < grid.Width; x++)
            for (int z = 0; z < grid.Height; z++)
                if (x == 0 || x == grid.Width - 1 || z == 0 || z == grid.Height - 1)
                    result.Add(new Vector2Int(x, z));
        return result;
    }

    private static Vector2Int FindFarthestBoundaryCell(MazeGridData grid, Vector2Int from, List<Vector2Int> boundaryCells, float minPhysicalDistance)
    {
        Dictionary<Vector2Int, int> distances = BFSDistances(grid, from);

        Vector2Int best = from;
        int bestDist = -1;
        Vector2Int fallback = from;
        int fallbackDist = -1;

        foreach (var cell in boundaryCells)
        {
            if (!distances.TryGetValue(cell, out int d)) continue;

            if (d > fallbackDist)
            {
                fallbackDist = d;
                fallback = cell;
            }

            if (Vector2Int.Distance(cell, from) >= minPhysicalDistance && d > bestDist)
            {
                bestDist = d;
                best = cell;
            }
        }

        return bestDist >= 0 ? best : fallback;
    }

    private static Dictionary<Vector2Int, int> BFSDistances(MazeGridData grid, Vector2Int start)
    {
        var distances = new Dictionary<Vector2Int, int>();
        var queue = new Queue<Vector2Int>();

        distances[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            MazeCell cell = grid.GetCell(current.x, current.y);

            foreach (Vector2Int neighbor in GetOpenNeighbors(grid, cell, current))
            {
                if (!distances.ContainsKey(neighbor))
                {
                    distances[neighbor] = distances[current] + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return distances;
    }

    private static List<Vector2Int> GetOpenNeighbors(MazeGridData grid, MazeCell cell, Vector2Int pos)
    {
        var result = new List<Vector2Int>();
        if (!cell.WallNorth) TryAddOpenNeighbor(grid, pos.x, pos.y + 1, result);
        if (!cell.WallSouth) TryAddOpenNeighbor(grid, pos.x, pos.y - 1, result);
        if (!cell.WallEast) TryAddOpenNeighbor(grid, pos.x + 1, pos.y, result);
        if (!cell.WallWest) TryAddOpenNeighbor(grid, pos.x - 1, pos.y, result);
        return result;
    }

    private static void TryAddOpenNeighbor(MazeGridData grid, int x, int z, List<Vector2Int> list)
    {
        if (grid.InBounds(x, z)) list.Add(new Vector2Int(x, z));
    }

    // --- Test / doğrulama araçları ---

    public static void PrintMaze(MazeGridData grid, List<Vector2Int> path = null)
    {
        HashSet<Vector2Int> pathSet = (path != null) ? new HashSet<Vector2Int>(path) : null;

        var sb = new System.Text.StringBuilder();
        for (int x = 0; x < grid.Width; x++) sb.Append("+--");
        sb.Append("+\n");

        for (int z = grid.Height - 1; z >= 0; z--)
        {
            sb.Append("|");
            for (int x = 0; x < grid.Width; x++)
            {
                MazeCell cell = grid.GetCell(x, z);

                if (cell.Type == CellType.Start) sb.Append(" S");
                else if (cell.Type == CellType.Exit) sb.Append(" E");
                else if (pathSet != null && pathSet.Contains(new Vector2Int(x, z))) sb.Append(" .");
                else sb.Append("  ");

                sb.Append(cell.WallEast ? "|" : " ");
            }
            sb.Append("\n");

            for (int x = 0; x < grid.Width; x++)
            {
                MazeCell cell = grid.GetCell(x, z);
                sb.Append("+");
                sb.Append(cell.WallSouth ? "--" : "  ");
            }
            sb.Append("+\n");
        }

        Debug.Log(sb.ToString());
    }

    public static void ValidateGeneration(MazeGridData grid)
    {
        int visitedCount = 0;
        int removedWalls = 0;

        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                MazeCell cell = grid.GetCell(x, z);
                if (cell.GeneratorVisited) visitedCount++;
                if (!cell.WallNorth && z < grid.Height - 1) removedWalls++;
                if (!cell.WallEast && x < grid.Width - 1) removedWalls++;
            }
        }

        int expectedCells = grid.Width * grid.Height;
        int expectedWalls = expectedCells - 1;

        Debug.Log($"[Doğrulama] Ziyaret edilen hücre: {visitedCount}/{expectedCells} " +
                  (visitedCount == expectedCells ? "OK" : "HATA!"));
        Debug.Log($"[Doğrulama] Kaldırılan duvar: {removedWalls}/{expectedWalls} " +
                  (removedWalls == expectedWalls ? "OK" : "HATA!"));
    }
}
