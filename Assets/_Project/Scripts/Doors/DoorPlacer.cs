using System.Collections.Generic;
using UnityEngine;

public class DoorPlacer : MonoBehaviour
{
    [Header("Bağlantılar")]
    public MazeBuilder mazeBuilder;
    public Transform player;

    [Header("Yerleşim")]
    [Tooltip("Ana yol üzerine konacak kapı sayısı — oyuncu bunlarla mutlaka karşılaşır.")]
    public int pathDoorCount = 3;
    [Tooltip("Yan dallara ve çıkmazlara konacak kapı sayısı — oyuncu kapıya bakarak yolu çözemesin diye.")]
    public int decoyDoorCount = 2;
    [Tooltip("Yolun başında ve sonunda kaç hücre kapısız kalsın.")]
    public int safeMargin = 3;

    [Header("Tip")]
    public bool mixTypes = true;
    public SlidingDoor.DoorType singleType = SlidingDoor.DoorType.Ritmik;

    [Header("Görünüm")]
    public Material doorMaterial;
    [Tooltip("Kapı bloğu modeli. Boş bırakılırsa basit küp kullanılır.")]
    public GameObject doorSlabPrefab;

    [Header("Hata Ayıklama")]
    public bool logDebugInfo = true;

    void Start()
    {
        if (mazeBuilder != null)
            mazeBuilder.SubscribeAndSyncNow(OnMazeReady);
        else
            Debug.LogWarning("[DoorPlacer] MazeBuilder atanmamış.");
    }

    void OnDestroy()
    {
        if (mazeBuilder != null) mazeBuilder.Unsubscribe(OnMazeReady);
    }

    private void OnMazeReady(MazeGridData grid)
    {
        ClearDoors();

        var path = SolvabilityValidator.GetPathFromStartToExit(grid);
        var usedKeys = new HashSet<string>();
        int index = 0;

        // --- 1) Ana yol üzerindeki kapılar ---
        int firstUsable = safeMargin;
        int lastUsable = path.Count - 1 - safeMargin;
        int usableSpan = lastUsable - firstUsable;

        if (usableSpan >= 1)
        {
            for (int i = 0; i < pathDoorCount; i++)
            {
                float t = (i + 1f) / (pathDoorCount + 1f);
                int p = firstUsable + Mathf.RoundToInt(t * usableSpan);
                p = Mathf.Clamp(p, firstUsable, lastUsable - 1);

                string key = PassageKey(path[p], path[p + 1]);
                if (!usedKeys.Add(key)) continue;

                PlaceDoor(path[p], path[p + 1], index++, true);
            }
        }
        else if (logDebugInfo)
        {
            Debug.LogWarning($"[DoorPlacer] Yol çok kısa ({path.Count} hücre), ana yola kapı konmadı.");
        }

        // --- 2) Yan dallardaki sahte kapılar ---
        var pathKeys = new HashSet<string>();
        for (int i = 0; i < path.Count - 1; i++)
            pathKeys.Add(PassageKey(path[i], path[i + 1]));

        var candidates = new List<(Vector2Int, Vector2Int)>();
        foreach (var passage in GetAllPassages(grid))
            if (!pathKeys.Contains(PassageKey(passage.Item1, passage.Item2)))
                candidates.Add(passage);

        Shuffle(candidates);

        int placedDecoys = 0;
        foreach (var passage in candidates)
        {
            if (placedDecoys >= decoyDoorCount) break;

            string key = PassageKey(passage.Item1, passage.Item2);
            if (!usedKeys.Add(key)) continue;

            PlaceDoor(passage.Item1, passage.Item2, index++, false);
            placedDecoys++;
        }

        if (logDebugInfo)
            Debug.Log($"[DoorPlacer] Toplam {index} kapı yerleştirildi ({index - placedDecoys} ana yolda, {placedDecoys} yan dalda). Ana yol {path.Count} hücre.");
    }

    private List<(Vector2Int, Vector2Int)> GetAllPassages(MazeGridData grid)
    {
        var list = new List<(Vector2Int, Vector2Int)>();

        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                MazeCell cell = grid.GetCell(x, z);

                if (!cell.WallNorth && z < grid.Height - 1)
                    list.Add((new Vector2Int(x, z), new Vector2Int(x, z + 1)));

                if (!cell.WallEast && x < grid.Width - 1)
                    list.Add((new Vector2Int(x, z), new Vector2Int(x + 1, z)));
            }
        }

        return list;
    }

    private static string PassageKey(Vector2Int a, Vector2Int b)
    {
        // Sıralamadan bağımsız anahtar
        if (b.x < a.x || (b.x == a.x && b.y < a.y))
            (a, b) = (b, a);

        return $"{a.x},{a.y}-{b.x},{b.y}";
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void PlaceDoor(Vector2Int a, Vector2Int b, int index, bool onMainPath)
    {
        Vector3 pa = mazeBuilder.CellToWorldPosition(a.x, a.y);
        Vector3 pb = mazeBuilder.CellToWorldPosition(b.x, b.y);
        Vector3 center = (pa + pb) / 2f;

        GameObject doorObj = new GameObject($"SlidingDoor_{index}");
        doorObj.transform.SetParent(transform, false);
        doorObj.transform.position = center;

        bool eastWestPassage = (a.x != b.x);
        doorObj.transform.rotation = eastWestPassage ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;

        SlidingDoor door = doorObj.AddComponent<SlidingDoor>();

        door.doorType = mixTypes
            ? ((index % 2 == 0) ? SlidingDoor.DoorType.Ritmik : SlidingDoor.DoorType.Tetiklemeli)
            : singleType;

        door.Initialize(player, mazeBuilder.cellSize, mazeBuilder.wallHeight, mazeBuilder.wallThickness, doorMaterial, doorSlabPrefab);

        if (logDebugInfo)
            Debug.Log($"[DoorPlacer] Kapı {index} ({door.doorType}, {(onMainPath ? "ana yol" : "yan dal")}): {a} ↔ {b}");
    }

    private void ClearDoors()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }
}
