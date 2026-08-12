using UnityEngine;
using TMPro;

public class MazeTimer : MonoBehaviour
{
    [Header("UI Referansları")]
    public TextMeshProUGUI timerText;

    [Header("Süre Ayarları")]
    public float totalTimeInSeconds = 180f; // 3 Dakika
    public float tensionThreshold = 30f;   // Son 30 saniye gerilim başlasın

    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public AudioClip tensionSound; // Dışarıdan atacağın ses dosyası

    private float currentTime;
    private bool isRunning = false;
    private float nextBeepTime = 0f;
    private float beepInterval = 1f;

    void Start()
    {
        currentTime = totalTimeInSeconds;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        // --- SON 30 SANİYE GERİLİM MANTIĞI ---
        if (currentTime <= tensionThreshold && currentTime > 0f)
        {
            // Süre azaldıkça ses çalma sıklığı hızlanır (1 sn -> 0.25 sn)
            beepInterval = Mathf.Lerp(0.25f, 1.0f, currentTime / tensionThreshold);

            if (Time.time >= nextBeepTime)
            {
                if (audioSource != null && tensionSound != null)
                {
                    audioSource.PlayOneShot(tensionSound);
                }
                nextBeepTime = Time.time + beepInterval;
            }

            // Pürüzsüz Sine Dalgası ile titremeyen Pulse efekti
            // 6f: Büyüyüp küçülme hızı | 0.08f: Büyüme miktarı (Gözü yormaz, akıcıdır)
            float scale = 1f + (Mathf.Sin(Time.time * 6f) * 0.08f);
            if (timerText != null)
            {
                timerText.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
        else
        {
            // 30 saniyenin dışındayken boyutu varsayılana sabitle
            if (timerText != null && timerText.transform.localScale != Vector3.one)
            {
                timerText.transform.localScale = Vector3.one;
            }
        }

        if (currentTime <= 0f)
        {
            currentTime = 0f;

            // Süre bittiğinde scale'i sıfırla ve durdur
            if (timerText != null) timerText.transform.localScale = Vector3.one;

            StopTimer();
            Debug.Log("SÜRE BİTTİ!");
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        int milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);

        timerText.text = string.Format("{0:00}:{1:00}<size=60%>{2:00}</size>", minutes, seconds, milliseconds);
    }

    public void StartTimer() => isRunning = true;

    public void StopTimer()
    {
        isRunning = false;
        if (timerText != null) timerText.transform.localScale = Vector3.one;
    }
}
