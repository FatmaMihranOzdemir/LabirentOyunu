using UnityEngine;

/// <summary>
/// Koridorda duvardan duvara gidip gelen testere tuzagi.
/// Oyuncu testereye yakinken ayaklari belirlenen yuksekligin altindaysa olur.
/// Dogru anda ziplarsa saglam gecer.
///
/// Donus, modelin KENDI ekseni etrafinda yapilir (modelcinin tasarladigi eksen).
/// Yukseklik ve gidis araligi ise olculerek otomatik ayarlanir:
/// testere ne kadar buyutulurse buyutulsun zeminden hep ayni kadar yukselir
/// ve disk kenari duvara girmez.
/// </summary>
public class SawTrap : MonoBehaviour
{
    public static event System.Action<SawTrap> OnPlayerHit;

    [Header("Baglantilar")]
    [Tooltip("Testere modelinin bulundugu child obje.")]
    public Transform blade;
    [Tooltip("Zemindeki ray/oluk parcasi. Bos birakilabilir.")]
    public Transform rail;
    [Tooltip("SawTrapPlacer tarafindan otomatik atanir, elle doldurman gerekmez.")]
    public Transform player;

    [Header("Hareket")]
    [Tooltip("Gecidin toplam genisligi. Placer otomatik ayarlar.")]
    public float travelWidth = 3f;
    [Tooltip("Bir uctan digerine gidis suresi, saniye. Kucuk deger = hizli testere.")]
    public float cycleDuration = 0.7f;
    [Tooltip("Testerenin kendi ekseni etrafinda donus hizi (sadece gorsel, derece/sn).")]
    public float spinSpeed = 400f;
    [Tooltip("Donus ekseni. Testere yanlis eksende donuyorsa burayi degistir.")]
    public SpinAxis spinAxis = SpinAxis.Z;
    [Tooltip("Testerenin duvarlarla arasinda birakacagi bosluk (metre).")]
    public float wallClearance = 0.3f;

    [Header("Testere Yuksekligi")]
    [Tooltip("Acikken testere, zeminden tam olarak asagidaki kadar yukselecek sekilde gomulur.")]
    public bool autoSinkBlade = true;
    [Tooltip("Testerenin zeminden yukarida kalan kismi (metre). Oyuncunun ziplama yuksekliginden KUCUK olmali.")]
    public float bladeExposedHeight = 1.05f;

    [Header("Ray Ayari")]
    [Tooltip("Acikken ray otomatik boyutlanir, ortalanir ve zemine oturur.")]
    public bool autoFitRail = true;
    [Tooltip("Ray, testerenin gidis araligindan bu kadar uzun olsun (metre).")]
    public float railExtra = 0f;
    [Tooltip("Otomatik yerlestirmeden SONRA raya uygulanacak kaydirma. X = saga/sola.")]
    public Vector3 railOffset = Vector3.zero;

    [Header("Carpisma Ayarlari")]
    [Tooltip("Acikken carpisma mesafesi testerenin OLCULEN gercek boyutundan hesaplanir. " +
             "Boylece oyuncu ancak diske gercekten degdiginde olur.")]
    public bool autoHitRadius = true;
    [Tooltip("Oyuncunun govde yaricapi. Otomatik hesaba eklenir.")]
    public float playerRadius = 0.3f;
    [Tooltip("Otomatik hesabin uzerine eklenen pay. Negatif verirsen tuzak daha affedici olur.")]
    public float hitPadding = 0f;
    [Tooltip("Auto Hit Radius KAPALIYSA kullanilir: yatay tehlike mesafesi.")]
    public float hitRadiusX = 0.8f;
    [Tooltip("Auto Hit Radius KAPALIYSA kullanilir: gecit dogrultusunda tehlike mesafesi.")]
    public float hitRadiusZ = 1.2f;
    [Tooltip("Auto Sink Blade kapaliysa kullanilir. Aciksa bladeExposedHeight gecerlidir.")]
    public float bladeTopHeight = 0.55f;
    [Tooltip("Oyuncunun pivot noktasi ayaginda degilse buradan telafi et.")]
    public float playerFeetOffset = 0f;

