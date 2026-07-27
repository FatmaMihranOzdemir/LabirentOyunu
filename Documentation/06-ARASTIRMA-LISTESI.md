# Araştırma listesi ve kabul kriterleri

## Third-person hareket

Araştır:

- `CharacterController.Move`
- gravity ve grounded kontrolü
- kameraya göre yön vektörü
- `Time.deltaTime`
- input callback ile polling farkı

Kabul:

- Oyuncu zeminden düşmeden yürür.
- Duvarların içinden geçmez.
- Kamera yönüne göre beklenen yöne gider.
- Kodda kare hızına bağlı hareket yoktur.

## Third-person kamera

Araştır:

- orbit kamera
- yaw/pitch ve açı sınırı
- smooth follow
- raycast/sphere cast ile kamera çarpışması
- Cinemachine kullanmanın artı/eksileri

Kabul:

- Kamera oyuncuyu takip eder.
- Duvar arkasında kaybolmaz veya duvar içine girmez.
- Fare hareketi kare hızından bağımsız ve ayarlanabilirdir.

## Labirent üretimi

Araştır:

- grid veri modeli
- recursive backtracker
- seed ve deterministik random
- veri ile görselin ayrılması

Kabul:

- Aynı seed aynı labirenti üretir.
- Farklı seed çoğunlukla farklı sonuç verir.
- Girişten çıkışa yol vardır.
- Üretici Unity GameObject'lerine bağımlı olmadan test edilebilir.

## Labirent inşası

Araştır:

- prefab instantiate
- parent transform
- object pooling gerekip gerekmediği
- mesh/collider maliyeti

Kabul:

- Mock grid doğru duvar düzenine dönüşür.
- Yeniden üretimde eski objeler kalmaz.
- Duvar collider'ları kesintisizdir.

## Kayan kapı

Araştır:

- enum tabanlı state machine
- coroutine ve Update tabanlı zamanlama farkı
- local/world position
- trigger/collision ayrımı

Kabul:

- Açık, kapanıyor, kapalı ve açılıyor durumları gözlenebilir.
- Kapı zamanlaması Inspector'dan ayarlanabilir.
- Oyuncu tehlike bölgesindeyken ölüm yalnızca bir kez tetiklenir.

## Game state

Araştır:

- state machine
- scene reload ile aynı sahnede reset farkı
- event/action
- pause sırasında `timeScale`

Kabul:

- Menü, oynanış, ölüm ve kazanma geçişleri kontrollüdür.
- Ölüm yeni seed ile yeni labirent başlatır.
- Yeniden başlatmada eski event abonelikleri kalmaz.

## Mini-harita

Araştır:

- ziyaret edilen hücrelerin veri temsili
- UI grid ve RenderTexture yaklaşımları
- oyuncuya göre dönen/sabit harita farkı
- sınırlı görüş yarıçapı

Kabul:

- Ziyaret edilmemiş hücre görünmez.
- Harita çıkış yolunu önceden ele vermez.
- Oyuncu konumu okunaklıdır.
- Labirent boyutu arttığında UI bozulmaz.

## Çözülebilirlik

Araştır:

- BFS queue ve visited set
- kapıların geçilebilirlik maliyeti
- test seed'leri

Kabul:

- Çözülemeyen mock veri reddedilir.
- Çözülebilen veri kabul edilir.
- Doğrulama makul sürede tamamlanır.
