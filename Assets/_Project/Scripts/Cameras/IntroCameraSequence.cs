using UnityEngine;

public class IntroCameraSequence : MonoBehaviour
{
    [Header("Kameraların Standart GameObject'lerini Sürükleyin")]
    public GameObject introCamObject;
    public GameObject followCamObject;

    [Header("İntro Süresi (Saniye)")]
    public float introDuration = 4f;

    void Start()
    {
        // Zaman akışını kesin olarak başlat
        Time.timeScale = 1f;

        if (introCamObject != null && followCamObject != null)
        {
            // İntro kamerasını aç, ana takibi kapat
            introCamObject.SetActive(true);
            followCamObject.SetActive(false);

            // Belirtilen süre sonra kameraları değiştir
            Invoke(nameof(SwitchToFollowCam), introDuration);
        }
        else
        {
            Debug.LogWarning("IntroCameraSequence: Kameralar Inspector'da atanmamış! İntro atlandı.");
        }
    }

    void SwitchToFollowCam()
    {
        if (introCamObject != null && followCamObject != null)
        {
            introCamObject.SetActive(false);
            followCamObject.SetActive(true);
        }
    }
}
