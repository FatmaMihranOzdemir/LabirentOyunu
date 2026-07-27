# Katkı düzeni

## Temel kurallar

1. Günlük çalışma doğrudan `main` dalında yapılmaz.
2. Her iş için kısa ömürlü bir dal açılır.
3. Bir commit tek bir anlaşılır değişiklik taşır.
4. Unity kapalıyken dal değiştirmek daha güvenlidir.
5. `.meta` dosyaları bağlı oldukları asset ile birlikte commit edilir.
6. `Library`, `Temp`, `Logs`, `Obj` ve `UserSettings` commit edilmez.
7. Ana sahnenin tek bir sahibi vardır; diğer kişiler kendi test sahnelerinde çalışır.

## Dal adları

- `feature/maze-generation`
- `feature/third-person-movement`
- `feature/sliding-door`
- `feature/minimap`
- `fix/player-wall-collision`
- `docs/update-roadmap`

## Commit örnekleri

- `feat(player): add walk input reading`
- `feat(maze): create grid data model`
- `test(door): add timed door test scene`
- `fix(camera): prevent wall clipping`
- `docs(git): explain scene ownership`

## Bir değişikliği göndermeden önce

- Unity Console temiz mi?
- Değişen `.meta` dosyaları eklendi mi?
- Alakasız sahne veya prefab değişmiş mi?
- Sahne Play Mode'da açılıyor mu?
- Büyük binary dosyalar Git LFS tarafından izleniyor mu?
- Pull request açıklamasında test yöntemi yazıyor mu?

Ayrıntılı akış: [Documentation/04-GIT-IS-AKISI.md](Documentation/04-GIT-IS-AKISI.md)
