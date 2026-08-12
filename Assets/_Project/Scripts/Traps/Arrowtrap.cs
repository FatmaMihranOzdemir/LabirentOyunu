using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Karsilikli duvarlardan ok firlatan perde tuzagi.
///
/// Oklar YATAY ucar: bir kismi sol duvardan saga, bir kismi sag duvardan sola.
/// Her ok farkli bir yukseklikte ("kat") ucar. Katlar alttan uste dizildigi icin
/// hepsi birlikte gecidi kapatan bir ok perdesi olusturur.
///
/// Perde belirli araliklarla kesilir; oyuncu bu bosluğu yakalayip kosarak gecer.
///
/// Collider kullanilmaz — carpisma matematiksel olarak hesaplanir.
/// </summary>
public class ArrowTrap : MonoBehaviour
{
    public static event System.Action<ArrowTrap> OnPlayerHit;

    [Header("Baglantilar")]
    [Tooltip("ArrowTrapPlacer tarafindan otomatik atanir.")]
    public Transform player;
    [Tooltip("Ok modeli. Bos birakirsan basit bir cubuk olusturulur.")]
    public GameObject arrowPrefab;
    [Tooltip("Duvardaki delik dekoru. Opsiyonel.")]
    public GameObject emitterPrefab;

    [Header("Gecit Olculeri")]
    [Tooltip("Gecidin toplam genisligi. Placer otomatik ayarlar.")]
    public float travelWidth = 6f;
    [Tooltip("Okun duvardan ne kadar iceriden cikacagi (metre).")]
    public float wallClearance = 0.2f;
    [Tooltip("Okun ucunun duvara ne kadar girebilecegi (metre). Duvar kalinligindan KUCUK olmali, " +
             "yoksa mizrak duvarin obur tarafindan gorunur. 0 verirsen hic girmez.")]
    public float wallBiteDepth = 0.1f;

    [Header("Perde Yukseklikleri")]
    [Tooltip("En alttaki okun zeminden yuksekligi.")]
    public float lowestArrowHeight = 0.4f;
    [Tooltip("En ustteki okun zeminden yuksekligi. Duvar yuksekliginden KUCUK olmali, " +
             "yoksa oklar duvarin ustunden gorunur.")]
    public float highestArrowHeight = 2.4f;
    [Tooltip("Kac kat ok olsun. Alttan uste esit araliklarla dizilirler.")]
    public int laneCount = 5;

    [Header("Perde Duzeni")]
    [Tooltip("Acikken komsu katlar ters duvardan gelir: biri soldan, digeri sagdan.")]
    public bool alternateSides = true;
    [Tooltip("Ayni katta iki ok arasindaki sure (saniye). Kucultursen perde sıklasir.")]
    public float spawnInterval = 0.35f;
    [Tooltip("Komsu katlar arasina eklenen faz kaymasi (saniye). Perdeye dalga gorunumu verir.")]
    public float laneOffset = 0.09f;
    [Tooltip("Okun ucus hizi (metre/saniye).")]
    public float arrowSpeed = 12f;

    [Header("Perde Ritmi")]
    [Tooltip("Perdenin acik (tehlikeli) kaldigi sure, saniye.")]
    public float activeDuration = 3f;
    [Tooltip("Perdenin kesildigi, oyuncunun kosarak gececegi bosluk, saniye.")]
    public float gapDuration = 1.8f;
    [Tooltip("Baslangic gecikmesi. Placer her tuzaga farkli deger verir.")]
    public float startDelay = 0f;

