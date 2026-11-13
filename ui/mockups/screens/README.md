# Core Game Screen Mockups

Bu paket oyunun beş temel ekranını içerir:

- `combat-screen-v1.png`
- `inventory-screen-v1.png`
- `character-screen-v1.png`
- `journal-screen-v1.png`
- `camp-screen-v1.png`

## Klasörler

- `source-highres/`: düzenleme ve referans için yüksek çözünürlüklü kaynaklar.
- `runtime-800/`: oyunun 800x800 penceresinde kullanılacak hazır çıktılar.
- `manifests/`: ekran kimlikleri, yolları ve çözünürlük bilgileri.

Dosya adları küçük harfli `kebab-case` kullanır ve `-v1`, `-v2` biçiminde sürümlenir.

Runtime çıktıları yeniden oluşturmak için:

```powershell
python .\build_runtime_exports.py
```
