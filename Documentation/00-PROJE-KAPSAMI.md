# Proje kapsamı

## Oyun fikri

Oyuncu, her denemede yeniden üretilen karanlık bir 3D labirentte üçüncü şahıs kamerayla ilerler. Sınırlı bilgi veren mini-harita yalnızca ziyaret edilen alanları gösterir. Ritmik veya yaklaşınca tetiklenen kayan kapılar oyuncuyu tehdit eder. Kapıya yakalanmak ölümü ve yeni bir labirentle yeniden başlamayı; çıkışı bulmak kazanmayı getirir.

## MVP

İlk oynanabilir sürüm şu koşulları sağlamalıdır:

- Oyuncu üçüncü şahıs olarak yürüyebilmeli ve dönebilmeli.
- Kamera oyuncuyu takip etmeli, duvarların içine girmemeli.
- Her oyun başlangıcında çözülebilir bir labirent oluşmalı.
- En az bir kayan kapı tipi çalışmalı.
- Kapıya yakalanınca oyun yeniden başlamalı.
- Çıkışa ulaşınca kazanma durumu görülmeli.
- Mini-harita yalnızca ziyaret edilen yakın hücreleri göstermeli.
- Menü → oyun → ölüm/kazanma → tekrar oyna döngüsü tamamlanmalı.

## MVP dışında

İlk dört haftada zorunlu değildir:

- Düşman yapay zekâsı
- Envanter ve eşya sistemi
- Çok oyunculu mod
- Kayıt sistemi
- Birden fazla bölüm teması
- Gelişmiş karakter özelleştirme
- Çevrimiçi skor tablosu

Bu sınır, ekibin önce tek bir çalışır oyun döngüsü üretmesini sağlar.

## Kaynak dokümana göre değişiklik

PDF'deki tasarım birinci şahıs kamera öneriyordu. Projenin güncel kararı üçüncü şahıstır. Bu yüzden:

- `PlayerController` yerine `ThirdPersonController` taslağı kullanılır.
- Kamera oyuncunun çocuğu olmak yerine ayrı bir `CameraRig` altında planlanır.
- Kamera engel kontrolü ve karakter animasyonu ayrı araştırma başlıklarıdır.

Kararlar değiştiğinde [KARAR-KAYDI.md](KARAR-KAYDI.md) güncellenmelidir.