    [Header("Hata Ayiklama")]
    [Tooltip("Acarsan Console'a surekli oyuncunun yuksekligini yazar, kalibrasyon icin.")]
    public bool debugLogHeight = false;
    [Tooltip("Acarsan olculen testere boyutlarini bir kez Console'a yazar.")]
    public bool logBladeMeasurements = false;

    public enum SpinAxis { X, Y, Z }

    private float timer;
    private bool hasHit;
    private float effectiveTopHeight;
    private float travelHalf;

    // Testerenin sabit yukseklik degeri — her karede tekrar hesaplanmaz
    private float bladeBaseY;

    // Modelin baslangictaki durusu ve biriken donus acisi.
    // Donus her karede bu ikisinden SIFIRDAN hesaplanir, uzerine eklenmez.
    private Quaternion bladeBaseRotation = Quaternion.identity;
    private float spinAngle;

    // Gercek carpisma mesafeleri — Start icinde hesaplanir
    private float effHitX;
    private float effHitZ;

    void Start()
    {
        effectiveTopHeight = bladeTopHeight;
        travelHalf = travelWidth * 0.5f;
        effHitX = hitRadiusX;
        effHitZ = hitRadiusZ;

        WarnIfScaleIsUneven();

        SetupBlade();
        if (autoFitRail) FitRail();
    }

    /// <summary>
    /// Donen bir objenin herhangi bir UST objesi esit olmayan olcekteyse,
    /// disk her karede farkli yonde ezilir ve titriyormus gibi gorunur.
    /// Bu kontrol sorunu tesbit edip Console'a net bir hata yazar.
    /// </summary>
    private void WarnIfScaleIsUneven()
    {
        Vector3 ls = transform.lossyScale;
        bool uneven = Mathf.Abs(ls.x - ls.y) > 0.001f || Mathf.Abs(ls.y - ls.z) > 0.001f;

        if (uneven)
        {
            Debug.LogError(
                $"[Testere] EŞİT OLMAYAN ÖLÇEK: {ls}. Dönen disk bu yüzden bozuk görünür. " +
                $"Üst objelerden birinin Scale'i eşit değil. Zincir: {BuildParentChain()}");
        }
    }

