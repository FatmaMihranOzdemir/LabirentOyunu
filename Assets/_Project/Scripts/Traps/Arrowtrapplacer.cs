using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Labirent her uretildiginde ana yol uzerine ok tuzaklari yerlestirir.
///
/// Iki kare gecikmeli calisir: once kapilar (DoorPlacer), sonra testereler
/// (SawTrapPlacer), en son ok tuzaklari kurulur. Boylece ok tuzagi sahnedeki
/// tum diger tuzaklari gorup onlarla ayni noktaya dusmekten kacinir.
/// </summary>
public class ArrowTrapPlacer : MonoBehaviour
{
    [Header("Baglantilar")]
    public MazeBuilder mazeBuilder;
    public Transform player;
    public GameObject arrowTrapPrefab;

    [Header("Yerlesim")]
    [Tooltip("Ana yol uzerine kac tane ok tuzagi konsun.")]
    public int trapCount = 2;
    [Tooltip("Giris ve cikisa bu kadar hucre yaklasmadan tuzak koyma.")]
    public int safeMargin = 3;
    [Tooltip("Iki ok tuzagi arasinda en az kac hucre olsun.")]
    public int minSpacing = 4;
    [Tooltip("Diger tuzaklardan (kapi, testere) en az kac hucre uzakta olsun.")]
    public float minCellsFromOtherTraps = 0.5f;

    [Header("Cesitlilik")]
    [Tooltip("Her tuzaga 0 ile bu deger arasinda rastgele baslangic gecikmesi verilir, " +
             "boylece hepsi ayni anda ates etmez.")]
    public float maxStartDelay = 1.5f;

    [Header("Hata Ayiklama")]
    public bool logDebugInfo = true;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Coroutine placeRoutine;

    // Tuzaklar labirent objesinin altina konmaz: ust objelerden birinin olcegi
    // esit degilse donen/hareket eden parcalar bozulur.
    private Transform container;

    private Transform GetContainer()
    {
        if (container == null)
        {
            GameObject go = new GameObject("ArrowTraps");
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
            Debug.LogWarning("[ArrowTrapPlacer] MazeBuilder atanmamis.");
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
        if (isActiveAndEnabled) placeRoutine = StartCoroutine(PlaceAfterOtherTraps(grid));
    }

    private IEnumerator PlaceAfterOtherTraps(MazeGridData grid)
    {
        // Kapilar ayni karede, testereler bir sonraki karede kuruluyor.
        // Iki kare bekleyerek hepsini gormus oluyoruz.
        yield return null;
        yield return null;

        if (arrowTrapPrefab == null)
        {
            Debug.LogWarning("[ArrowTrapPlacer] arrowTrapPrefab atanmamis, tuzak konmadi.");
            yield break;
        }

        List<Vector2Int> path = SolvabilityValidator.GetPathFromStartToExit(grid);
        if (path.Count < safeMargin * 2 + 2)
        {
            Debug.LogWarning("[ArrowTrapPlacer] Ana yol cok kisa, ok tuzagi icin yer yok.");
            yield break;
        }

        // Sahnedeki tum tuzaklarin konumlarini topla
        List<Vector3> occupied = new List<Vector3>();

        foreach (SlidingDoor d in FindObjectsByType<SlidingDoor>())
            if (d != null) occupied.Add(d.transform.position);

        foreach (SawTrap s in FindObjectsByType<SawTrap>())
            if (s != null) occupied.Add(s.transform.position);

        int otherTrapCount = occupied.Count;
        float minDistance = minCellsFromOtherTraps * mazeBuilder.cellSize;

        List<int> candidateIndices = new List<int>();
        for (int i = safeMargin; i < path.Count - 1 - safeMargin; i++)
            candidateIndices.Add(i);

        Shuffle(candidateIndices);

        int placed = 0;
        int rejected = 0;
        List<int> usedIndices = new List<int>();
        System.Random rng = new System.Random();

        foreach (int index in candidateIndices)
        {
            if (placed >= trapCount) break;

            bool tooCloseOnPath = false;
            foreach (int used in usedIndices)
            {
                if (Mathf.Abs(used - index) < minSpacing) { tooCloseOnPath = true; break; }
            }
            if (tooCloseOnPath) continue;

            Vector3 mid = PassageMidpoint(path[index], path[index + 1]);

            bool blocked = false;
            foreach (Vector3 o in occupied)
            {
                if (Vector3.Distance(o, mid) < minDistance) { blocked = true; break; }
            }
            if (blocked) { rejected++; continue; }

            float delay = (float)rng.NextDouble() * maxStartDelay;
            PlaceTrap(path[index], path[index + 1], mid, delay);

            occupied.Add(mid);
            usedIndices.Add(index);
            placed++;
        }

        if (logDebugInfo)
        {
            Debug.Log($"[ArrowTrapPlacer] {placed}/{trapCount} ok tuzagi yerlestirildi. " +
                      $"Sahnedeki diger tuzak sayisi: {otherTrapCount}. " +
                      $"Cakisma yuzunden elenen aday: {rejected}.");
        }

        placeRoutine = null;
    }

    private Vector3 PassageMidpoint(Vector2Int a, Vector2Int b)
    {
        Vector3 pa = mazeBuilder.CellToWorldPosition(a.x, a.y);
        Vector3 pb = mazeBuilder.CellToWorldPosition(b.x, b.y);
        return (pa + pb) * 0.5f;
    }

    private void PlaceTrap(Vector2Int cellA, Vector2Int cellB, Vector3 mid, float startDelay)
    {
        Vector3 pa = mazeBuilder.CellToWorldPosition(cellA.x, cellA.y);
        Vector3 pb = mazeBuilder.CellToWorldPosition(cellB.x, cellB.y);

        // Tuzagin +Z ekseni gecidin yonune baksin.
        // Boylece +X ekseni duvardan duvara olan yon olur ve oklar o yonde ucar.
        Vector3 travelDir = (pb - pa).normalized;
        Quaternion rot = Quaternion.LookRotation(travelDir, Vector3.up);

        GameObject obj = Instantiate(arrowTrapPrefab, GetContainer());
        obj.transform.position = mid;
        obj.transform.rotation = rot;
        obj.transform.localScale = Vector3.one;
        obj.name = "ArrowTrap";

        ArrowTrap trap = obj.GetComponent<ArrowTrap>();
        if (trap != null)
        {
            trap.player = player;
            trap.travelWidth = mazeBuilder.cellSize;
            trap.startDelay = startDelay;
        }
        else
        {
            Debug.LogWarning("[ArrowTrapPlacer] Prefab uzerinde ArrowTrap scripti bulunamadi!");
        }

        spawned.Add(obj);
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
