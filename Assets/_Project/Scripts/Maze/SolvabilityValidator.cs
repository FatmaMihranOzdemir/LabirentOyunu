using System.Collections.Generic;
using UnityEngine;

public static class SolvabilityValidator
{
    public static bool IsSolvable(MazeGridData grid)
    {
        return GetPathFromStartToExit(grid).Count > 0;
    }

    /// <summary>
    /// Girişten çıkışa giden yolu sıralı hücre listesi olarak döndürür.
    /// İlk eleman StartPosition, son eleman ExitPosition'dır.
    /// Yol yoksa boş liste döner.
    /// Kişi 2: kapıları bu listedeki ardışık hücre çiftlerinin arasına koyun.
    /// </summary>
    public static List<Vector2Int> GetPathFromStartToExit(MazeGridData grid)
    {
        return GetPath(grid, grid.StartPosition, grid.ExitPosition);
    }

    /// <summary>
    /// İki hücre arasındaki en kısa yolu BFS ile bulur.
    /// </summary>
    public static List<Vector2Int> GetPath(MazeGridData grid, Vector2Int from, Vector2Int to)
    {
        var path = new List<Vector2Int>();
        if (grid == null) return path;
        if (!grid.InBounds(from.x, from.y) || !grid.InBounds(to.x, to.y)) return path;

        // Her hücre için "hangi hücreden geldim" bilgisi — ekmek kırıntıları
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var queue = new Queue<Vector2Int>();

        queue.Enqueue(from);
        visited.Add(from);

        bool found = false;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == to)
            {
                found = true;
                break;
            }

            foreach (Vector2Int neighbor in GetOpenNeighbors(grid, current))
            {
                if (visited.Contains(neighbor)) continue;

                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        if (!found) return path;

        // Kırıntıları geriye takip et
        Vector2Int node = to;
        path.Add(node);

        while (node != from)
        {
            node = cameFrom[node];
            path.Add(node);
        }

        path.Reverse();
        return path;
    }

    private static List<Vector2Int> GetOpenNeighbors(MazeGridData grid, Vector2Int pos)
    {
        var result = new List<Vector2Int>();
        MazeCell cell = grid.GetCell(pos.x, pos.y);
        if (cell == null) return result;

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
