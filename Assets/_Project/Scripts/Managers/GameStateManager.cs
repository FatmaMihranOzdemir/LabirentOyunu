using System.Collections;
using UnityEngine;

/// <summary>
/// Olum ve kazanma akisini yonetir.
///
/// Olum sebepleri: kapi tuzagi, testere tuzagi, ok tuzagi, surenin bitmesi.
/// Kazanma: cikisa ulasmak.
/// Hepsi buraya event ile haber verir, tek bir akista toplanir.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    [Header("Baglantilar")]
    public MazeBuilder mazeBuilder;
    [Tooltip("Olunce acilacak panel. Bos birakirsan ekran gosterilmez ama oyun yine yeniden baslar.")]
    public GameObject deathScreen;
    [Tooltip("Geri sayim sayaci. Bos birakilabilir, o zaman sure ile olum calismaz.")]
    public MazeTimer mazeTimer;

    [Header("Ayarlar")]
    [Tooltip("Olum ekrani bu kadar saniye gosterilir, sonra yeni labirent uretilir.")]
    public float restartDelay = 2f;
    [Tooltip("Olum aninda kapatilacak scriptler. Genelde ThirdPersonController.")]
    public MonoBehaviour[] disableWhileGameOver;

    [Header("Hata Ayiklama")]
    public bool logDebugInfo = true;

    private bool isGameOver;

    void OnEnable()
    {
        SlidingDoor.OnPlayerCrushed += HandlePlayerCrushed;
        SawTrap.OnPlayerHit += HandleSawHit;
        ArrowTrap.OnPlayerHit += HandleArrowHit;
        MazeTimer.OnTimeUp += HandleTimeUp;
        ExitMarker.OnExitReached += HandleExitReached;
    }

    void OnDisable()
    {
        SlidingDoor.OnPlayerCrushed -= HandlePlayerCrushed;
        SawTrap.OnPlayerHit -= HandleSawHit;
        ArrowTrap.OnPlayerHit -= HandleArrowHit;
        MazeTimer.OnTimeUp -= HandleTimeUp;
        ExitMarker.OnExitReached -= HandleExitReached;
    }

    void Start()
    {
        if (deathScreen != null) deathScreen.SetActive(false);

        // Kurulum hatalarini oyun baslar baslamaz bildir
        if (mazeBuilder == null)
            Debug.LogError("[Oyun] Maze Builder atanmamis! Olumden sonra yeni labirent uretilemez.");

        if (deathScreen == null)
            Debug.LogWarning("[Oyun] Death Screen atanmamis. Olunce ekran gosterilmeyecek.");

        if (mazeTimer == null)
            Debug.LogWarning("[Oyun] Maze Timer atanmamis. Sure bitince olum calismayacak, " +
                             "sayac da yeniden baslamayacak.");

        if (disableWhileGameOver == null || disableWhileGameOver.Length == 0)
            Debug.LogWarning("[Oyun] Disable While Game Over bos. Olduktan sonra oyuncu " +
                             "hareket etmeye devam edebilir.");
    }

    private void HandlePlayerCrushed(SlidingDoor door) => Die("kapiya sikisti");
    private void HandleSawHit(SawTrap saw) => Die("testereye carpti");
    private void HandleArrowHit(ArrowTrap arrow) => Die("ok vurdu");
    private void HandleTimeUp() => Die("sure bitti");

    private void Die(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (logDebugInfo)
            Debug.Log($"[Oyun] OLUM — {reason}. {restartDelay} saniye sonra yeni labirent.");

        SetPlayerControlEnabled(false);

        if (mazeTimer != null) mazeTimer.StopTimer();
        if (deathScreen != null) deathScreen.SetActive(true);

        StartCoroutine(RestartAfterDelay());
    }

    private void HandleExitReached()
    {
        if (isGameOver) return;

        if (logDebugInfo)
            Debug.Log("[Oyun] KAZANDIN — cikisa ulasildi, yeni labirent uretiliyor.");

        Restart();
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        Restart();
    }

    public void Restart()
    {
        if (deathScreen != null) deathScreen.SetActive(false);
        if (mazeBuilder != null) mazeBuilder.BuildMaze();
        if (mazeTimer != null) mazeTimer.ResetTimer();

        SetPlayerControlEnabled(true);
        isGameOver = false;
    }

    private void SetPlayerControlEnabled(bool value)
    {
        if (disableWhileGameOver == null) return;

        foreach (var script in disableWhileGameOver)
            if (script != null) script.enabled = value;
    }
}
