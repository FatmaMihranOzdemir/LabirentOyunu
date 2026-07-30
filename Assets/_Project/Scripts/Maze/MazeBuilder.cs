using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    [Header("Boyut Ayarları")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.2f;

    [Header("Materyaller (opsiyonel, boş bırakılabilir)")]
    public Material wallMaterial;
    public Material floorMaterial;

    void Start()
    {
        BuildMaze();
    }

    public void BuildMaze()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        MazeGridData grid = MazeGenerator.Generate(width, height);
        CreateFloor(grid);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                MazeCell cell = grid.GetCell(x, z);
                Vector3 center = new Vector3(x * cellSize, 0, z * cellSize);

                if (z == 0 && cell.WallSouth)
                    CreateWall(center + new Vector3(0, 0, -cellSize / 2f), new Vector3(cellSize, wallHeight, wallThickness));
                if (x == 0 && cell.WallWest)
                    CreateWall(center + new Vector3(-cellSize / 2f, 0, 0), new Vector3(wallThickness, wallHeight, cellSize));
                if (cell.WallNorth)
                    CreateWall(center + new Vector3(0, 0, cellSize / 2f), new Vector3(cellSize, wallHeight, wallThickness));
                if (cell.WallEast)
                    CreateWall(center + new Vector3(cellSize / 2f, 0, 0), new Vector3(wallThickness, wallHeight, cellSize));
            }
        }
    }

    private void CreateWall(Vector3 localPos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.parent = transform;
        wall.transform.localPosition = localPos + new Vector3(0, wallHeight / 2f, 0);
        wall.transform.localScale = scale;
        if (wallMaterial != null) wall.GetComponent<Renderer>().material = wallMaterial;
    }

    private void CreateFloor(MazeGridData grid)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = transform;
        floor.transform.localScale = new Vector3(grid.Width * cellSize, 0.2f, grid.Height * cellSize);
        floor.transform.localPosition = new Vector3((grid.Width - 1) * cellSize / 2f, -0.1f, (grid.Height - 1) * cellSize / 2f);
        if (floorMaterial != null) floor.GetComponent<Renderer>().material = floorMaterial;
    }
}
