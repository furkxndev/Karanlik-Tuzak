# Karanlık Tuzak

"Level Devil" tarzı, karanlık atmosferli bir 2D mobil troll platform oyunu.
Tüm dünya, karakter, kontroller ve seviyeler **kod ile runtime'da** otomatik oluşur —
sahne kurulumu, sprite import'u veya prefab gerektirmez.

## Özellikler

- 🎮 5 seviye, her biri kendine özgü troll mekaniğiyle (çöken zemin, sahte platform,
  fışkıran diken, ters yerçekimi, ezici pistonlar, ters kontroller)
- 📱 Dokunmatik kontroller (Android / iOS) + editörde klavye testi
- 🌑 Bilinçli karanlık ve kasvetli palet, yazısız sade arayüz
- ⚙️ Unity **2022.3** (URP 2D)

## Çalıştırma

Unity'de projeyi aç, `Assets/Scenes/SampleScene.unity` sahnesini aç ve **Play**'e bas.
`GameBootstrap` sayesinde oyun otomatik başlar.

## Kontroller

| Aksiyon | Dokunmatik | Klavye |
|---|---|---|
| Hareket | ◄ ► butonları | ← → / A D |
| Zıpla | ▲ butonu | Space / ↑ / W |
| Yeniden başlat | — | R |

> Daha ayrıntılı seviye/mekanik anlatımı için bkz. [`README_OYUN.md`](README_OYUN.md).
