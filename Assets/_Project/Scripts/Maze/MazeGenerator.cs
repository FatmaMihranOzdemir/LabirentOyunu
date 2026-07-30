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
        start.Visited = true;
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
            chosen.Visited = true;
            stack.Push(chosen);
        }

        grid.StartPosition = new Vector2Int(0, 0);
        grid.ExitPosition = new Vector2Int(width - 1, height - 1);

        grid.GetCell(grid.StartPosition.x, grid.StartPosition.y).Type = CellType.Start;
        grid.GetCell(grid.ExitPosition.x, grid.ExitPosition.y).Type = CellType.Exit;

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
        if (!neighbor.Visited) list.Add((neighbor, dir));
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
}
