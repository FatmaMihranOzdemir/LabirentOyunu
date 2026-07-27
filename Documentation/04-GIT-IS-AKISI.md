# Git iş akışı

## Depoyu ilk kez hazırlayan kişi

```powershell
git lfs install
git add .
git status
git commit -m "chore: add Unity project skeleton"
```

Uzak depo daha sonra eklenebilir:

```powershell
git remote add origin <REPO-ADRESI>
git branch -M main
git push -u origin main
```

Repo adresi ekip kararı gerektirdiği için taslağa eklenmemiştir.

## Günlük akış

```powershell
git switch main
git pull --ff-only
git switch -c feature/kisa-is-adi
```

İş bittikten sonra:

```powershell
git add <yalnizca-ilgili-dosyalar>
git diff --cached
git commit -m "feat(area): describe change"
git push -u origin feature/kisa-is-adi
```

Ardından pull request açın. Başkasının test sahnesini veya prefabını yanlışlıkla değiştirmediğinizi kontrol edin.

## Unity'ye özel kurallar

- Asset taşımayı Windows Explorer yerine Unity Project panelinden yapın; `.meta` bağlantıları korunur.
- Bir asset silinirse onun `.meta` dosyası da aynı commit'te silinmelidir.
- Aynı `.unity` sahnesini iki kişi aynı anda düzenlememelidir.
- Prefab değişikliklerini küçük tutun.
- Unity açıkken dal değiştirmeyin; editör dosyaları yeniden yazabilir.
- `Packages/manifest.json` ve `ProjectSettings/` değişikliklerini pull request açıklamasında belirtin.

## Unity Smart Merge

`.gitattributes` Unity YAML dosyalarını `unityyamlmerge` sürücüsüne yönlendirir. Her ekip üyesi, kendi Unity kurulum yoluna göre merge aracını bir kez tanımlamalıdır.

Windows örneği:

```powershell
git config merge.unityyamlmerge.name "Unity SmartMerge"
git config merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/6000.5.5f1/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p "$BASE" "$REMOTE" "$LOCAL" "$MERGED"'
git config merge.unityyamlmerge.recursive binary
```

Smart Merge yardım eder ama sahne sahipliği kuralının yerine geçmez.

## Git LFS

`.gitattributes` büyük model, kaynak görsel, ses ve video dosyalarını LFS'e yönlendirir. Kontrol:

```powershell
git lfs track
git lfs ls-files
```

Bir ekip üyesi LFS kurmadan büyük asset commit etmemelidir.

## Çakışma olursa

1. Çalışmayı durdurun ve iki değişikliğin sahiplerini belirleyin.
2. C# metin çakışmalarını birlikte çözün.
3. Sahne/prefab çakışmasında mümkünse bir tarafın değişikliğini ayrı prefab olarak yeniden uygulayın.
4. Çakışma işaretleri bulunan Unity YAML dosyasını editörde açmayın.
5. Çözümden sonra sahneyi Unity'de açıp Play Mode testi yapın.
