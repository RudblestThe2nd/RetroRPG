# Human Face Variants V1

Karakter oluşturma ekranında kullanılmak üzere hazırlanmış 24 farklı insan portresi paketidir.

## İçerik

- 12 maskülen ve 12 feminen kimlik
- Ten tonu, yüz yapısı, yaş, saç rengi, saç dokusu ve sakal çeşitliliği
- Her portre ayrı 256x256 RGBA PNG
- `atlases/human-face-variants-transparent-atlas-v1.png`: 6x4 atlas
- `manifests/human-face-variants-v1.json`: kimlik, beden sunumu ve dosya yolları
- `manifests/qa-report-v1.json`: boyut ve biçim kontrolü
- `previews/human-face-variants-v1-preview.png`: bütün portrelerin kontrol görünümü

Portreler karakter oluşturma UI'sındaki portre çerçeveleri içinde kullanılmak üzere nötr ifadeli ve benzer kadrajlıdır.

## Yeniden derleme

```powershell
python .\source\build_human_face_variants.py
```
