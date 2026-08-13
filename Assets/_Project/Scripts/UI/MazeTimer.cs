using UnityEngine;
using TMPro;

/// <summary>
/// Geri sayim sayaci.
/// Sure bittiginde OnTimeUp olayini tetikler; GameStateManager bunu dinleyip
/// olum akisini baslatir. Labirent yenilendiginde ResetTimer ile sifirlanir.
/// </summary>
public class MazeTimer : MonoBehaviour
{
    /// <summary>Sure sifirlandiginda bir kez tetiklenir.</summary>
    public static event System.Action OnTimeUp;

    [Header("UI Referansları")]
    public TextMeshProUGUI timerText;

    [Header("Süre Ayarları")]
    public float totalTimeInSeconds = 180f; // 3 Dakika
    public float tensionThreshold = 30f;    // Son 30 saniye gerilim başlasın

    [Header("Ses Ayarları")]
    public AudioSource audioSource;
    public AudioClip tensionSound;
    public AudioClip bgMusicClip;           // Arka plan müziği (MP3/WAV ses dosyası)

    private float currentTime;
    private bool isRunning = false;
    private bool timeUpFired = false;
    private float nextBeepTime = 0f;
    private float beepInterval = 1f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        ResetTimer();
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
                    audioSource.PlayOneShot(tensionSound);

                nextBeepTime = Time.time + beepInterval;
            }

            // Pürüzsüz sine dalgası ile titremeyen pulse efekti
            float scale = 1f + (Mathf.Sin(Time.time * 6f) * 0.08f);
            if (timerText != null)
                timerText.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            if (timerText != null && timerText.transform.localScale != Vector3.one)
                timerText.transform.localScale = Vector3.one;
        }

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            UpdateTimerUI();
            StopTimer();

            // Sadece bir kez haber ver
            if (!timeUpFired)
            {
                timeUpFired = true;
                Debug.Log("[Sayac] SÜRE BİTTİ — ölüm tetikleniyor.");
                OnTimeUp?.Invoke();
            }

            return;
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(currentTime / 60F);
        int seconds = Mathf.FloorToInt(currentTime % 60F);
        int milliseconds = Mathf.FloorToInt((currentTime * 100F) % 100F);

        timerText.text = string.Format("{0:00}:{1:00}<size=60%>{2:00}</size>",
                                       minutes, seconds, milliseconds);
    }

    public void StartTimer()
    {
        isRunning = true;

        // Arka plan müziğini direkt klip olarak AudioSource üzerinden çalıyoruz
        if (audioSource != null && bgMusicClip != null)
        {
            audioSource.clip = bgMusicClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void StopTimer()
    {
        isRunning = false;
        if (timerText != null) timerText.transform.localScale = Vector3.one;

        // Süre bittiğinde veya sayaç durduğunda müziği kes
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    /// <summary>
    /// Sayaci basa sarar ve calistirir.
    /// Labirent her yenilendiginde GameStateManager tarafindan cagrilir.
    /// </summary>
    public void ResetTimer()
    {
        currentTime = totalTimeInSeconds;
        timeUpFired = false;
        nextBeepTime = 0f;

        if (timerText != null) timerText.transform.localScale = Vector3.one;

        UpdateTimerUI();
        StartTimer();
    }
}
