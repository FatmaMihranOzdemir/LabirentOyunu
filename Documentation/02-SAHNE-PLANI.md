# Sahne planı

Sahneler hazır verilmemiştir. Ekip, Unity editörünü öğrenmek için bu planı uygulayarak sahneleri kendisi oluşturacaktır.

## Ana sahne kökleri

`Assets/_Project/Scenes/Main/Main.unity` içinde:

```text
Environment
GeneratedMaze
Gameplay
Managers
SpawnPoints
Cameras
Lighting
UI
```

Boş köklerin Transform değerlerini sıfırlayın:

- Position: `(0, 0, 0)`
- Rotation: `(0, 0, 0)`
- Scale: `(1, 1, 1)`

## Geçici test koridoru

İlk gün gerçek labirenti beklemeyin. Primitive objelerle:

- Yaklaşık `20 x 20` bir zemin
- Dört dış duvar
- Aralarında yaklaşık `5` birim boşluk olan iki iç koridor duvarı
- Koridor ortasında geçici bir kayan kapı
- Başlangıç ve çıkış işaretleri

oluşturun. Amaç sanat üretmek değil; oyuncu, kamera ve kapıyı birbirinden bağımsız test etmektir.

## Üçüncü şahıs oyuncu iskeleti

```text
Gameplay
└── Player
    ├── Visual
    └── GroundCheck

Cameras
└── CameraRig
    └── Main Camera
```

İlk placeholder için Capsule kullanılabilir. `CharacterController` ekleyin. Kamera doğrudan `Player` altına bağlanmamalıdır; ayrı rig, takip ve duvar engeli davranışlarını öğrenmek için daha açık bir sınır oluşturur.

Araştırılacak konular:

- Kameraya göre hareket yönü
- Oyuncunun hareket yönüne dönmesi
- Kamera orbit/yaw/pitch sınırları
- Kamera ile duvar arasında raycast/sphere cast
- Animator parametreleri ve root motion kararı

## Kapı iskeleti

```text
Gameplay
└── DoorRoot
    ├── DoorVisual
    ├── KillZone
    └── ProximityTrigger
```

`DoorRoot` hareket için referans noktasıdır. `DoorVisual` görünen mesh'i; trigger objeleri algılama sınırlarını temsil eder. İlk kapıda yalnızca ritmik davranışı tamamlamak, tetiklemeli türü sonra eklemek daha kolaydır.

## Çıkış ve yöneticiler

```text
Gameplay
└── ExitTrigger

Managers
└── GameManager

SpawnPoints
├── PlayerSpawnPoint
└── ExitSpawnPoint
```

`ExitTrigger` başlangıçta yalnızca işaretleyici olabilir. Kazanma kodu GameState sistemi çalıştıktan sonra bağlanır.

## Test sahneleri

- `Scenes/Test/Test_Maze.unity` — labirent verisi ve üretimi
- `Scenes/Test/Test_Player.unity` — oyuncu, kamera, kapı, ölüm/kazanma
- `Scenes/Test/Test_Visual.unity` — URP, ışık, ses, UI ve mini-harita

Ana sahne günlük deney alanı değildir. Tamamlanmış prefab ve sistemler, ana sahne sahibi tarafından `Main.unity` içine alınır.
