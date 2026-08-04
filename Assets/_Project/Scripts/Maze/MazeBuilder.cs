using UnityEngine;

public class MazeBuilder : MonoBehaviour
{
    public MazeGridData Grid { get; private set; }
    public event System.Action<MazeGridData> OnMazeBuilt;

    /// <summary>
    /// Kişi 2 ve Kişi 3 abone olurken bu metodu kullanmalı.
    /// Labirent zaten üretilmişse geri çağırma anında bir kez çalışır,
    /// üretilmemişse ilk üretimde ve sonraki her yeniden üretimde çalışır.
    /// </summary>
    public void SubscribeAndSyncNow(System.Action<MazeGridData> callback)
    {
        if (callback == null) return;
        OnMazeBuilt += callback;
        if (Grid != null) callback(Grid);
    }

    public void Unsubscribe(System.Action<MazeGridData> callback)
    {
        OnMazeBuilt -= callback;
    }

    [Header("Boyut Ayarları")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 4f;
    public float wallHeight = 3f;
    public float wallThickness = 0.2f;

    public enum MazeDifficulty { Kolay, Orta, Zor }

    [Header("Zorluk Ayarı")]
    public MazeDifficulty difficulty = MazeDifficulty.Orta;

    [ContextMenu("Zorluk Ön Ayarını Uygula")]
    public void ApplyDifficultyPreset()
    {
        switch (difficulty)
        {
            case MazeDifficulty.Kolay: width = 6; height = 6; break;
            case MazeDifficulty.Orta: width = 10; height = 10; break;
            case MazeDifficulty.Zor: width = 16; height = 16; break;
        }
    }

    [Header("Materyaller (opsiyonel, boş bırakılabilir)")]
    public Material wallMaterial;
    public Material floorMaterial;

    [Header("Prefablar (opsiyonel — boşsa basit küp kullanılır)")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;

    [Header("Giriş / Çıkış")]
    public Transform player;

    [Header("Hata Ayıklama")]
    public bool logDebugInfo = true;

    void Start()
    {
        BuildMaze();
    }

    [ContextMenu("Labirenti Şimdi Oluştur")]
    public void BuildMaze()
    {
        ClearChildren();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        MazeGridData grid = MazeGenerator.Generate(width, height);
        sw.Stop();

        if (logDebugInfo)
        {
            Debug.Log($"[Performans] {width}x{height} labirent {sw.ElapsedMilliseconds} ms'de üretildi.");
            MazeGenerator.ValidateGeneration(grid);
            MazeGenerator.PrintMaze(grid);
            Debug.Log($"[Çözülebilirlik] {(SolvabilityValidator.IsSolvable(grid) ? "EVET, çözülebilir." : "HAYIR, HATA VAR!")}");
        }

        CreateFloor(grid);

        string startDoorDir = GetBoundaryDoorDirection(grid, grid.StartPosition);
        string exitDoorDir = GetBoundaryDoorDirection(grid, grid.ExitPosition);

        BuildWalls(grid);
        BuildEntranceVestibule(grid, startDoorDir);
        BuildExitPlatform(grid, exitDoorDir);

        Grid = grid;
        OnMazeBuilt?.Invoke(grid);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    // Duvarlar SADECE grid verisine göre örülür.
    // Giriş/çıkış delikleri MazeGenerator.OpenBoundaryWall içinde veride açılır,
    // burada ayrıca bir istisna mantığı YOKTUR. Tek doğruluk kaynağı: grid.
    private void BuildWalls(MazeGridData grid)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                MazeCell cell = grid.GetCell(x, z);
                Vector3 center = CellToWorldPosition(x, z);

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

    // --- Giriş odası: üç tarafı duvarla kapalı, tek açıklığı labirent kapısı ---

    private void BuildEntranceVestibule(MazeGridData grid, string doorDir)
    {
        Vector3 cellCenter = CellToWorldPosition(grid.StartPosition.x, grid.StartPosition.y);
        Vector3 outward = GetOutwardDirection(doorDir);
        Vector3 side = GetSideDirection(doorDir);
        Vector3 roomCenter = cellCenter + outward * cellSize;

        CreatePad("EntranceFloor", roomCenter);

        CreateWall(roomCenter + outward * (cellSize / 2f), WallScaleForNormal(outward));
        CreateWall(roomCenter + side * (cellSize / 2f), WallScaleForNormal(side));
        CreateWall(roomCenter - side * (cellSize / 2f), WallScaleForNormal(side));

        if (logDebugInfo)
            Debug.Log($"[Giriş] Giriş odası oluşturuldu ({doorDir} kapısı): {roomCenter}");

        if (player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.position = roomCenter + new Vector3(0, 1f, 0);
            player.rotation = Quaternion.LookRotation(-outward);

            if (cc != null) cc.enabled = true;
        }

        else
        {
            Debug.LogWarning("[Giriş] Player alanı atanmamış, oyuncu taşınamadı.");
        }
    }

    // --- Çıkış platformu: dışarıda, açık, ışıklandırılacak ---

    private void BuildExitPlatform(MazeGridData grid, string doorDir)
    {
        Vector3 cellCenter = CellToWorldPosition(grid.ExitPosition.x, grid.ExitPosition.y);
        Vector3 outward = GetOutwardDirection(doorDir);
        Vector3 padCenter = cellCenter + outward * cellSize;

        CreatePad("ExitFloor", padCenter);

        GameObject trigger = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trigger.name = "ExitTrigger";
        trigger.transform.parent = transform;
        trigger.transform.localPosition = padCenter + new Vector3(0, wallHeight / 2f, 0);
        trigger.transform.localScale = new Vector3(cellSize * 0.9f, wallHeight * 0.9f, cellSize * 0.9f);

        trigger.GetComponent<Collider>().isTrigger = true;
        trigger.GetComponent<Renderer>().material.color = Color.green;
        trigger.AddComponent<ExitMarker>();

        if (logDebugInfo)
            Debug.Log($"[Çıkış] Çıkış platformu ve tetikleyici yerleştirildi ({doorDir} kapısı): {padCenter}");
    }

    // --- Yön yardımcıları ---

    private Vector3 GetOutwardDirection(string direction)
    {
        switch (direction)
        {
            case "N": return Vector3.forward;
            case "S": return Vector3.back;
            case "E": return Vector3.right;
            case "W": return Vector3.left;
            default: return Vector3.zero;
        }
    }

    private Vector3 GetSideDirection(string direction)
    {
        return (direction == "N" || direction == "S") ? Vector3.right : Vector3.forward;
    }

    private Vector3 WallScaleForNormal(Vector3 normal)
    {
        if (Mathf.Abs(normal.x) > 0.5f)
            return new Vector3(wallThickness, wallHeight, cellSize + wallThickness);
        return new Vector3(cellSize + wallThickness, wallHeight, wallThickness);
    }

    // DİKKAT: Buradaki kontrol sırası (S, N, W, E),
    // MazeGenerator.OpenBoundaryWall içindeki sırayla birebir aynı olmak zorunda.
    private string GetBoundaryDoorDirection(MazeGridData grid, Vector2Int pos)
    {
        if (pos.y == 0) return "S";
        if (pos.y == grid.Height - 1) return "N";
        if (pos.x == 0) return "W";
        if (pos.x == grid.Width - 1) return "E";
        return null;
    }

    // --- Yapı elemanları ---

    public Vector3 CellToWorldPosition(int x, int z)
    {
        return new Vector3(x * cellSize, 0, z * cellSize);
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int z = Mathf.RoundToInt(worldPosition.z / cellSize);
        return new Vector2Int(x, z);
    }

    private void CreateWall(Vector3 localPos, Vector3 scale)
    {
        GameObject wall;

        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, transform);
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.parent = transform;
            if (wallMaterial != null) wall.GetComponent<Renderer>().material = wallMaterial;
        }

        wall.name = "Wall";
        wall.transform.localPosition = localPos + new Vector3(0, wallHeight / 2f, 0);
        wall.transform.localScale = scale;
        wall.transform.localRotation = Quaternion.identity;

     
    }

    private void CreatePad(string padName, Vector3 center)
    {
        GameObject pad = CreateFloorObject();
        pad.name = padName;
        pad.transform.localScale = new Vector3(cellSize, 0.2f, cellSize);
        pad.transform.localPosition = new Vector3(center.x, -0.1f, center.z);
    }



    private void CreateFloor(MazeGridData grid)
    {
        GameObject floor = CreateFloorObject();
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(grid.Width * cellSize, 0.2f, grid.Height * cellSize);
        floor.transform.localPosition = new Vector3((grid.Width - 1) * cellSize / 2f, -0.1f, (grid.Height - 1) * cellSize / 2f);
    }

    private GameObject CreateFloorObject()
    {
        GameObject obj;

        if (floorPrefab != null)
        {
            obj = Instantiate(floorPrefab, transform);
        }
        else
        {
            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.parent = transform;
            if (floorMaterial != null) obj.GetComponent<Renderer>().material = floorMaterial;
        }

        obj.transform.localRotation = Quaternion.identity;
        return obj;
    }

   



}
