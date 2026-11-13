# Elf Face Variants V1

Karakter oluşturma ekranı için hazırlanmış 24 farklı Elf portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen kimlik
- Uzun sivri kulaklar ve zarif, köşeli Elf yüz oranları
- Ten tonu, yüz yapısı, yetişkin yaş, göz rengi, saç rengi ve saç dokusu çeşitliliği
- Her portre ayrı 256x256 RGBA PNG
- `atlases/elf-face-variants-transparent-atlas-v1.png`: 6x4 şeffaf atlas
- `manifests/elf-face-variants-v1.json`: kimlik, beden sunumu ve dosya yolları
- `manifests/qa-report-v1.json`: boyut, biçim ve alfa kontrolü
- `previews/elf-face-variants-v1-preview.png`: bütün portrelerin kontrol görünümü

## Yeniden derleme

```powershell
python ..\tools\build_face_variant_pack.py --root . --race elf --title "Elf Face Variants V1" --source "elf-face-variants-source-v1.png"
```
