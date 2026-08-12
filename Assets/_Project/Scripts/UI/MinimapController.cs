using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Bağlantılar")]
    public MazeBuilder mazeBuilder;
    public Transform player;
    public RawImage minimapImage;

    [Header("Görünüm")]
    [Tooltip("Haritada oyuncunun çevresinde kaç hücre görünsün.")]
    public int viewRadius = 4;
    [Tooltip("Her hücrenin kaç piksel çizileceği. RawImage boyutu (2*viewRadius+1)*cellPixels olmalı.")]
    public int cellPixels = 32;
    public int wallPixels = 4;
    [Tooltip("0 = sadece basılan hücre keşfedilir. Artırma, ileriyi gösterir.")]
    public int revealRadius = 0;
    [Tooltip("Açıksa harita oyuncuyla birlikte döner, ekranın yukarısı hep 'ön' olur.")]
    public bool rotateWithPlayer = false;

    [Header("Renkler")]
    public Color unknownColor = new Color(0.03f, 0.03f, 0.05f, 0.8f);
    public Color floorColor = new Color(0.82f, 0.78f, 0.70f, 1f);
    public Color wallColor = new Color(0.06f, 0.06f, 0.08f, 1f);
    public Color playerColor = new Color(0.95f, 0.25f, 0.2f, 1f);
    public Color exitColor = new Color(0.30f, 0.92f, 0.45f, 1f);
    public Color borderColor = new Color(0.88f, 0.84f, 0.76f, 0.95f);

    [Header("Hata Ayıklama")]
    [Tooltip("Açarsan Console'a oyuncunun hesaplanan grid konumunu yazar.")]
    public bool logGridPosition = false;

    private MazeGridData grid;
    private Texture2D texture;
    private Color[] buffer;

    // Dairesel maske bir kez hesaplanır, her karede tekrar hesaplanmaz.
    private float[] maskAlpha;
    private bool[] maskIsBorder;

    private Vector2Int lastPlayerCell = new Vector2Int(int.MinValue, int.MinValue);
    private float lastGx = float.MinValue;
    private float lastGz = float.MinValue;

    // Grid eksenleri — MazeBuilder'ın kendi fonksiyonundan türetilir.
    private Vector3 gridOrigin;
    private Vector3 gridStepX;
    private Vector3 gridStepZ;
    private bool axesReady;

    void Start()
    {
        CreateTexture();

        if (mazeBuilder != null)
            mazeBuilder.SubscribeAndSyncNow(OnMazeReady);
        else
            Debug.LogWarning("[Minimap] MazeBuilder atanmamış.");
    }

    void OnDestroy()
    {
        if (mazeBuilder != null) mazeBuilder.Unsubscribe(OnMazeReady);
    }

    private void OnMazeReady(MazeGridData newGrid)
    {
        grid = newGrid;
        lastPlayerCell = new Vector2Int(int.MinValue, int.MinValue);
        lastGx = float.MinValue;
        lastGz = float.MinValue;

        CacheGridAxes();
    }

    /// <summary>
    /// MazeBuilder'a üç hücrenin dünya konumunu sorarak grid eksenlerini çıkarır.
    /// Böylece CellToWorldPosition ister transform kullansın ister kullanmasın,
    /// minimap her zaman labirentle aynı koordinat sistemini kullanır.
    /// </summary>
    private void CacheGridAxes()
    {
        if (mazeBuilder == null) { axesReady = false; return; }

        gridOrigin = mazeBuilder.CellToWorldPosition(0, 0);
        gridStepX = mazeBuilder.CellToWorldPosition(1, 0) - gridOrigin;
        gridStepZ = mazeBuilder.CellToWorldPosition(0, 1) - gridOrigin;

        axesReady = gridStepX.sqrMagnitude > 0.0001f && gridStepZ.sqrMagnitude > 0.0001f;

        if (!axesReady)
            Debug.LogWarning("[Minimap] Grid eksenleri hesaplanamadı. cellSize sıfır olabilir.");
    }

    private void CreateTexture()
    {
        int side = (2 * viewRadius + 1) * cellPixels;

        texture = new Texture2D(side, side, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        buffer = new Color[side * side];
        BuildMask(side);

        if (minimapImage != null) minimapImage.texture = texture;
    }

    private void BuildMask(int side)
    {
        maskAlpha = new float[side * side];
        maskIsBorder = new bool[side * side];

        float center = side / 2f;
        float radius = side / 2f;
        float fadeStart = radius * 0.88f;
        float borderInner = radius - 3f;

        for (int y = 0; y < side; y++)
        {
            for (int x = 0; x < side; x++)
            {
                float dx = x + 0.5f - center;
                float dy = y + 0.5f - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                int i = y * side + x;

                if (dist > radius)
                {
                    maskAlpha[i] = 0f;
                }
                else if (dist >= borderInner)
                {
                    maskIsBorder[i] = true;
                    maskAlpha[i] = 1f;
                }
                else if (dist > fadeStart)
                {
                    maskAlpha[i] = 1f - (dist - fadeStart) / (borderInner - fadeStart);
                }
                else
                {
                    maskAlpha[i] = 1f;
                }
            }
        }
    }

    void Update()
    {
        if (grid == null || player == null || texture == null || mazeBuilder == null) return;
        if (!axesReady) CacheGridAxes();
        if (!axesReady) return;

        if (minimapImage != null)
        {
            float yaw = player.eulerAngles.y;
            minimapImage.rectTransform.localRotation =
                rotateWithPlayer ? Quaternion.Euler(0f, 0f, yaw) : Quaternion.identity;
        }

        // Oyuncunun grid üzerindeki KESİRLİ konumu.
        // Labirentin gerçek eksenlerine izdüşüm alınır — transform kayması olsa bile doğru sonuç verir.
        Vector3 rel = player.position - gridOrigin;
        float gx = Vector3.Dot(rel, gridStepX) / gridStepX.sqrMagnitude;
        float gz = Vector3.Dot(rel, gridStepZ) / gridStepZ.sqrMagnitude;

        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(gx), Mathf.RoundToInt(gz));

        if (logGridPosition)
            Debug.Log($"[Minimap] gx={gx:F2} gz={gz:F2} → hücre ({cell.x},{cell.y})");

        if (cell != lastPlayerCell)
        {
            lastPlayerCell = cell;
            RevealAround(cell);
        }
        else if (Mathf.Abs(gx - lastGx) < 0.004f && Mathf.Abs(gz - lastGz) < 0.004f)
        {
            return; // oyuncu neredeyse hiç kımıldamadı, yeniden çizmeye gerek yok
        }

        lastGx = gx;
        lastGz = gz;

        Redraw(cell, gx - cell.x, gz - cell.y);
    }

    private void RevealAround(Vector2Int center)
    {
        for (int dx = -revealRadius; dx <= revealRadius; dx++)
        {
            for (int dz = -revealRadius; dz <= revealRadius; dz++)
            {
                MazeCell cell = grid.GetCell(center.x + dx, center.y + dz);
                if (cell != null) cell.PlayerVisited = true;
            }
        }
    }

    // fracX / fracZ: oyuncunun hücre merkezine göre kesirli sapması (-0.5 .. 0.5)
    private void Redraw(Vector2Int playerCell, float fracX, float fracZ)
    {
        int side = texture.width;

        int offX = Mathf.RoundToInt(fracX * cellPixels);
        int offZ = Mathf.RoundToInt(fracZ * cellPixels);

        for (int i = 0; i < buffer.Length; i++) buffer[i] = unknownColor;

        // Kenarlarda boşluk kalmasın diye bir hücre fazladan çiziyoruz.
        int range = viewRadius + 1;

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dz = -range; dz <= range; dz++)
            {
                MazeCell cell = grid.GetCell(playerCell.x + dx, playerCell.y + dz);
                if (cell == null || !cell.PlayerVisited) continue;

                int px = (dx + viewRadius) * cellPixels - offX;
                int pz = (dz + viewRadius) * cellPixels - offZ;

                Color baseColor = (cell.Type == CellType.Exit) ? exitColor : floorColor;
                FillBlock(side, px, pz, cellPixels, cellPixels, baseColor);

                if (cell.WallSouth) FillBlock(side, px, pz, cellPixels, wallPixels, wallColor);
                if (cell.WallNorth) FillBlock(side, px, pz + cellPixels - wallPixels, cellPixels, wallPixels, wallColor);
                if (cell.WallWest) FillBlock(side, px, pz, wallPixels, cellPixels, wallColor);
                if (cell.WallEast) FillBlock(side, px + cellPixels - wallPixels, pz, wallPixels, cellPixels, wallColor);
            }
        }

        DrawPlayerMarker(side);
        ApplyMask();

        texture.SetPixels(buffer);
        texture.Apply();
    }

    private void DrawPlayerMarker(int side)
    {
        float cx = side / 2f;
        float cy = side / 2f;
        float radius = Mathf.Max(3f, cellPixels * 0.32f);
        float radiusSq = radius * radius;

        int extent = Mathf.CeilToInt(radius) + 1;

        for (int y = -extent; y <= extent; y++)
        {
            for (int x = -extent; x <= extent; x++)
            {
                if (x * x + y * y > radiusSq) continue;

                int px = Mathf.RoundToInt(cx + x);
                int py = Mathf.RoundToInt(cy + y);
                if (px < 0 || px >= side || py < 0 || py >= side) continue;

                buffer[py * side + px] = playerColor;
            }
        }
    }

    private void ApplyMask()
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            if (maskIsBorder[i])
            {
                buffer[i] = borderColor;
                continue;
            }

            buffer[i].a *= maskAlpha[i];
        }
    }

    private void FillBlock(int side, int x0, int y0, int bw, int bh, Color c)
    {
        for (int y = y0; y < y0 + bh; y++)
        {
            if (y < 0 || y >= side) continue;
            for (int x = x0; x < x0 + bw; x++)
            {
                if (x < 0 || x >= side) continue;
                buffer[y * side + x] = c;
            }
        }
    }
}
