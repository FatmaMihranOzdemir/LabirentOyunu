using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    public GameObject endScreen;
    public TMP_Text resultText;

    [Header("Oyun bitince kapatılacak scriptler")]
    public MonoBehaviour[] PlayerScripts;

    [Header("Pause Menu")]
    public MonoBehaviour PauseMenuScript;

    private void Start()
    {
        endScreen.SetActive(false);
    }

    public void ShowWin()
    {
        Debug.Log("YOU WON ÇALIŞTI!");

        resultText.text = "";
        endScreen.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Karakter ve kamera kontrolünü kapat
        foreach (MonoBehaviour playerScript in PlayerScripts)
        {
            if (playerScript != null)
                playerScript.enabled = false;
        }

        // ESC ile Pause Menu açılmasın
        if (PauseMenuScript != null)
            PauseMenuScript.enabled = false;
    }

    public void ShowLose()
    {
        resultText.text = "";
        endScreen.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (MonoBehaviour playerScript in PlayerScripts)
        {
            if (playerScript != null)
                playerScript.enabled = false;
        }

        if (PauseMenuScript != null)
            PauseMenuScript.enabled = false;
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}