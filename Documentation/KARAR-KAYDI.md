# Karar kaydı

Önemli teknik ve tasarım kararlarını kısa gerekçeleriyle burada tutun.

## K-001 — Perspektif

- Tarih: 2026-07-27
- Karar: Oyun üçüncü şahıs olacak.
- Gerekçe: Ekibin güncel isteği.
- Etki: Kamera ayrı rig altında olacak; karakter yönelimi, animasyon ve kamera engeli MVP araştırmalarına eklendi.

## K-002 — Unity sürümü

- Tarih: 2026-07-27
- Karar: Başlangıç iskeleti yerelde kurulu `6000.5.5f1` sürümüyle oluşturuldu.
- Gerekçe: Ekip başlangıcında aynı editör sürümüyle tekrarlanabilir proje açılışı.
- Etki: Tüm ekip aynı sürümü kurmalı. LTS sürümüne geçiş istenirse gameplay başlamadan ekipçe yeni karar alınmalı.

## K-003 — Hazır gameplay kodu yok

- Tarih: 2026-07-27
- Karar: Sistem dosyaları derlenebilir, davranış içermeyen taslaklar olacak.
- Gerekçe: Ekip bütün özellikleri araştırıp öğrenerek kendisi geliştirmek istiyor.
- Etki: İlk oynanabilir hareket, labirent ve kapı ekip tarafından yazılacak.

## K-004 — Ayrı test sahneleri

- Tarih: 2026-07-27
- Karar: Her sorumluluk alanı ayrı test sahnesinde çalışacak.
- Gerekçe: Unity sahne çakışmalarını azaltmak.
- Etki: Main sahnenin tek sahibi olacak.

## Yeni karar şablonu

```text
## K-XXX — Başlık
- Tarih:
- Karar:
- Gerekçe:
- Etki:
```
