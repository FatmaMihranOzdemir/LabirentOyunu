using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using StarterAssets;

public class EndScreen : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject endScreen;
    public TMP_Text resultText;

    [Header("Oyun bitince kapatılacak scriptler")]
    public MonoBehaviour[] PlayerScripts;

    [Header("Pause Menu")]
    public MonoBehaviour PauseMenuScript;

    private bool isEnded = false;

    private void Start()
    {
        if (endScreen != null)
            endScreen.SetActive(false);
    }

    public void ShowWin()
    {
        if (isEnded) return;
        isEnded = true;

        Debug.Log("<color=green>YOU WON EKRANI SABİTLENDİ!</color>");

        if (resultText != null)
            resultText.text = "";

        // 1. Ekranı zorla aç
        if (endScreen != null)
            endScreen.SetActive(true);

        // 2. Zamanı anında durdur (böylece hiçbir sayaç veya gecikmeli kod sahneyi yenileyemez)
        Time.timeScale = 0f;

        // 3. Karakter hareket ve kamera girdilerini kökten kapat
        StarterAssetsInputs starterInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.cursorLocked = false;
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        foreach (MonoBehaviour playerScript in PlayerScripts)
        {
            if (playerScript != null)
                playerScript.enabled = false;
        }

        if (PauseMenuScript != null)
            PauseMenuScript.enabled = false;

        // 4. Fare imlecini ekrana kilitle ve görünür yap
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowLose()
    {
        if (isEnded) return;
        isEnded = true;

        if (resultText != null)
            resultText.text = "";

        if (endScreen != null)
            endScreen.SetActive(true);

        Time.timeScale = 0f;

        StarterAssetsInputs starterInputs = Object.FindAnyObjectByType<StarterAssetsInputs>();
        if (starterInputs != null)
        {
            starterInputs.cursorLocked = false;
            starterInputs.cursorInputForLook = false;
            starterInputs.move = Vector2.zero;
            starterInputs.look = Vector2.zero;
        }

        foreach (MonoBehaviour playerScript in PlayerScripts)
        {
            if (playerScript != null)
                playerScript.enabled = false;
        }

        if (PauseMenuScript != null)
            PauseMenuScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Buton Fonksiyonları
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
