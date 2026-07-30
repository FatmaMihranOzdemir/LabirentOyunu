using System;

[Serializable]
public class MazeCell
{
    public int X;
    public int Z;

    public bool WallNorth = true;
    public bool WallSouth = true;
    public bool WallEast = true;
    public bool WallWest = true;

    public bool Visited = false;

    public CellType Type = CellType.Normal;

    public MazeCell(int x, int z)
    {
        X = x;
        Z = z;
    }
}

public enum CellType
{
    Normal,
    Start,
    Exit
}
