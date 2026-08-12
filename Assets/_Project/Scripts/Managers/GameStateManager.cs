using System.Collections;
using UnityEngine;

/// <summary>
/// Olum ve kazanma akisini yonetir.
/// Kapi tuzagi, testere tuzagi, ok tuzagi ve cikis noktasi buraya event ile haber verir.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public MazeBuilder mazeBuilder;
    public GameObject deathScreen;
    public float restartDelay = 2f;
    public MonoBehaviour[] disableWhileGameOver;

    private bool isGameOver;

    void OnEnable()
    {
        SlidingDoor.OnPlayerCrushed += HandlePlayerCrushed;
        SawTrap.OnPlayerHit += HandleSawHit;
        ArrowTrap.OnPlayerHit += HandleArrowHit;
        ExitMarker.OnExitReached += HandleExitReached;
    }

    void OnDisable()
    {
        SlidingDoor.OnPlayerCrushed -= HandlePlayerCrushed;
        SawTrap.OnPlayerHit -= HandleSawHit;
        ArrowTrap.OnPlayerHit -= HandleArrowHit;
        ExitMarker.OnExitReached -= HandleExitReached;
    }

    void Start()
    {
        if (deathScreen != null) deathScreen.SetActive(false);
    }

    private void HandlePlayerCrushed(SlidingDoor door) => Die();
    private void HandleSawHit(SawTrap saw) => Die();
    private void HandleArrowHit(ArrowTrap arrow) => Die();

    private void Die()
    {
        if (isGameOver) return;
        isGameOver = true;

        SetPlayerControlEnabled(false);
        if (deathScreen != null) deathScreen.SetActive(true);
        StartCoroutine(RestartAfterDelay());
    }

    private void HandleExitReached()
    {
        if (isGameOver) return;
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
