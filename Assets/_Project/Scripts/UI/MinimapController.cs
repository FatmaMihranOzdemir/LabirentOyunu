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
    public int cellPixels = 32;
    public int wallPixels = 4;
    [Tooltip("0 = sadece basılan hücre keşfedilir. Artırma, ileriyi gösterir.")]
    public int revealRadius = 0;
    [Tooltip("Kapalıysa harita sabit kalır, ok döner. Açıksa harita döner, ok hep yukarı bakar.")]
    public bool rotateWithPlayer = false;

    [Header("Renkler")]
    public Color unknownColor = new Color(0.03f, 0.03f, 0.05f, 0.8f);
    public Color floorColor = new Color(0.82f, 0.78f, 0.70f, 1f);
    public Color wallColor = new Color(0.06f, 0.06f, 0.08f, 1f);
    public Color playerColor = new Color(1f, 0.82f, 0.15f, 1f);
    public Color exitColor = new Color(0.30f, 0.92f, 0.45f, 1f);
    public Color borderColor = new Color(0.88f, 0.84f, 0.76f, 0.95f);

    private MazeGridData grid;
    private Texture2D texture;
    private Color[] buffer;
    private Vector2Int lastPlayerCell = new Vector2Int(int.MinValue, int.MinValue);
    private float lastYaw = float.MinValue;

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
        lastYaw = float.MinValue;
    }

    private void CreateTexture()
    {
        int side = (2 * viewRadius + 1) * cellPixels;

        texture = new Texture2D(side, side, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        buffer = new Color[side * side];

        if (minimapImage != null) minimapImage.texture = texture;
    }

    void Update()
    {
        if (grid == null || player == null || texture == null) return;

        float yaw = player.eulerAngles.y;

        if (minimapImage != null)
        {
            minimapImage.rectTransform.localRotation =
                rotateWithPlayer ? Quaternion.Euler(0f, 0f, yaw) : Quaternion.identity;
        }

        Vector2Int cell = mazeBuilder.WorldToCell(player.position);

        bool cellChanged = cell != lastPlayerCell;
        bool turned = Mathf.Abs(Mathf.DeltaAngle(lastYaw, yaw)) > 2f;

        if (!cellChanged && !turned) return;

        lastPlayerCell = cell;
        lastYaw = yaw;

        if (cellChanged) RevealAround(cell);
        Redraw(cell, yaw);
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

    private void Redraw(Vector2Int playerCell, float yaw)
    {
        int side = texture.width;

        for (int i = 0; i < buffer.Length; i++) buffer[i] = unknownColor;

        for (int dx = -viewRadius; dx <= viewRadius; dx++)
        {
            for (int dz = -viewRadius; dz <= viewRadius; dz++)
            {
                MazeCell cell = grid.GetCell(playerCell.x + dx, playerCell.y + dz);
                if (cell == null || !cell.PlayerVisited) continue;

                int px = (dx + viewRadius) * cellPixels;
                int pz = (dz + viewRadius) * cellPixels;

                Color baseColor = (cell.Type == CellType.Exit) ? exitColor : floorColor;
                FillBlock(side, px, pz, cellPixels, cellPixels, baseColor);

                if (cell.WallSouth) FillBlock(side, px, pz, cellPixels, wallPixels, wallColor);
                if (cell.WallNorth) FillBlock(side, px, pz + cellPixels - wallPixels, cellPixels, wallPixels, wallColor);
                if (cell.WallWest) FillBlock(side, px, pz, wallPixels, cellPixels, wallColor);
                if (cell.WallEast) FillBlock(side, px + cellPixels - wallPixels, pz, wallPixels, cellPixels, wallColor);
            }
        }

        DrawPlayerArrow(side, rotateWithPlayer ? 0f : yaw);
        ApplyCircularMask(side);

        texture.SetPixels(buffer);
        texture.Apply();
    }

    // Oku, oyuncunun baktığı yöne döndürerek çizer.
    private void DrawPlayerArrow(int side, float yawDegrees)
    {
        float cx = side / 2f;
        float cy = side / 2f;

        float h = Mathf.Max(8f, cellPixels * 1.3f);
        float halfW = Mathf.Max(3f, cellPixels * 0.45f);

        float rad = yawDegrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(rad);
        float cos = Mathf.Cos(rad);

        int extent = Mathf.CeilToInt(h);

        for (int y = -extent; y <= extent; y++)
        {
            for (int x = -extent; x <= extent; x++)
            {
                // piksel konumunu okun yerel uzayına çevir (yerel +y = bakış yönü)
                float localY = x * sin + y * cos;
                float localX = x * cos - y * sin;

                if (localY < -h / 2f || localY > h / 2f) continue;

                float t = (h / 2f - localY) / h;
                if (Mathf.Abs(localX) > halfW * t) continue;

                int px = Mathf.RoundToInt(cx + x);
                int py = Mathf.RoundToInt(cy + y);
                if (px < 0 || px >= side || py < 0 || py >= side) continue;

                buffer[py * side + px] = playerColor;
            }
        }
    }

    private void ApplyCircularMask(int side)
    {
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
                    buffer[i].a = 0f;
                }
                else if (dist >= borderInner)
                {
                    buffer[i] = borderColor;
                }
                else if (dist > fadeStart)
                {
                    float t = 1f - (dist - fadeStart) / (borderInner - fadeStart);
                    buffer[i].a *= t;
                }
            }
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