    [Header("Ok Gorunumu")]
    [Tooltip("Oklara uygulanacak materyal. Bos birakirsan modelin kendi materyali kalir.")]
    public Material arrowMaterial;
    [Tooltip("Materyal verilmediyse oklar bu renge boyanir.")]
    public Color arrowTint = new Color(0.75f, 0.09f, 0.07f);
    [Tooltip("Acikken oklarin rengi arrowTint ile degistirilir.")]
    public bool applyArrowTint = true;
    [Tooltip("Ok modelinin +X yonune (saga) bakmasi icin gereken duzeltme donusu. " +
             "Model yukari bakiyorsa (mizrak, ok): 0, 0, -90 yaz.")]
    public Vector3 arrowModelRotation = new Vector3(0f, 0f, -90f);
    [Tooltip("Ok modelinin buyutme carpani.")]
    public float arrowModelScale = 0.6f;
    [Tooltip("Modelin enine kalinligi. Ok cok ince gorunuyorsa artir.")]
    public float arrowThicknessMultiplier = 1.5f;
    [Tooltip("Prefab kullanmiyorsan olusturulacak cubugun uzunlugu.")]
    public float fallbackArrowLength = 0.9f;
    [Tooltip("Prefab kullanmiyorsan olusturulacak cubugun kalinligi.")]
    public float fallbackArrowThickness = 0.07f;

    [Header("Duvar Isiklari")]
    [Tooltip("Oklarin ciktigi iki duvara kirmizi isik koyar.")]
    public bool createEmitterLights = true;
    [Tooltip("Isigin rengi.")]
    public Color emitterLightColor = new Color(1f, 0.12f, 0.08f);
    [Tooltip("Perde acikken isik siddeti.")]
    public float emitterLightIntensity = 3f;
    [Tooltip("Isigin menzili. Kucuk tut ki koridoru kizila boyamasin.")]
    public float emitterLightRange = 3f;
    [Tooltip("Perde kesildiginde isik bu orana duser. Oyuncuya gecis anini haber verir.")]
    [Range(0f, 1f)]
    public float lightDimRatio = 0.15f;
    [Tooltip("Isik siddetinin degisim yumusakligi. Buyutursen gecis daha ani olur.")]
    public float lightFadeSpeed = 6f;

    [Header("Carpisma")]
    [Tooltip("Ok ile oyuncu arasinda yatayda bu mesafeden yakinsa carpti sayilir.")]
    public float hitRadiusX = 0.45f;
    [Tooltip("Oyuncunun perde duzleminde sayilmasi icin gereken yakinlik.")]
    public float hitRadiusZ = 0.6f;
    [Tooltip("Oyuncunun boyu. Ok bu araliktan geciyorsa govdesine isabet etmis sayilir.")]
    public float playerHeight = 1.7f;
    [Tooltip("Oyuncunun pivot noktasi ayaginda degilse buradan telafi et.")]
    public float playerFeetOffset = 0f;

    [Header("Hata Ayiklama")]
    public bool logDebugInfo = false;

    // --- ic durum ---

    private class Arrow
    {
        public Transform t;
        public float x;      // yatay konum (degisir)
        public float y;      // kat yuksekligi (sabit)
        public int dir;      // +1 saga, -1 sola
        public bool active;
    }

    private readonly List<Arrow> arrows = new List<Arrow>();
    private float[] laneY;          // her katin yuksekligi
    private int[] laneDir;          // her katin yonu
    private float[] laneTimer;      // her katin kendi sayaci

    private float halfWidth;
    private float arrowHalfLength;   // okun kendi uzunlugunun yarisi, olculerek bulunur
    private float spawnX;            // dogum noktasi (merkez konumu)
    private float despawnX;          // bu noktayi gecince ok kaybolur
    private float phaseTimer;
    private bool curtainActive = true;
    private bool hasHit;
    private Transform arrowRoot;
    private readonly List<Light> emitterLights = new List<Light>();

    void Start()
    {
        halfWidth = Mathf.Max(0.5f, travelWidth * 0.5f - wallClearance);

        BuildLanes();
        BuildArrowPool();
        MeasureArrowLength();
        BuildEmitters();
        BuildEmitterLights();

        phaseTimer = -startDelay;
        curtainActive = true;
    }

