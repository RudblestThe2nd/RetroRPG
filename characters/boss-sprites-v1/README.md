# Mini-Boss ve Ana Boss Sprite Paketi V1

Oyunun 10 biyomu için bir mini-boss ve bir ana boss içerir.

## Teknik standart

- 10 mini-boss + 10 ana boss: toplam 20 boss.
- Her boss için `idle` ve `attack`: toplam 40 sprite.
- Sprite boyutu: 256x256 RGBA PNG.
- Ortak pivot: `[128, 240]`.
- Mini-boss çarpışma profili: `large`.
- Ana boss çarpışma profili: `huge`.
- İki şeffaf üretim atlası, iki QA önizlemesi ve hücre manifestleri bulunur.
- QA: 40/40 başarılı.

Boss kimlikleri biyomlara göre `manifests/boss-sprites-v1.json` içinde listelenmiştir.