    private string BuildParentChain()
    {
        var sb = new System.Text.StringBuilder();
        Transform t = transform;
        while (t != null)
        {
            sb.Append($"{t.name}(scale={t.localScale}) <- ");
            t = t.parent;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Testereyi olcer, zemine gomer ve gidis araligini belirler.
    /// Modelin kendi pivotuna DOKUNMAZ — donus, modelcinin tasarladigi eksende kalir.
    /// Pivotu degistirmek, modelde disk disinda parca varsa yalpalamaya yol acar.
    /// </summary>
    private void SetupBlade()
    {
        if (blade == null) return;

        // Modelin tasarim durusunu sakla — donus hep bunun uzerine kurulur
        bladeBaseRotation = blade.localRotation;

        // Modeli orijin noktasinda olc
        blade.localPosition = Vector3.zero;
        Bounds b = MeasureBounds(transform, blade);

        if (autoSinkBlade)
        {
            // Modelin en ust noktasi tam bladeExposedHeight'ta kalsin
            bladeBaseY = bladeExposedHeight - b.max.y;
            effectiveTopHeight = bladeExposedHeight;
        }
        else
        {
            bladeBaseY = 0f;
        }

        // Diskin merkezden yanlara en uzak noktasi
        float halfSpanX = Mathf.Max(Mathf.Abs(b.max.x), Mathf.Abs(b.min.x));
        travelHalf = Mathf.Max(0f, travelWidth * 0.5f - halfSpanX - wallClearance);

        // Carpisma mesafesini diskin gercek olcusunden turet:
        // diskin yarim kalinligi/yaricapi + oyuncunun govde yaricapi.
        if (autoHitRadius)
        {
            effHitX = b.extents.x + playerRadius + hitPadding;
            effHitZ = b.extents.z + playerRadius + hitPadding;
        }

        blade.localPosition = new Vector3(0f, bladeBaseY, 0f);

        if (logBladeMeasurements)
        {
            Debug.Log($"[Testere-Olcum] {name} | boyut={b.size} merkez={b.center} " +
                      $"tepe={b.max.y:F2} yanAcikligi={halfSpanX:F2} " +
                      $"tabanY={bladeBaseY:F2} gidisYarisi={travelHalf:F2} " +
                      $"| carpismaX={effHitX:F2} carpismaZ={effHitZ:F2}");
        }
    }

    /// <summary>
    /// Rayi testerenin gercek gidis araligina gore boyutlar,
    /// tuzagin merkezine hizalar ve zemine oturtur.
    /// </summary>
    private void FitRail()
    {
        if (rail == null) return;

        rail.localScale = Vector3.one;
        rail.localPosition = Vector3.zero;
        Bounds own = MeasureBounds(rail, rail);
        Vector3 sz = own.size;
        if (sz.x <= 0.0001f && sz.y <= 0.0001f && sz.z <= 0.0001f) return;

        float target = travelHalf * 2f + railExtra;
        Vector3 s = Vector3.one;

        if (sz.x >= sz.y && sz.x >= sz.z) s.x = target / Mathf.Max(sz.x, 0.0001f);
        else if (sz.z >= sz.x && sz.z >= sz.y) s.z = target / Mathf.Max(sz.z, 0.0001f);
        else s.y = target / Mathf.Max(sz.y, 0.0001f);

        rail.localScale = s;

        Bounds inTrap = MeasureBounds(transform, rail);

        Vector3 lp = Vector3.zero;
        lp.x -= inTrap.center.x;
        lp.z -= inTrap.center.z;
        lp.y -= inTrap.min.y;
        lp += railOffset;

        rail.localPosition = lp;
    }

    /// <summary>
    /// contentRoot altindaki TUM mesh parcalarinin, measureSpace'in yerel eksenlerindeki
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

        if (!started) return new Bounds(Vector3.zero, Vector3.zero);
        return result;
    }

    void Update()
    {
        if (blade == null) return;

        timer += Time.deltaTime;
        float t = Mathf.PingPong(timer / cycleDuration, 1f);
        float localX = Mathf.Lerp(-travelHalf, travelHalf, t);

        // Konum: sadece X ekseninde gider gelir. Y sabit, Z her zaman sifir.
        blade.localPosition = new Vector3(localX, bladeBaseY, 0f);

        // Donus: birikmeli Rotate() yerine acidan sifirdan hesaplanir.
        // Boylece ondalik hatalar birikmez, eksen kaymaz, disk savrulmaz.
        Vector3 axis = spinAxis == SpinAxis.X ? Vector3.right
                     : spinAxis == SpinAxis.Y ? Vector3.up
                     : Vector3.forward;

        spinAngle = Mathf.Repeat(spinAngle + spinSpeed * Time.deltaTime, 360f);
        blade.localRotation = bladeBaseRotation * Quaternion.AngleAxis(spinAngle, axis);

        if (debugLogHeight && player != null)
        {
            float y = transform.InverseTransformPoint(player.position).y;
            Debug.Log($"[Testere-Kalibrasyon] Oyuncu local Y: {y:F2} | Testere tepesi: {effectiveTopHeight:F2}");
        }

        CheckHit();
    }

    private void CheckHit()
    {
        if (hasHit || player == null || blade == null) return;

        Vector3 local = transform.InverseTransformPoint(player.position);
        float bladeX = blade.localPosition.x;

        float dx = Mathf.Abs(local.x - bladeX);
        float dz = Mathf.Abs(local.z);

        if (dx > effHitX || dz > effHitZ) return;

        float feetY = local.y - playerFeetOffset;

        if (feetY < effectiveTopHeight)
        {
            hasHit = true;
            Debug.Log($"[Testere] Oyuncu carpti ({name}). feetY={feetY:F2} esik={effectiveTopHeight:F2}");
            OnPlayerHit?.Invoke(this);
        }
    }
}