    /// <summary>
    /// Havuzdaki bir oku olcup uzunlugunu bulur, sonra dogum ve kaybolma
    /// noktalarini buna gore belirler.
    ///
    /// Amac: okun UCU duvara degdigi anda kaybolmasi. Boylece mizrak
    /// duvarin obur tarafina tasmazken "duvardan duvara" gorunumu de korunur.
    /// </summary>
    private void MeasureArrowLength()
    {
        arrowHalfLength = 0f;

        if (arrows.Count > 0)
        {
            Transform sample = arrows[0].t;

            // Ucus sirasindaki durusuna getir ki uzunlugu dogru eksende olculsun
            Quaternion previous = sample.localRotation;
            sample.localRotation = Quaternion.Euler(arrowModelRotation);
            sample.gameObject.SetActive(true);

            Bounds b = MeasureBounds(arrowRoot, sample);
            arrowHalfLength = b.extents.x;

            sample.gameObject.SetActive(false);
            sample.localRotation = previous;
        }

        // Dogum: kuyruk duvarda, govde koridorun icinde
        spawnX = halfWidth - arrowHalfLength + wallBiteDepth;
        // Kaybolma: uc karsi duvara degdiginde
        despawnX = halfWidth - arrowHalfLength + wallBiteDepth;

        // Gecit cok darsa en azindan bir miktar yol alsin
        if (spawnX <= 0.1f)
        {
            spawnX = halfWidth * 0.9f;
            despawnX = halfWidth * 0.9f;
        }

        if (logDebugInfo)
            Debug.Log($"[Ok] {name} ok yari uzunlugu={arrowHalfLength:F2} " +
                      $"dogum=±{spawnX:F2} kaybolma=±{despawnX:F2} yariGenislik={halfWidth:F2}");
    }

    /// <summary>
    /// contentRoot altindaki tum mesh parcalarinin measureSpace eksenlerindeki
    /// toplam sinir kutusunu hesaplar.
    /// </summary>
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

