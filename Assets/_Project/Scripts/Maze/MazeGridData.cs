using UnityEngine;

public class MazeGridData
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public MazeCell[,] Cells { get; private set; }

    public Vector2Int StartPosition;
    public Vector2Int ExitPosition;

    public MazeGridData(int width, int height)
    {
        Width = width;
        Height = height;
        Cells = new MazeCell[width, height];

        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
                Cells[x, z] = new MazeCell(x, z);
    }

    public bool InBounds(int x, int z) => x >= 0 && x < Width && z >= 0 && z < Height;

    public MazeCell GetCell(int x, int z) => InBounds(x, z) ? Cells[x, z] : null;
}
