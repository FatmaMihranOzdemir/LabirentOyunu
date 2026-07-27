# Unity kurulumu ve ilk kontrol

## Her ekip üyesinde

1. Unity Hub kurulu olmalı.
2. `6000.5.5f1` editör sürümü ve hedef platform modülü kurulu olmalı.
3. Git ve Git LFS kurulu olmalı.
4. Unity projesi, depo kökündeki klasörden açılmalı.

Farklı Unity sürümleri sahne ve ProjectSettings dosyalarında gereksiz değişiklik üretebilir. Sürüm yükseltmesi yalnızca ekip kararıyla, ayrı dalda yapılmalıdır.

## Proje açıldıktan sonra kontrol

Unity menüsünde:

1. **Edit → Project Settings → Editor**
   - Version Control Mode: `Visible Meta Files`
   - Asset Serialization Mode: `Force Text`
2. **Edit → Project Settings → Player**
   - Active Input Handling: `Input System Package (New)` veya ekip geçiş sürecindeyse `Both`
3. **Window → Package Manager**
   - Universal RP yüklü mü?
   - Input System yüklü mü?
4. **Project Settings → Graphics**
   - URP asset atanmış mı?
5. Console penceresinde kırmızı hata var mı?

Bu iskelette URP ve Input System yerel Unity 3D URP şablonundan gelir. Cinemachine özellikle eklenmemiştir; ekip üçüncü şahıs kamera yaklaşımını araştırdıktan sonra ekleyip eklememeye karar vermelidir.

## İlk ekip işi

`Assets/_Project/Input/PlayerControls.inputactions` dosyasını Unity içinden oluşturun. İlk aşamada yalnızca şu eylemleri tartışın:

- `Move` — Vector2
- `Look` — Vector2
- `Sprint` — Button (MVP için isteğe bağlı)
- `Pause` — Button

Hazır bir binding listesi kopyalamak yerine Input Action, Action Map, binding ve control scheme kavramlarını araştırın. Klavye/fare çalıştıktan sonra gamepad değerlendirin.

## İlk commit öncesi

- Unity bir kez açılmış ve kapanmış olmalı.
- Oluşan klasör `.meta` dosyaları eklenmeli.
- Console hatasız olmalı.
- `git status` içinde `Library/`, `Temp/` veya `Logs/` görünmemeli.
- `ProjectVersion.txt` değişmemiş olmalı.
