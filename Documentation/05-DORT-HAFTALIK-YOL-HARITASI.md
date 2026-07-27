# Dört haftalık yol haritası

Bu plan hedef sırasıdır; her kutu araştırma, küçük uygulama ve test gerektirir.

## Hafta 1 — Ortak temel ve mock dünya

Ortak:

- [ ] Herkes aynı Unity sürümüyle projeyi açtı
- [ ] Git LFS ve dal akışı denendi
- [ ] Main ve üç test sahnesi oluşturuldu
- [ ] Basit koridor ve placeholder prefablar hazırlandı
- [ ] Dosya sahipleri kesinleştirildi

Kişi 1:

- [ ] Grid/hücre verisinin tutacağı bilgiler tasarlandı
- [ ] Küçük, elle tanımlı mock grid Console'da gösterildi

Kişi 2:

- [ ] Input Actions oluşturuldu
- [ ] CharacterController ile yürüme ve dönüş yapıldı
- [ ] Basit üçüncü şahıs takip kamerası denendi

Kişi 3:

- [ ] URP ve ışık ayarları incelendi
- [ ] Boş minimap paneli ve temel UI yerleşimi yapıldı

## Hafta 2 — Çekirdek sistemler

Kişi 1:

- [ ] Recursive backtracker araştırıldı ve saf veri üzerinde uygulandı
- [ ] MazeBuilder mock veriden gerçek veriye bağlandı

Kişi 2:

- [ ] Kameraya göre hareket ve karakter dönüşü tamamlandı
- [ ] Ritmik SlidingDoor state machine'i tamamlandı
- [ ] Kapı kill zone testi yapıldı

Kişi 3:

- [ ] Ziyaret edilen hücre verisi mock grid üzerinde tutuldu
- [ ] Sınırlı yarıçaplı minimap prototipi yapıldı
- [ ] Geçici kapı/ayak sesi denendi

## Hafta 3 — Entegrasyon

- [ ] Labirent verisi builder, minimap ve doğrulayıcıya bağlandı
- [ ] BFS ile girişten çıkışa yol doğrulandı
- [ ] Kapı yerleşiminin yolu imkânsızlaştırmadığı test edildi
- [ ] Ölüm, yeniden üretim ve kazanma durumları bağlandı
- [ ] Kamera duvar engeli çözüldü
- [ ] Baştan sona bir tur tamamlandı

## Hafta 4 — Dengeleme ve MVP

- [ ] Labirent boyutu ve kapı sayısı ayarlanabilir
- [ ] Hareket ve kamera hissi iyileştirildi
- [ ] Ölüm/kazanma UI'ı tamamlandı
- [ ] Ses ve ışık dengelendi
- [ ] Büyük labirent performansı ölçüldü
- [ ] Temiz klonda oyun açılıp oynandı
- [ ] Bilinen hatalar yazıldı
- [ ] MVP build alındı

## Her hafta sonu demo soruları

- Çalışan ve ekranda gösterilebilen ne var?
- Hangi kabul kriteri geçti?
- Bir sonraki sistemi engelleyen veri/sözleşme eksikliği var mı?
- Hangi ortak dosyalar değişecek?
- Kapsam dışına taşan iş var mı?
