using UnityEngine;

[ExecuteAlways]
public class MazeForestGenerator : MonoBehaviour
{
    [Header("Bağlantılar")]
    public MazeBuilder mazeBuilder;

    [Header("Ağaç Ayarları")]
    public GameObject[] treePrefabs;

    [Header("Orman Sınır Ayarları (Sabit Labirent İçin)")]
    [Tooltip("Labirentin bittiği maksimum X koordinatı (İçeri girerse artır)")]
    public float mazeMaxX = 60f;
    [Tooltip("Labirentin bittiği maksimum Z koordinatı (İçeri girerse artır)")]
    public float mazeMaxZ = 60f;
    [Tooltip("Duvarlardan kaç birim uzaktan orman başlasın?")]
    public float safetyMargin = 6f;

    [Header("Yoğunluk Ayarları")]
    [Tooltip("Dışarıya doğru kaç metre boyunca ağaç dikilsin?")]
    public float forestWidth = 30f;
    [Tooltip("Ağaçların arasındaki mesafe")]
    public float treeDensity = 2.5f;
    [Tooltip("Doğallık sapması")]
    public float randomness = 0.6f;

    private void OnEnable()
    {
        if (mazeBuilder != null)
        {
            mazeBuilder.Unsubscribe(OnMazeReady);
            mazeBuilder.SubscribeAndSyncNow(OnMazeReady);
        }
    }

    private void OnDisable()
    {
        if (mazeBuilder != null)
        {
            mazeBuilder.Unsubscribe(OnMazeReady);
        }
    }

    void OnMazeReady(MazeGridData grid)
    {
        ClearForest();

        if (treePrefabs == null || treePrefabs.Length == 0) return;

        GameObject forestContainer = new GameObject("GeneratedForest");
        forestContainer.transform.parent = this.transform;

        // Forest objesinin pozisyonundan bağımsız dünya koordinatı için burayı sıfırlıyoruz
        forestContainer.transform.localPosition = Vector3.zero;

        // Labirentin güvenli dış sınırlarını belirliyoruz
        float minSafeX = -safetyMargin;
        float maxSafeX = mazeMaxX + safetyMargin;
        float minSafeZ = -safetyMargin;
        float maxSafeZ = mazeMaxZ + safetyMargin;

        // Ağaçların dikileceği en dış çerçeve sınırları
        float startX = minSafeX - forestWidth;
        float endX = maxSafeX + forestWidth;
        float startZ = minSafeZ - forestWidth;
        float endZ = maxSafeZ + forestWidth;

        for (float x = startX; x <= endX; x += treeDensity)
        {
            for (float z = startZ; z <= endZ; z += treeDensity)
            {
                // EĞER koordinat güvenli bölgenin İÇİNDEYSE burayı tamamen atla!
                if (x >= minSafeX && x <= maxSafeX && z >= minSafeZ && z <= maxSafeZ)
                {
                    continue;
                }

                // Hafif rastgelelik
                float posX = x + Random.Range(-randomness, randomness);
                float posZ = z + Random.Range(-randomness, randomness);
                Vector3 spawnPos = new Vector3(posX, 0f, posZ);

                GameObject selectedTreePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                if (selectedTreePrefab == null) continue;

                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                GameObject tree = Instantiate(selectedTreePrefab, spawnPos, randomRotation, forestContainer.transform);

                float randomScale = Random.Range(0.8f, 1.4f);
                tree.transform.localScale *= randomScale;
            }
        }
    }

    public void ClearForest()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }
}
