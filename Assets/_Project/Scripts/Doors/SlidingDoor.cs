using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public enum DoorType { Ritmik, Tetiklemeli }
    public enum DoorState { Acik, Kapaniyor, Kapali, Aciliyor }

    public static event System.Action<SlidingDoor> OnPlayerCrushed;

    [Header("Tip")]
    public DoorType doorType = DoorType.Ritmik;

    [Header("Zamanlama (saniye)")]
    [Tooltip("Use Uniform Timing KAPALIYSA kullanilir. DoorPlacer bu alanlari degistirebilir.")]
    public float openDuration = 3f;
    public float closingDuration = 0.7f;
    public float closedDuration = 2f;
    public float openingDuration = 0.8f;

    [Header("Tek Tip Zamanlama")]
    [Tooltip("Acikken butun kapilar birebir ayni hizda hareket eder. " +
             "Yukaridaki sure alanlari ve DoorPlacer'in verdigi rastgele degerler yok sayilir.")]
    public bool useUniformTiming = true;
    [Tooltip("Kapinin kapanma hizi (metre/saniye). Buyutursen daha hizli kapanir.")]
    public float closeSpeed = 7f;
    [Tooltip("Kapinin acilma hizi (metre/saniye). Genelde kapanmadan biraz yavas olur.")]
    public float openSpeed = 5f;
    [Tooltip("Kapi acik konumda bu kadar bekler (saniye).")]
    public float uniformOpenWait = 3f;
    [Tooltip("Kapi kapali konumda bu kadar bekler (saniye).")]
    public float uniformClosedWait = 2f;

    [Header("Tetiklemeli Ayarı")]
    public float triggerDistance = 8f;

    [Header("Ezme Ayarı")]
    [Tooltip("Oyuncunun levha ile AYNI hizada sayılması için gereken yakınlık. Küçük tut: " +
             "kapının önünde durup dokunmak ölüm sayılmasın, sadece geçidin içinde kalmak saysın.")]
    public float crushZTolerance = 0.15f;

    [Tooltip("Levha, oyuncunun ayak hizasından bu kadar yükselince oyuncu tamamen içinde kalmış " +
             "sayılır ve ezilir. Oyuncu boyuna yakın bir değer olmalı. Küçültürsen erken ölür.")]
    public float crushEngulfHeight = 1.2f;

    [Tooltip("Oyuncunun pivot noktası ayağında değilse buradan telafi et.")]
    public float playerFeetOffset = 0f;

    [Header("Ezme — Tavana Sıkışma")]
    [Tooltip("Oyuncu levhanın ÜSTÜNDEYKEN, levha ile kiriş arasındaki boşluk bu değerin altına düşerse ezilir.")]
    public float crushGapHeight = 1.3f;
    [Tooltip("Oyuncunun levhanın üstünde sayılması için ayaklarının levha tepesine bu kadar yakın olması gerekir.")]
    public float onTopTolerance = 0.6f;

    [Header("Kiriş (sabit üst parça)")]
    [Tooltip("Kirişin yüksekliği, geçit yüksekliğinin oranı olarak.")]
    public float beamHeightRatio = 0.18f;
    [Tooltip("Kiriş de hareketli parçayla aynı prefabtan yapılsın mı?")]
    public bool beamUsesPrefab = true;

    [Header("Hata Ayıklama")]
    [Tooltip("Açarsan her karede oyuncunun kapıya göre konumunu Console'a yazar.")]
    public bool logCrushDebug = false;

    public DoorState State { get; private set; } = DoorState.Acik;

    private Transform slab;
    private Transform player;
    private float timer;
    private Vector3 openLocalPos;
    private Vector3 closedLocalPos;
    private float slabHeight;
    private float slabWidth;
    private float slabThickness;
    private float beamBottomY;
    private bool crushedThisCycle;

    public void Initialize(Transform playerTransform, float passageWidth, float height, float thickness, Material material, GameObject slabPrefab)
    {
        player = playerTransform;
        slabHeight = height;
        slabWidth = passageWidth;
        slabThickness = Mathf.Max(thickness, 0.3f);

        // --- Hareketli levha ---
        GameObject slabRoot = new GameObject("DoorSlab");
        slabRoot.transform.SetParent(transform, false);
        slabRoot.transform.localPosition = Vector3.zero;
        slabRoot.transform.localRotation = Quaternion.identity;
        slabRoot.transform.localScale = Vector3.one;

        Vector3 slabTarget = new Vector3(slabWidth * 0.98f, slabHeight, slabThickness);
        BuildVisual(slabRoot.transform, slabPrefab, material, slabTarget,
                    new Color(0.30f, 0.27f, 0.25f));

        BoxCollider box = slabRoot.AddComponent<BoxCollider>();
        box.center = Vector3.zero;
        box.size = slabTarget;

        slab = slabRoot.transform;

        closedLocalPos = new Vector3(0f, slabHeight / 2f, 0f);
        openLocalPos = new Vector3(0f, -slabHeight / 2f - 0.3f, 0f);
        slab.localPosition = openLocalPos;

        // --- Sabit kiris ---
        GameObject beamRoot = new GameObject("DoorFrame");
        beamRoot.transform.SetParent(transform, false);
        beamRoot.transform.localRotation = Quaternion.identity;
        beamRoot.transform.localScale = Vector3.one;

        float beamHeight = slabHeight * beamHeightRatio;
        Vector3 beamTarget = new Vector3(slabWidth, beamHeight, slabThickness * 1.8f);

        BuildVisual(beamRoot.transform,
                    beamUsesPrefab ? slabPrefab : null,
                    material,
                    beamTarget,
                    new Color(0.45f, 0.15f, 0.12f));

        beamRoot.transform.localPosition = new Vector3(0f, slabHeight - beamHeight * 0.5f, 0f);
        beamBottomY = slabHeight - beamHeight;

        SetState(DoorState.Acik);
    }

    private void BuildVisual(Transform parent, GameObject prefab, Material material, Vector3 targetSize, Color fallbackColor)
    {
        if (prefab == null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Visual";
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = targetSize;

            Renderer r = cube.GetComponent<Renderer>();
            if (material != null) r.material = material;
            else r.material.color = fallbackColor;

            Collider c = cube.GetComponent<Collider>();
            if (c != null) Destroy(c);
            return;
        }

        GameObject fit = new GameObject("Fit");
        fit.transform.SetParent(parent, false);
        fit.transform.localPosition = Vector3.zero;
        fit.transform.localRotation = Quaternion.identity;
        fit.transform.localScale = Vector3.one;

        GameObject inst = Instantiate(prefab, fit.transform, false);
        inst.name = "Visual";
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        foreach (Collider col in inst.GetComponentsInChildren<Collider>(true))
            Destroy(col);

        Bounds b = MeasureBounds(fit.transform, inst.transform);

        if (b.size.x < b.size.z)
        {
            inst.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            b = MeasureBounds(fit.transform, inst.transform);
        }

        Vector3 size = b.size;
        Vector3 scale = new Vector3(
            size.x > 0.0001f ? targetSize.x / size.x : 1f,
            size.y > 0.0001f ? targetSize.y / size.y : 1f,
            size.z > 0.0001f ? targetSize.z / size.z : 1f);

        fit.transform.localScale = scale;
        fit.transform.localPosition = -Vector3.Scale(b.center, scale);

        if (material != null)
        {
            foreach (Renderer r in inst.GetComponentsInChildren<Renderer>(true))
                r.material = material;
        }
    }

    private static Bounds MeasureBounds(Transform measureSpace, Transform contentRoot)
    {
        MeshFilter[] filters = contentRoot.GetComponentsInChildren<MeshFilter>(true);
        bool started = false;
        Bounds result = new Bounds(Vector3.zero, Vector3.zero);

        foreach (MeshFilter f in filters)
        {
            if (f.sharedMesh == null) continue;

            Bounds mb = f.sharedMesh.bounds;
            Vector3 c = mb.center;
            Vector3 e = mb.extents;

            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = c + new Vector3(
                    (i & 1) == 0 ? -e.x : e.x,
                    (i & 2) == 0 ? -e.y : e.y,
                    (i & 4) == 0 ? -e.z : e.z);

                Vector3 world = f.transform.TransformPoint(corner);
                Vector3 local = measureSpace.InverseTransformPoint(world);

                if (!started)
                {
                    result = new Bounds(local, Vector3.zero);
                    started = true;
                }
                else
                {
                    result.Encapsulate(local);
                }
            }
        }

        if (!started) return new Bounds(Vector3.zero, Vector3.one);
        return result;
    }

    void Update()
    {
        if (slab == null) return;

        timer += Time.deltaTime;

        switch (State)
        {
            case DoorState.Acik:
                {
                    float wait = useUniformTiming ? uniformOpenWait : openDuration;

                    if (doorType == DoorType.Ritmik)
                    {
                        if (timer >= wait) SetState(DoorState.Kapaniyor);
                    }
                    else if (PlayerIsNear())
                    {
                        SetState(DoorState.Kapaniyor);
                    }
                }
                break;

            case DoorState.Kapaniyor:
                {
                    bool done = MoveSlabTowards(closedLocalPos, closeSpeed, closingDuration);
                    CheckCrush();
                    if (done) SetState(DoorState.Kapali);
                }
                break;

            case DoorState.Kapali:
                {
                    CheckCrush();
                    float wait = useUniformTiming ? uniformClosedWait : closedDuration;
                    if (timer >= wait) SetState(DoorState.Aciliyor);
                }
                break;

            case DoorState.Aciliyor:
                {
                    bool done = MoveSlabTowards(openLocalPos, openSpeed, openingDuration);
                    if (done) SetState(DoorState.Acik);
                }
                break;
        }
    }

    /// <summary>
    /// Levhayi hedefe dogru hareket ettirir.
    ///
    /// Tek tip zamanlama acikken SABIT HIZ kullanilir (metre/saniye):
    /// mesafe her kapida ayni oldugu icin butun kapilar birebir ayni hizda hareket eder.
    /// DoorPlacer'in verdigi rastgele sure degerleri bu modda hicbir etki yapmaz.
    ///
    /// Kapaliyken eski davranisa doner: verilen sure icinde yumusak gecis.
    /// </summary>
    /// <returns>Hedefe ulasildiysa true.</returns>
    private bool MoveSlabTowards(Vector3 target, float speed, float duration)
    {
        if (useUniformTiming)
        {
            float step = Mathf.Max(0.1f, speed) * Time.deltaTime;
            slab.localPosition = Vector3.MoveTowards(slab.localPosition, target, step);
            return (slab.localPosition - target).sqrMagnitude < 0.0001f;
        }

        Vector3 from = (target == closedLocalPos) ? openLocalPos : closedLocalPos;
        float t = Mathf.Clamp01(timer / Mathf.Max(0.01f, duration));
        slab.localPosition = Vector3.Lerp(from, target, t);
        return t >= 1f;
    }

    private void SetState(DoorState newState)
    {
        State = newState;
        timer = 0f;
        if (newState == DoorState.Acik) crushedThisCycle = false;
    }

    private bool PlayerIsNear()
    {
        if (player == null) return false;
        Vector3 d = player.position - transform.position;
        d.y = 0f;
        return d.magnitude < triggerDistance;
    }

    /// <summary>
    /// Iki ayri sikisma durumu var:
    ///
    /// 1) Oyuncu gecidin ICINDE kalmis ve levha onu tamamen yutmus.
    ///    Levhanin tepesi, oyuncunun ayaklarindan crushEngulfHeight kadar yukari cikmis olmali.
    ///    Levha sadece dizine gelmisse OLMEZ — hala kacabilir.
    ///
    /// 2) Oyuncu levhanin USTUNE cikmis ve yukselen levha onu tavandaki kirise sikistiriyor.
    /// </summary>
    private void CheckCrush()
    {
        if (player == null || crushedThisCycle) return;

        Vector3 local = transform.InverseTransformPoint(player.position);
        float feetY = local.y - playerFeetOffset;
        float slabTop = slab.localPosition.y + slabHeight * 0.5f;

        if (logCrushDebug)
        {
            Debug.Log($"[Kapı-Debug] {name} | local=({local.x:F2}, {local.y:F2}, {local.z:F2}) " +
                      $"slabTop={slabTop:F2} feetY={feetY:F2} " +
                      $"gerekenTepe={feetY + crushEngulfHeight:F2}");
        }

        // Gecidin genisligi disindaysa hic ilgilenme
        if (Mathf.Abs(local.x) > slabWidth * 0.5f) return;

        // Levhanin kalinlik ekseninde ICINDE mi?
        // Sadece dokunmak yetmez, geciyor olmasi gerek.
        if (Mathf.Abs(local.z) > slabThickness * 0.5f + crushZTolerance) return;

        // --- Durum 2: levhanin ustunde, kirise sikisiyor ---
        if (feetY >= slabTop - onTopTolerance)
        {
            float gap = beamBottomY - slabTop;
            if (gap < crushGapHeight) Crush("kirise sikisti");
            return;
        }

        // --- Durum 1: levha oyuncuyu tamamen yuttu mu? ---
        // Sadece bacagina gelmisse olmez, kacma sansi var.
        if (slabTop < feetY + crushEngulfHeight) return;

        Crush("levhanin icinde sikisti");
    }

    private void Crush(string reason)
    {
        crushedThisCycle = true;
        Debug.Log($"[Kapı] Oyuncu ezildi ({name}, {doorType}) — {reason}.");
        OnPlayerCrushed?.Invoke(this);
    }
}
