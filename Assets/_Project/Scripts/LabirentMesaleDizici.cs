using UnityEngine;

public class LabirentMesaleDizici : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject kaynakMesale;
    public GameObject lambaGrubu;
    public float yerdenYukseklik = 2.5f;
    public float duvardanOfset = 0.2f;

    [Header("Işık Ayarları")]
    [Tooltip("Duvarın en az bu uzunlukta olması gerekir, yoksa meşale koyulmaz (çok kısa parçaları/köşeleri atlamak için)")]
    public float minDuvarUzunlugu = 2.5f;
    public float isikIntensity = 2.5f;
    [Tooltip("Range'i duvar uzunluğunun bir oranı olarak otomatik ayarla (0.6 = duvarın %60'ı kadar menzil, uçlara doğru hafif karanlık kalır)")]
    public float rangeOraniDuvarUzunluguna = 0.65f;

    [Header("Genel Ortam Işığı (Ambient)")]
    [Tooltip("Duvar dokusunun karanlık bölgelerde de hafifçe seçilmesini sağlar. Çok düşük = simsiyah köşeler, çok yüksek = meşale etkisi kaybolur.")]
    public Color ambientRenk = new Color(0.10f, 0.10f, 0.13f);

    [ContextMenu("1- ÖNCE HER ŞEYİ SİL VE SIFIRDAN DİZ")]
    public void TemizleVeSilBastanDiz()
    {
        // 1. KÖKTEN TEMİZLİK: Grubun içindeki tüm eski meşale ve ışıkları kazı
        KapsamliIçTemizlik();

        if (kaynakMesale == null || lambaGrubu == null)
        {
            Debug.LogError("Kanka Prefab veya Lamba Grubu eksik!");
            return;
        }

        // Karanlık yerlerin dokusu gözüksün diye loş taban aydınlatması
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = ambientRenk;

        GameObject anaMesaleKonteyner = GameObject.Find("Duzgun_Mesaleler_Grubu");
        if (anaMesaleKonteyner == null)
        {
            anaMesaleKonteyner = new GameObject("Duzgun_Mesaleler_Grubu");
        }

        int sayac = 0;

        foreach (Transform parca in lambaGrubu.transform)
        {
            // Eğer parça bir şekilde meşale veya ışık ise es geç
            string isim = parca.name.ToLower();
            if (isim.Contains("lamp") || isim.Contains("light") || isim.Contains("floor") ||
                isim.Contains("ground") || isim.Contains("zemin") || isim.Contains("tavan") || isim.Contains("ceiling"))
                continue;

            MeshFilter mf = parca.GetComponent<MeshFilter>();
            if (mf == null) continue;

            Renderer duvarRenderer = parca.GetComponent<Renderer>();
            Vector3 merkez = parca.position;
            Vector3 boyut = parca.localScale;

            if (duvarRenderer != null)
            {
                merkez = duvarRenderer.bounds.center;
                boyut = duvarRenderer.bounds.size;
            }

            // Geniş zeminleri kesin olarak ele
            if (boyut.x > 8f && boyut.z > 8f && boyut.y < 2f) continue;
            if (boyut.x < 2f && boyut.z < 2f) continue; // Çok küçük sütunları ele

            Vector3 ileri = parca.forward;

            float duvarUzunlugu = (boyut.x > boyut.z) ? boyut.x : boyut.z;

            // Çok kısa parçalara (köşe parçaları vs.) meşale koyma
            if (duvarUzunlugu < minDuvarUzunlugu) continue;

            // Duvarın GERÇEK tabanını bul (grup offsetinden bağımsız, güvenilir yöntem)
            float duvarTabanY = (duvarRenderer != null) ? duvarRenderer.bounds.min.y : (parca.position.y - boyut.y / 2f);
            float finalY = duvarTabanY + yerdenYukseklik;

            // Duvarın iki tarafını da hesapla (forward yönü her duvarda tutarlı olmayabilir)
            Vector3 merkezFinalY = merkez;
            merkezFinalY.y = finalY;
            Vector3 pozIleri = merkezFinalY + (ileri * ((boyut.z / 2f) + duvardanOfset));
            Vector3 pozGeri = merkezFinalY - (ileri * ((boyut.z / 2f) + duvardanOfset));

            // Hangi tarafta collider YOKSA (yani koridor/boş taraf) oraya koy.
            // Bu sayede meşale asla duvarın veya bitişik geometrinin içine gömülmez.
            float kontrolYaricapi = 0.15f;
            bool ileriDolu = Physics.CheckSphere(pozIleri, kontrolYaricapi);
            bool geriDolu = Physics.CheckSphere(pozGeri, kontrolYaricapi);

            Vector3 dunyaPozisyonu;
            Quaternion mesaleRotasyonu;

            if (!ileriDolu)
            {
                dunyaPozisyonu = pozIleri;
                mesaleRotasyonu = parca.rotation;
            }
            else if (!geriDolu)
            {
                dunyaPozisyonu = pozGeri;
                mesaleRotasyonu = parca.rotation * Quaternion.Euler(0f, 180f, 0f);
            }
            else
            {
                // İkisi de doluysa (nadir durum) varsayılan tarafa koy, en azından atlanmasın
                dunyaPozisyonu = pozIleri;
                mesaleRotasyonu = parca.rotation;
            }

            GameObject yeniMesale = Instantiate(kaynakMesale, dunyaPozisyonu, mesaleRotasyonu, anaMesaleKonteyner.transform);
            yeniMesale.name = "Oluşan_Meşale";
            sayac++;

            Light icindekiIsik = yeniMesale.GetComponentInChildren<Light>();
            if (icindekiIsik != null)
            {
                icindekiIsik.transform.SetParent(yeniMesale.transform);
                icindekiIsik.transform.localPosition = Vector3.zero;
                icindekiIsik.transform.localRotation = Quaternion.identity;

                icindekiIsik.type = LightType.Point;
                icindekiIsik.shadows = LightShadows.Soft;
                icindekiIsik.intensity = isikIntensity;
                // Range'i duvar uzunluğuna göre ayarla: uçlara doğru doğal karanlık geçişi için
                icindekiIsik.range = duvarUzunlugu * rangeOraniDuvarUzunluguna;
            }
        }

        Debug.LogWarning($"Sahne temizlendi ve her duvara TEK meşale dizildi. Toplam meşale sayısı: {sayac}");
    }

    private void KapsamliIçTemizlik()
    {
        if (lambaGrubu == null) return;

        // Lamba grubunun İÇİNDEKİ tüm objeleri kontrol et ve eski kalıntıları sil
        Transform[] tumCocuklar = lambaGrubu.GetComponentsInChildren<Transform>(true);

        // Tersten döngü (silme işlemlerinde hata olmasın diye)
        for (int i = tumCocuklar.Length - 1; i >= 0; i--)
        {
            if (tumCocuklar[i] == null || tumCocuklar[i] == lambaGrubu.transform) continue;

            string altIsim = tumCocuklar[i].name.ToLower();
            if (altIsim.Contains("lamp") || altIsim.Contains("light") || altIsim.Contains("oluşan") || altIsim.Contains("mesale"))
            {
                DestroyImmediate(tumCocuklar[i].gameObject);
            }
        }

        // Dışarıda kalan bağımsız gruplar varsa onları da uçur
        GameObject eskiGrup = GameObject.Find("Olusan_Mesaleler_Grubu");
        if (eskiGrup != null) DestroyImmediate(eskiGrup);

        GameObject yeniGrup = GameObject.Find("Duzgun_Mesaleler_Grubu");
        if (yeniGrup != null) DestroyImmediate(yeniGrup);
    }
}
