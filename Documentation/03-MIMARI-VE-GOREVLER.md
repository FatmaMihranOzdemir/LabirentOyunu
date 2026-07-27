# Mimari ve görev paylaşımı

## Kişi 1 — Labirent ve algoritma

Sahip olduğu alanlar:

- `Scripts/Maze/`
- `Prefabs/Maze/`
- `Scenes/Test/Test_Maze.unity`

Taslak sınıflar:

- `MazeGenerator.cs` — grid verisini üretir
- `MazeBuilder.cs` — grid verisini sahnedeki objelere dönüştürür
- `SolvabilityValidator.cs` — girişten çıkışa yol olup olmadığını doğrular

Önce saf veri, sonra görsel üretim yapılmalıdır. Algoritma ile GameObject üretimi aynı sınıfa yığılmamalıdır.

## Kişi 2 — Oyuncu ve mekanikler

Sahip olduğu alanlar:

- `Scripts/Player/`
- `Scripts/Cameras/`
- `Scripts/Doors/`
- `Scripts/Managers/`
- `Prefabs/Player/`
- `Prefabs/Doors/`
- `Scenes/Test/Test_Player.unity`

Taslak sınıflar:

- `ThirdPersonController.cs`
- `ThirdPersonCameraController.cs`
- `SlidingDoor.cs`
- `GameStateManager.cs`

Hareket, kamera ve oyun durumu ayrı sorumluluklardır. Birbirlerini doğrudan bulmak için sürekli `Find` çağırmak yerine Inspector referansı veya daha sonra tasarlanacak olaylar kullanılmalıdır.

## Kişi 3 — Görsel, atmosfer ve arayüz

Sahip olduğu alanlar:

- `Art/`
- `Audio/`
- `Scripts/UI/`
- `UI/`
- `Prefabs/UI/`
- `Scenes/Test/Test_Visual.unity`

Taslak sınıf:

- `MinimapController.cs`

Mini-harita, gerçek labirent hazır olmadan küçük bir mock grid ile geliştirilebilir. Sanat dosyalarının kaynak/lisans bilgisi `ThirdParty/` veya proje dokümanında tutulmalıdır.

## Ortak alanlar

- `Scripts/Common/`
- `Input/`
- `ProjectSettings/`
- `Packages/`
- `Scenes/Main/Main.unity`

Ortak dosyalarda değişiklik yapmadan önce ekip haberleşmelidir. Ana sahnenin tek sahibi belirlenmelidir.

## Sistemler arası önerilen veri akışı

```text
MazeGenerator (veri)
       ↓
MazeBuilder (görsel dünya)
       ↓
Player / Doors / Exit
       ↓
GameStateManager

MazeGenerator verisi ──→ MinimapController
Kapı yerleşimleri ─────→ SolvabilityValidator
```

Bu şema sözleşme sınırıdır; hazır implementasyon değildir. Veri tipleri ilk mock grid denemesinden sonra ekipçe kararlaştırılmalıdır.
