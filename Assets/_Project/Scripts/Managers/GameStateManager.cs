using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class GameStateManager : MonoBehaviour
{
    [Header("Paneller (UI)")]
    [Tooltip("Tuzakta ölünce açılacak panel")]
    public GameObject deathScreen;

    [Tooltip("Süre bittiğinde açılacak panel")]
    public GameObject timeUpScreen;

    [Header("Player & Zamanlayici")]
    public GameObject playerArmature;
    public MazeTimer mazeTimer;

    [Header("Hata Ayiklama")]
    public bool logDebugInfo = true;

    private ThirdPersonController controller;
    private StarterAssetsInputs starterInputs;
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
        Time.timeScale = 1f;

        if (playerArmature != null)
        {
            controller = playerArmature.GetComponent<ThirdPersonController>();
            starterInputs = playerArmature.GetComponent<StarterAssetsInputs>();
        }

        // Başlarken iki paneli de kapat
        if (deathScreen != null) deathScreen.SetActive(false);
        if (timeUpScreen != null) timeUpScreen.SetActive(false);
    }

    // --- TUZAK ÖLÜMLERİ ---
    private void HandlePlayerCrushed(SlidingDoor door) => TriggerDeath("Kapıya sıkıştı");
    private void HandleSawHit(SawTrap saw) => TriggerDeath("Testereye çarptı");
    private void HandleArrowHit(ArrowTrap arrow) => TriggerDeath("Ok vurdu");

    private void TriggerDeath(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        if (logDebugInfo) Debug.Log($"[Oyun] ÖLÜM: {reason}");

        if (mazeTimer != null) mazeTimer.StopTimer();
        if (deathScreen != null) deathScreen.SetActive(true);

        FreezeGame();
    }

    // --- SÜRE BİTİMİ ---
    private void HandleTimeUp()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (logDebugInfo) Debug.Log("[Oyun] SÜRE BİTTİ!");

        if (mazeTimer != null) mazeTimer.StopTimer();
        if (timeUpScreen != null) timeUpScreen.SetActive(true);

        FreezeGame();
    }

    // --- KAZANMA ---
    private void HandleExitReached()
    {
        if (isGameOver) return;
        if (logDebugInfo) Debug.Log("[Oyun] KAZANDIN! Çıkışa ulaşıldı.");

        // RestartGame(); // KAPATTIK!
    }

    // Oyunu durdurup fareyi UI için serbest bırakan ortak metod
    private void FreezeGame()
    {
        Time.timeScale = 0f;

        if (controller != null) controller.enabled = false;

        if (starterInputs != null)
        {
            starterInputs.cursorLocked = false;
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- BUTON FONKSİYONLARI ---
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(1); // Main sahnesi (1)
    }

    public void ToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Main Menu sahnesi (0)
    }
}
