using UnityEngine;

public class BatSwarmManager : MonoBehaviour
{
    [Header("Yarasa Ayarları")]
    public GameObject batPrefab;
    public int batCount = 40;
    public RuntimeAnimatorController batAnimatorController;

    [Header("Uçuş Alanı ve Boyut")]
    public float baseScale = 12f;
    public float flyRadius = 45f;       // Alanı biraz daha genişlettik
    public float flyHeight = 15f;
    public float minSpeed = 12f;
    public float maxSpeed = 22f;

    private Transform[] bats;
    private float[] speeds;
    private float[] radii;
    private float[] angles;
    private float[] yOffsets;
    private float[] waveOffsets;

    void Start()
    {
        if (batPrefab == null) return;

        bats = new Transform[batCount];
        speeds = new float[batCount];
        radii = new float[batCount];
        angles = new float[batCount];
        yOffsets = new float[batCount];
        waveOffsets = new float[batCount];

        for (int i = 0; i < batCount; i++)
        {
            // Yarasaların açılarını 360 dereceye eşit ve aralıklı dağıtıyoruz
            angles[i] = (360f / batCount) * i + Random.Range(-10f, 10f);

            // Yarıçapları geniş bir banda yayıyoruz (üst üste binmezler)
            radii[i] = Random.Range(15f, flyRadius);
            speeds[i] = Random.Range(minSpeed, maxSpeed);

            // Yükseklik aralıklarını daha geniş tuttuk (-4m ile +6m arası)
            yOffsets[i] = Random.Range(-4f, 6f);
            waveOffsets[i] = Random.Range(0f, 100f);

            GameObject bat = Instantiate(batPrefab, transform);
            bats[i] = bat.transform;

            float randomScale = Random.Range(baseScale * 0.8f, baseScale * 1.2f);
            bat.transform.localScale = Vector3.one * randomScale;

            Animator anim = bat.GetComponent<Animator>();
            if (anim == null) anim = bat.AddComponent<Animator>();

            if (batAnimatorController != null)
            {
                anim.runtimeAnimatorController = batAnimatorController;
                anim.Play(0, -1, Random.Range(0f, 1f));
            }
        }
    }

    void Update()
    {
        Vector3 center = transform.position;

        for (int i = 0; i < batCount; i++)
        {
            if (bats[i] == null) continue;

            angles[i] += speeds[i] * Time.deltaTime;
            float rad = angles[i] * Mathf.Deg2Rad;

            float x = center.x + Mathf.Cos(rad) * radii[i];
            float z = center.z + Mathf.Sin(rad) * radii[i];
            // Her yarasa farklı zamanlarda aşağı/yukarı süzülür
            float y = center.y + flyHeight + yOffsets[i] + Mathf.Sin(Time.time * 1.5f + waveOffsets[i]) * 1.2f;

            Vector3 nextPos = new Vector3(x, y, z);

            Vector3 moveDir = nextPos - bats[i].position;
            if (moveDir != Vector3.zero)
            {
                bats[i].rotation = Quaternion.LookRotation(moveDir) * Quaternion.Euler(0, 90f, 0);
            }

            bats[i].position = nextPos;
        }
    }
}