                if (!started) { result = new Bounds(local, Vector3.zero); started = true; }
                else result.Encapsulate(local);
            }
        }

        if (!started) return new Bounds(Vector3.zero, Vector3.zero);
        return result;
    }

    /// <summary>
    /// Oklarin ciktigi iki duvara birer kirmizi nokta isigi koyar.
    /// Isiklar perdenin ritmiyle parlayip soner — oyuncuya gecis anini haber verir.
    /// </summary>
    private void BuildEmitterLights()
    {
        if (!createEmitterLights) return;

        float midY = (lowestArrowHeight + highestArrowHeight) * 0.5f;

        CreateLight(new Vector3(-halfWidth - wallClearance * 0.5f, midY, 0f));
        CreateLight(new Vector3(halfWidth + wallClearance * 0.5f, midY, 0f));
    }

    private void CreateLight(Vector3 localPos)
    {
        GameObject go = new GameObject("EmitterLight");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPos;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        Light l = go.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = emitterLightColor;
        l.intensity = emitterLightIntensity;
        l.range = emitterLightRange;
        l.shadows = LightShadows.None;

        emitterLights.Add(l);
    }

    /// <summary>
    /// Isiklari perdenin durumuna gore yumusakca parlatir veya sondurur.
    /// </summary>
    private void UpdateEmitterLights()
    {
        if (emitterLights.Count == 0) return;

        float target = curtainActive
            ? emitterLightIntensity
            : emitterLightIntensity * lightDimRatio;

        float t = 1f - Mathf.Exp(-lightFadeSpeed * Time.deltaTime);

        foreach (Light l in emitterLights)
        {
            if (l == null) continue;
            l.intensity = Mathf.Lerp(l.intensity, target, t);
        }
    }

    /// <summary>
    /// Katlari alttan uste esit araliklarla yerlestirir
    /// ve her birine hangi duvardan gelecegini atar.
    /// </summary>
    private void BuildLanes()
    {
        int n = Mathf.Max(1, laneCount);

        laneY = new float[n];
        laneDir = new int[n];
        laneTimer = new float[n];

        float low = Mathf.Min(lowestArrowHeight, highestArrowHeight);
        float high = Mathf.Max(lowestArrowHeight, highestArrowHeight);

        for (int i = 0; i < n; i++)
        {
            laneY[i] = (n == 1) ? (low + high) * 0.5f
                                : Mathf.Lerp(low, high, i / (n - 1f));

            // Komsu katlar ters duvardan gelsin
            laneDir[i] = (alternateSides && i % 2 == 1) ? -1 : 1;

            // Faz kaymasi: katlar ayni anda ates etmesin
            laneTimer[i] = -i * laneOffset;
        }
    }

    private void BuildArrowPool()
    {
        GameObject rootGO = new GameObject("Arrows");
        arrowRoot = rootGO.transform;
        arrowRoot.SetParent(transform, false);
        arrowRoot.localPosition = Vector3.zero;
        arrowRoot.localRotation = Quaternion.identity;
        arrowRoot.localScale = Vector3.one;

        float flightTime = (halfWidth * 2f) / Mathf.Max(0.1f, arrowSpeed);
        int perLane = Mathf.CeilToInt(flightTime / Mathf.Max(0.05f, spawnInterval)) + 2;
        int poolSize = Mathf.Max(8, perLane * Mathf.Max(1, laneCount));

        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = CreateArrowVisual();
            go.transform.SetParent(arrowRoot, false);
            go.SetActive(false);

            arrows.Add(new Arrow { t = go.transform, active = false });
        }

        if (logDebugInfo)
            Debug.Log($"[Ok] {name} havuz: {poolSize} ok ({laneCount} kat x {perLane})");
    }

    private GameObject CreateArrowVisual()
    {
        if (arrowPrefab != null)
        {
            GameObject inst = Instantiate(arrowPrefab);
            inst.name = "Arrow";

            foreach (Collider c in inst.GetComponentsInChildren<Collider>(true))
                Destroy(c);

            float s = Mathf.Max(0.01f, arrowModelScale);
            float thick = Mathf.Max(0.1f, arrowThicknessMultiplier);

            // Model kendi yerel uzayinda +Y yonunde uzun.
            // Uzunlugu s ile, enini s*thick ile olcekliyoruz.
            inst.transform.localScale = Vector3.Scale(
                inst.transform.localScale,
                new Vector3(s * thick, s, s * thick));

            ApplyArrowLook(inst);
            return inst;
        }

        // Prefab yoksa: ince silindir. Y ekseninde uzar, model duzeltmesi
        // varsayilan olarak onu yatay cevirir.
        GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cyl.name = "Arrow";

        Collider col = cyl.GetComponent<Collider>();
        if (col != null) Destroy(col);

        cyl.transform.localScale = new Vector3(
            fallbackArrowThickness,
            fallbackArrowLength * 0.5f,
            fallbackArrowThickness);

        GameObject holder = new GameObject("ArrowHolder");
        cyl.transform.SetParent(holder.transform, false);

        ApplyArrowLook(holder);
        return holder;
    }

    /// <summary>
    /// Oka materyal ve/veya renk uygular.
    /// Materyal verilmisse o kullanilir, verilmemisse mevcut materyalin rengi degistirilir.
    /// Kopya materyal olusturuldugu icin paketteki orijinal materyal bozulmaz.
    /// </summary>
    private void ApplyArrowLook(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            if (arrowMaterial != null)
            {
                r.material = arrowMaterial;
                continue;
            }

            if (!applyArrowTint) continue;

            // r.material zaten kopya dondurur, orijinal asset etkilenmez
            Material m = r.material;

            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", arrowTint);
            else if (m.HasProperty("_Color")) m.SetColor("_Color", arrowTint);
        }
    }

    /// <summary>
    /// Her katin iki duvardaki agzina dekor koyar. Prefab verilmemisse atlanir.
    /// </summary>
    private void BuildEmitters()
    {
        if (emitterPrefab == null || laneY == null) return;

        for (int i = 0; i < laneY.Length; i++)
        {
            CreateEmitter(new Vector3(-halfWidth - wallClearance, laneY[i], 0f), 90f);
            CreateEmitter(new Vector3(halfWidth + wallClearance, laneY[i], 0f), -90f);
        }
    }

    private void CreateEmitter(Vector3 localPos, float yaw)
    {
        GameObject e = Instantiate(emitterPrefab, transform, false);
        e.name = "Emitter";
        e.transform.localPosition = localPos;
        e.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

        foreach (Collider c in e.GetComponentsInChildren<Collider>(true))
            Destroy(c);
    }

    void Update()
    {
        UpdateCurtainRhythm();
        UpdateEmitterLights();
        MoveArrows();
        SpawnArrows();
        CheckHit();
    }

    private void UpdateCurtainRhythm()
    {
        phaseTimer += Time.deltaTime;

        float limit = curtainActive ? activeDuration : gapDuration;

        if (phaseTimer >= limit)
        {
            phaseTimer = 0f;
            curtainActive = !curtainActive;

            if (logDebugInfo)
                Debug.Log($"[Ok] {name} perde {(curtainActive ? "ACILDI (tehlike)" : "KESILDI (gec!)")}");
        }
    }

    private void SpawnArrows()
    {
        if (!curtainActive || laneY == null) return;

        for (int i = 0; i < laneY.Length; i++)
        {
            laneTimer[i] += Time.deltaTime;

            if (laneTimer[i] < spawnInterval) continue;

            laneTimer[i] -= spawnInterval;
            FireInLane(i);
        }
    }

    private void FireInLane(int lane)
    {
        Arrow a = GetFreeArrow();
        if (a == null) return;

        a.dir = laneDir[lane];
        a.y = laneY[lane];
        a.x = a.dir > 0 ? -spawnX : spawnX;
        a.active = true;

        a.t.gameObject.SetActive(true);
        a.t.localPosition = new Vector3(a.x, a.y, 0f);

        // Once modeli +X yonune cevir, sonra sola gidenleri 180 derece dondur.
        Quaternion correction = Quaternion.Euler(arrowModelRotation);
        Quaternion side = a.dir > 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);
        a.t.localRotation = side * correction;
    }

    private Arrow GetFreeArrow()
    {
        foreach (Arrow a in arrows)
            if (!a.active) return a;

        return null;
    }

    private void MoveArrows()
    {
        float step = arrowSpeed * Time.deltaTime;

        foreach (Arrow a in arrows)
        {
            if (!a.active) continue;

            a.x += a.dir * step;

            // Okun UCU karsi duvara degdiginde kaybolur — duvarin obur tarafina tasmaz
            if (Mathf.Abs(a.x) > despawnX)
            {
                a.active = false;
                a.t.gameObject.SetActive(false);
                continue;
            }

            a.t.localPosition = new Vector3(a.x, a.y, 0f);
        }
    }

    /// <summary>
    /// Ok, oyuncunun govde hacminden geciyorsa isabet sayilir.
    /// </summary>
    private void CheckHit()
    {
        if (hasHit || player == null) return;

        Vector3 local = transform.InverseTransformPoint(player.position);

        if (Mathf.Abs(local.z) > hitRadiusZ) return;

        float feetY = local.y - playerFeetOffset;
        float headY = feetY + playerHeight;

        foreach (Arrow a in arrows)
        {
            if (!a.active) continue;

            if (Mathf.Abs(local.x - a.x) > hitRadiusX) continue;
            if (a.y < feetY || a.y > headY) continue;

            hasHit = true;
            Debug.Log($"[Ok] Oyuncu vuruldu ({name}). " +
                      $"okX={a.x:F2} okY={a.y:F2} | oyuncuX={local.x:F2} ayak={feetY:F2} bas={headY:F2}");
            OnPlayerHit?.Invoke(this);
            return;
        }
    }
}
