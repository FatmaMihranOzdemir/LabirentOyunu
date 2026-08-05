using UnityEngine;

[ExecuteAlways]
public class MazeForestGenerator : MonoBehaviour
{
    [Header("Bağlantılar")]
    public MazeBuilder mazeBuilder;

    [Header("Ağaç Ayarları")]
    public GameObject[] treePrefabs;

    [Header("Orman Sınır Ayarları")]
    [Tooltip("Duvarlardan kaç birim uzaktan orman başlasın?")]
    public float safetyMargin = 2f;
    [Tooltip("Dışarıya doğru kaç metre boyunca ağaç dikilsin?")]
    public float forestWidth = 30f;

    [Header("Yoğunluk ve Doğallık")]
    [Tooltip("Ağaçlar arası mesafe (Küçüldükçe sıklaşır, ideal: 3 - 4)")]
    [Range(2f, 8f)]
    public float treeSpacing = 3.5f;
    [Tooltip("Doğallık sapması")]
    public float randomness = 0.8f;

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

        if (treePrefabs == null || treePrefabs.Length == 0 || grid == null) return;

        GameObject forestContainer = new GameObject("GeneratedForest");
        forestContainer.transform.parent = this.transform;
        forestContainer.transform.localPosition = Vector3.zero;

        // BURA DEĞİŞTİ: Labirentin GERÇEK boyutlarını dinamik hesaplıyoruz!
        float calculatedMaxX = grid.Width * mazeBuilder.cellSize;
        float calculatedMaxZ = grid.Height * mazeBuilder.cellSize;

        // Labirentin güvenli dış sınırları
        float minSafeX = -safetyMargin;
        float maxSafeX = calculatedMaxX + safetyMargin;
        float minSafeZ = -safetyMargin;
        float maxSafeZ = calculatedMaxZ + safetyMargin;

        // Ağaçların dikileceği en dış çerçeve sınırları
        float startX = minSafeX - forestWidth;
        float endX = maxSafeX + forestWidth;
        float startZ = minSafeZ - forestWidth;
        float endZ = maxSafeZ + forestWidth;

        // treeSpacing ile güvenli adım atıyoruz
        for (float x = startX; x <= endX; x += treeSpacing)
        {
            for (float z = startZ; z <= endZ; z += treeSpacing)
            {
                // Koordinat güvenli bölgenin İÇİNDEYSE pas geç!
                if (x >= minSafeX && x <= maxSafeX && z >= minSafeZ && z <= maxSafeZ)
                {
                    continue;
                }

                // Rastgele kaydırma
                float posX = x + Random.Range(-randomness, randomness);
                float posZ = z + Random.Range(-randomness, randomness);
                Vector3 spawnPos = new Vector3(posX, 0f, posZ);

                GameObject selectedTreePrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                if (selectedTreePrefab == null) continue;

                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                GameObject tree = Instantiate(selectedTreePrefab, spawnPos, randomRotation, forestContainer.transform);

                float randomScale = Random.Range(0.8f, 1.4f);
                tree.transform.localScale *= randomScale;

                // İçinden geçilmesini önlemek için Collider kontrolü ve boyutlandırması
                if (tree.GetComponent<Collider>() == null)
                {
                    CapsuleCollider col = tree.AddComponent<CapsuleCollider>();
                    col.radius = 0.3f; // Yarıçapı küçülttük ki görünmez duvar hissi vermesin
                    col.height = 4f;
                    col.center = new Vector3(0, 2f, 0);
                }
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
