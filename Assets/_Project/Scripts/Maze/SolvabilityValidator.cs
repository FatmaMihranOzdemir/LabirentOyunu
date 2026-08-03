using System.Collections.Generic;
using UnityEngine;

public static class SolvabilityValidator
{
    public static bool IsSolvable(MazeGridData grid)
    {
        Vector2Int start = grid.StartPosition;
        Vector2Int exit = grid.ExitPosition;

        bool[,] visited = new bool[grid.Width, grid.Height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        queue.Enqueue(start);
        visited[start.x, start.y] = true;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == exit) return true;

            foreach (Vector2Int neighbor in GetOpenNeighbors(grid, current))
            {
                if (!visited[neighbor.x, neighbor.y])
                {
                    visited[neighbor.x, neighbor.y] = true;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    private static List<Vector2Int> GetOpenNeighbors(MazeGridData grid, Vector2Int pos)
    {
        var result = new List<Vector2Int>();
        MazeCell cell = grid.GetCell(pos.x, pos.y);

        if (!cell.WallNorth) TryAddOpen(grid, pos.x, pos.y + 1, result);
        if (!cell.WallSouth) TryAddOpen(grid, pos.x, pos.y - 1, result);
        if (!cell.WallEast) TryAddOpen(grid, pos.x + 1, pos.y, result);
        if (!cell.WallWest) TryAddOpen(grid, pos.x - 1, pos.y, result);

        return result;
    }

    private static void TryAddOpen(MazeGridData grid, int x, int z, List<Vector2Int> list)
    {
        if (grid.InBounds(x, z)) list.Add(new Vector2Int(x, z));
    }
}
