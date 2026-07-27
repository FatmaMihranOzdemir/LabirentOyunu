# Labirent Oyunu — Unity başlangıç iskeleti

Bu depo, üç kişilik ekibin üçüncü şahıs (third-person) bir labirent oyunu geliştirirken aynı klasör ve Git düzeniyle başlaması için hazırlanmış bir **taslaktır**.

Oynanış sistemleri özellikle tamamlanmamıştır. C# dosyaları yalnızca sorumluluk sınırlarını ve araştırılacak konuları gösteren, davranış içermeyen başlangıç sınıflarıdır. Sahne, prefab, Input Actions, animasyon ve oyun mantığını ekip araştırarak oluşturacaktır.

## Teknik temel

- Unity: `6000.5.5f1` (ekipte herkes aynı sürümü kullanmalı)
- Şablon: 3D URP
- Dil: C#
- Perspektif: Üçüncü şahıs
- Girdi: Unity Input System
- Sürüm kontrolü: Git + Git LFS

> Kaynak PDF birinci şahıs kamera tarif ediyordu. Bu taslak, son isteğe göre üçüncü şahıs kamera ve oyuncu yapısına çevrilmiştir.

## İlk açılış

1. Git LFS'in kurulu olduğunu doğrulayın: `git lfs version`
2. Depoyu klonladıktan sonra `git lfs install` çalıştırın.
3. Unity Hub'da **Add project from disk** ile bu klasörü seçin.
4. Unity paketleri yüklenirken bekleyin; `Library/` klasörü yerelde üretilecektir ve Git'e eklenmeyecektir.
5. Unity açılınca Console'da hata olmadığını doğrulayın.
6. [Başlangıç rehberindeki](Documentation/01-UNITY-KURULUMU.md) ayarları kontrol edin.
7. Sahneleri [sahne planına](Documentation/02-SAHNE-PLANI.md) göre ekipçe Unity içinde oluşturun.

## Taslakta bulunanlar

- URP proje ayarları
- Input System paketi
- Ekip klasör yapısı
- Davranış içermeyen C# sınıf taslakları
- Ana/test sahnesi planı
- Üç kişilik dosya sahipliği
- Dört haftalık öğrenme ve geliştirme yol haritası
- Unity uyumlu `.gitignore`, Git LFS ve Smart Merge kuralları

## Özellikle bulunmayanlar

- Hazır karakter hareketi veya kamera kodu
- Hazır labirent algoritması
- Hazır kapı, ölüm, kazanma veya minimap sistemi
- Hazır sahne/prefab
- İndirilmiş model, ses veya texture

Başlangıç noktası: [Documentation/00-PROJE-KAPSAMI.md](Documentation/00-PROJE-KAPSAMI.md)
