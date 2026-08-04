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

    // Üretim algoritmasının iç bayrağı.
    // Üretim bittiğinde TÜM hücrelerde true olur — oyun sırasında anlamı yoktur.
    // Minimap veya oynanış kodunda KULLANMAYIN.
    public bool GeneratorVisited = false;

    // Oyuncunun bu hücreye fiilen girip girmediği.
    // Minimap ve keşif mekaniği BUNU kullanmalı. Başlangıçta tüm hücrelerde false.
    public bool PlayerVisited = false;

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
