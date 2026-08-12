using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Labirent her uretildiginde ana yol uzerine testere tuzaklari yerlestirir.
///
/// ONEMLI: Yerlestirme bir kare gecikmeli yapilir. Boylece DoorPlacer once kapilarini
/// kurar, testereler de sahnedeki kapilari gorup onlardan uzak durur.
/// Ayni geçide iki tuzak binmesi bu sayede engellenir.
/// </summary>
public class SawTrapPlacer : MonoBehaviour
{
    [Header("Baglantilar")]
    public MazeBuilder mazeBuilder;
    public Transform player;
    public GameObject sawTrapPrefab;

    [Header("Yerlesim")]
    [Tooltip("Ana yol uzerine kac tane testere konsun.")]
    public int trapCount = 2;
    [Tooltip("Giris ve cikisa bu kadar hucre yaklasmadan testere koyma.")]
    public int safeMargin = 3;
    [Tooltip("Iki testere arasinda en az kac hucre olsun.")]
    public int minSpacing = 3;

    [Header("Diger Tuzaklardan Uzaklik")]
    [Tooltip("Kapilardan ve diger testerelerden en az kac hucre uzakta olsun.")]
    public float minCellsFromOtherTraps = 2f;

    [Header("Hata Ayiklama")]
    public bool logDebugInfo = true;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Coroutine placeRoutine;

    // Tuzaklar labirent objesinin ALTINA konmaz.
    // Sebep: ust objelerden birinin olcegi esit degilse (orn. 1.1, 1, 1)
    // donen testere her karede farkli yonde ezilir ve titriyormus gibi gorunur.
    // Bagimsiz, olcegi 1 olan bir kap kullanmak bunu tamamen engeller.
    private Transform container;

    private Transform GetContainer()
    {
        if (container == null)
        {
            GameObject go = new GameObject("SawTraps");
            container = go.transform;
            container.SetParent(null);
            container.position = Vector3.zero;
            container.rotation = Quaternion.identity;
            container.localScale = Vector3.one;
        }
        return container;
    }

    void Start()
    {
        if (mazeBuilder != null)
            mazeBuilder.SubscribeAndSyncNow(OnMazeReady);
        else
            Debug.LogWarning("[SawTrapPlacer] MazeBuilder atanmamis.");
    }

    void OnDestroy()
    {
        if (mazeBuilder != null) mazeBuilder.Unsubscribe(OnMazeReady);
        if (container != null) Destroy(container.gameObject);
    }

    private void OnMazeReady(MazeGridData grid)
    {
        ClearTraps();

        if (placeRoutine != null) StopCoroutine(placeRoutine);
        if (isActiveAndEnabled) placeRoutine = StartCoroutine(PlaceAfterDoors(grid));
    }

    private IEnumerator PlaceAfterDoors(MazeGridData grid)
    {
        // Kapilarin sahneye kurulmasini bekle (ayni karede olusuyorlar).
        yield return null;

        if (sawTrapPrefab == null)
        {
            Debug.LogWarning("[SawTrapPlacer] sawTrapPrefab atanmamis, testere konmadi.");
            yield break;
        }

        List<Vector2Int> path = SolvabilityValidator.GetPathFromStartToExit(grid);
        if (path.Count < safeMargin * 2 + 2)
        {
            Debug.LogWarning("[SawTrapPlacer] Ana yol cok kisa, testere icin yer yok.");
            yield break;
        }

        // Sahnedeki kapilarin konumlarini topla — buralara testere konmayacak
        List<Vector3> occupied = new List<Vector3>();
        SlidingDoor[] doors = FindObjectsByType<SlidingDoor>();
        foreach (SlidingDoor d in doors)
            if (d != null) occupied.Add(d.transform.position);

        float minDistance = minCellsFromOtherTraps * mazeBuilder.cellSize;

        // Giris/cikis yakinini eleyerek aday gecitleri topla
        List<int> candidateIndices = new List<int>();
        for (int i = safeMargin; i < path.Count - 1 - safeMargin; i++)
            candidateIndices.Add(i);

        Shuffle(candidateIndices);

        int placed = 0;
        int rejectedByDoor = 0;
        List<int> usedIndices = new List<int>();

        foreach (int index in candidateIndices)
        {
            if (placed >= trapCount) break;

            // Yol uzerinde diger testerelere cok yakin mi?
            bool tooCloseOnPath = false;
            foreach (int used in usedIndices)
            {
                if (Mathf.Abs(used - index) < minSpacing) { tooCloseOnPath = true; break; }
            }
            if (tooCloseOnPath) continue;

            // Fiziksel olarak bir kapiya ya da baska bir tuzaga cok yakin mi?
            Vector3 mid = PassageMidpoint(path[index], path[index + 1]);

            bool blocked = false;
            foreach (Vector3 o in occupied)
            {
                if (Vector3.Distance(o, mid) < minDistance) { blocked = true; break; }
            }
            if (blocked) { rejectedByDoor++; continue; }

            PlaceTrap(path[index], path[index + 1], mid);
            occupied.Add(mid);
            usedIndices.Add(index);
            placed++;
        }

        if (logDebugInfo)
        {
            Debug.Log($"[SawTrapPlacer] {placed}/{trapCount} testere yerlestirildi. " +
                      $"Sahnedeki kapi sayisi: {doors.Length}. " +
                      $"Cakisma yuzunden elenen aday: {rejectedByDoor}.");

            if (placed < trapCount)
                Debug.LogWarning($"[SawTrapPlacer] Istenen sayida testere konamadi. " +
                                 $"Kapi sayisini, safeMargin'i veya minCellsFromOtherTraps'i azaltmayi dene.");
        }

        placeRoutine = null;
    }

    private Vector3 PassageMidpoint(Vector2Int cellA, Vector2Int cellB)
    {
        Vector3 posA = mazeBuilder.CellToWorldPosition(cellA.x, cellA.y);
        Vector3 posB = mazeBuilder.CellToWorldPosition(cellB.x, cellB.y);
        return (posA + posB) * 0.5f;
    }

    private void PlaceTrap(Vector2Int cellA, Vector2Int cellB, Vector3 mid)
    {
        Vector3 posA = mazeBuilder.CellToWorldPosition(cellA.x, cellA.y);
        Vector3 posB = mazeBuilder.CellToWorldPosition(cellB.x, cellB.y);

        // Tuzagin +Z ekseni gecidin yonune baksin,
        // boylece +X ekseni duvardan duvara olan yon olur
        Vector3 travelDir = (posB - posA).normalized;
        Quaternion rot = Quaternion.LookRotation(travelDir, Vector3.up);

        GameObject trapObj = Instantiate(sawTrapPrefab, GetContainer());
        trapObj.transform.position = mid;
        trapObj.transform.rotation = rot;
        trapObj.transform.localScale = Vector3.one;
        trapObj.name = "SawTrap";

        SawTrap trap = trapObj.GetComponent<SawTrap>();
        if (trap != null)
        {
            trap.player = player;
            trap.travelWidth = Mathf.Max(1f, mazeBuilder.cellSize - 0.6f);
        }
        else
        {
            Debug.LogWarning("[SawTrapPlacer] Prefab uzerinde SawTrap scripti bulunamadi!");
        }

        spawned.Add(trapObj);
    }

    private void ClearTraps()
    {
        foreach (var obj in spawned)
            if (obj != null) Destroy(obj);
        spawned.Clear();
    }

    private void Shuffle(List<int> list)
    {
        System.Random rng = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
