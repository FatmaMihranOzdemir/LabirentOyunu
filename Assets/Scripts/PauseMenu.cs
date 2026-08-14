using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject PauseMenuObject; // Pause Menu altındaki Canvas objesi

    [Header("Player (Karakter)")]
    public GameObject playerArmature; // Karakterin kendisi

    private ThirdPersonController controller;
    private StarterAssetsInputs starterInputs;
    private bool isPaused;

    private void Start()
    {
        if (playerArmature != null)
        {
            controller = playerArmature.GetComponent<ThirdPersonController>();
            starterInputs = playerArmature.GetComponent<StarterAssetsInputs>();
        }

        ShowMenu(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMenu(!isPaused);
        }

        // Menü açıkken fareyi ve kamerayı zorla serbest/kilitli tut
        if (isPaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (starterInputs != null)
            {
                starterInputs.look = Vector2.zero;
                starterInputs.move = Vector2.zero;
            }
        }
    }

    public void ShowMenu(bool show)
    {
        isPaused = show;

        if (PauseMenuObject != null)
            PauseMenuObject.SetActive(show);

        // 1. Zamanı durdur
        Time.timeScale = show ? 0f : 1f;

        // 2. StarterAssets girdi ve kamera kilidini kapat
        if (starterInputs != null)
        {
            starterInputs.cursorLocked = !show;
            starterInputs.cursorInputForLook = !show; // Kameranın dönmesini engeller!
        }

        if (controller != null)
        {
            controller.enabled = !show; // Hareketi durdurur
        }

        // 3. Fare imlecini aç/kapat
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = show;
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
