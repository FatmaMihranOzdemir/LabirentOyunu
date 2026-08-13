using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject PauseMenuObject;

    [Header("Pause sırasında kapanacak karakter scriptleri")]
    public MonoBehaviour[] PlayerScripts;

    private bool isPaused;

    private void Start()
    {
        ShowMenu(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMenu(!isPaused);
        }
    }

    private void LateUpdate()
    {
        // Karakter scripti tekrar kilitlese bile fareyi serbest tutar.
        if (isPaused)   
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void ShowMenu(bool show)
    {
        isPaused = show;

        PauseMenuObject.SetActive(show);
        Time.timeScale = show ? 0f : 1f;

        Cursor.lockState = show
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = show;

        foreach (MonoBehaviour playerScript in PlayerScripts)
        {
            if (playerScript != null)
                playerScript.enabled = !show;
        }
    }

    public void Resume()
    {
        ShowMenu(false);
    }

    public void ToMenu(int sceneIndex)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}