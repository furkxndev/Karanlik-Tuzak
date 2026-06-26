# Karanlık Tuzak — "Level Devil" Tarzı 2D Mobil Platform Oyunu

Unity 2022.3 (URP 2D) için, tamamen **kod ile runtime'da inşa edilen** troll platform oyunu.
Hiçbir sahne kurulumu, sprite import'u veya prefab ayarı gerektirmez — bütün dünya,
karakter, dokunmatik kontroller ve 5 seviye otomatik oluşur.

## Nasıl Çalıştırılır

İki seçenekten biri yeterli:

1. **En kolay yol:** Unity'de projeyi aç, herhangi bir sahneyi (örn. `Assets/Scenes/SampleScene.unity`)
   aç ve **Play**'e bas. `GameBootstrap` içindeki `[RuntimeInitializeOnLoadMethod]` sayesinde
   oyun boş sahnede bile otomatik başlar.
2. **Manuel:** Boş bir sahnede yeni bir boş `GameObject` oluştur, üzerine
   `Assets/Scripts/Core/GameBootstrap.cs` scriptini ekle, Play'e bas.

> Not: Scriptleri Unity dışında oluşturduğum için, Unity'yi ilk açtığında editör onları
> import edip `.meta` dosyalarını otomatik üretecektir. Derleme hatası olmamalı.

### Mobil Build (Android/iOS)
- `File > Build Settings > Android (veya iOS) > Switch Platform`.
- Sahneyi Build listesine ekle. Dokunmatik UI butonları otomatik kurulur ve çalışır.
- `Application.targetFrameRate = 60` ayarı kodda yapılır.

## Kontroller

| Aksiyon | Dokunmatik | Klavye (editör testi) |
|---|---|---|
| Sol / Sağ | Ekranın sol altındaki ◄ ► butonları (basılı tut) | ← → veya A / D |
| Zıpla | Ekranın sağ altındaki ▲ butonu | Space / ↑ / W |
| Yeniden başlat | — | R |

Zıplama; basılı tutunca daha yükseğe, erken bırakınca daha alçak (değişken zıplama).
Coyote-time ve jump-buffer ile akıcı his.

## Atmosfer

Renkli/neşeli değil — bilinçli olarak **karanlık ve kasvetli** bir palet
(`SpriteFactory.cs` içinde tanımlı): neredeyse siyah arka plan, koyu gri zeminler,
soluk gri karakter, kan kırmızısı tuzaklar, sönük yeşilimsi çıkış parıltısı.

## Seviyeler ve Troll Mekanikleri (5 bölüm)

- **Level 1 — Sahte Güven:** Çıkışa yaklaşınca zemin **çöker** (`CollapsingPlatform`)
  ve yukarıdan **ölümcül blok düşer** (`FallingObstacle`).
- **Level 2 — Güvenli Görünen:** Bazı zeminler **sahte** (`FakePlatform`), bazı düz
  alanlardan basınca **dikenler fışkırır** (`AppearingSpikes`). Sahte zeminler
  gerçeğiyle birebir aynı görünür.
- **Level 3 — Ters Köşe:** Zıpla tuşu **yerçekimini ters çevirir**
  (`gravityFlipMode`); tavanda yürürsün. Çıkış kapısı senden **kaçar** (`FleeingDoor`).
- **Level 4 — Ezici Koridor:** Tavandan ritmik inip kalkan **ezici pistonlar**
  (`Crusher`) arasından zamanlamayla geçersin. Sondaki **yalancı çıkış kapısı**
  gerçeğiyle aynı görünür ama dokununca öldürür — gerçek çıkış biraz ileride.
- **Level 5 — Ters Kontroller (Final):** Sol↔Sağ **yer değiştirir**
  (`invertControls`). Önceki tüm tuzakların (sahte zemin, fışkıran diken, çöken köprü,
  ezici) karışımı bir final gauntlet'i.

## Görünüm Notları

- **Yazısız arayüz:** Profesyonel, sade görünüm için ekranda hiçbir metin yoktur.
  Dokunmatik butonlar ok ikonlarıyla çizilir; geçiş/ölüm geri bildirimi ekran flaşıyla
  verilir (kırmızı = ölüm, yeşil = bölüm geçildi).
- **Aydınlık alacakaranlık paleti:** Atmosfer karanlık-kasvetli tonda ama artık çok
  daha aydınlık; gradient gökyüzü, parallax siluetler ve sis ile derinlikli.
- **Renk uyumu:** Tehlikeler (diken, ezici, düşen blok) mor-mavi arka planın
  **tamamlayıcısı sıcak mercan/turuncu** tonunda — net okunur ve uyumlu. Çıkış kapısı
  arka planla **analog camgöbeği bir portal** (parıltı halesi + yuvarlak çerçeve + iç çekirdek).
- **Detaylı engeller:** Dikenler artık katmanlı (koyu anahat + gövde + parlak uç
  vurgusu + taban plakası + nabız atan parıltı). Ezici metalik gövde, perçinler, uyarı
  şeridi ve diş sırasıyla; düşen blok parıltı halesi ve çapraz tehlike şeritleriyle gelir.
- **Görsel efektler (juice):** Çarpma anlarında **toz/kıvılcım partikülleri**
  (`Fx`), **kamera sarsıntısı** (`CameraFollow.Shake`) ve **uyarı nabzı** (`Pulse`) —
  iniş, ezici çarpması, düşen blok ve ölüm anlarında tetiklenir.

## Ses (Tamamen Prosedürel)

`AudioManager.cs` tüm sesleri runtime'da sentezler (hiç ses dosyası import edilmez):
- **Karakter:** zıplama, iniş, ayak sesleri, yerçekimi tersleme "whoosh"u
- **Engeller:** diken fışkırması, zemin çökmesi, ezici çarpması, sahte zemin/düşen blok
- **Olaylar:** ölüm, bölüm geçme arpeji, UI buton tıklaması
- **Atmosfer:** sürekli, kusursuz döngülenen düşük fon uğultusu (drone)

## Mimari (Assets/Scripts)

```
Core/
  GameBootstrap.cs   - Tek giriş noktası; her şeyi kurar (auto-boot)
  GameManager.cs     - Kamera, karakter, UI, seviye akışı, ölüm/respawn
  SpriteFactory.cs   - Tüm sprite'ları ve paleti runtime üretir
  AudioManager.cs    - Tüm sesleri ve fon uğultusunu prosedürel sentezler
  InputState.cs      - Dokunmatik + klavye girdi durumu
Player/
  PlayerController.cs- Rigidbody2D+BoxCollider2D fizik; coyote/buffer/yerçekimi ters
UI/
  TouchUIBuilder.cs  - Dokunmatik butonlar, HUD, ölüm flaşı (uGUI)
Levels/
  LevelManager.cs    - 5 seviyeyi ve tüm geometriyi kod ile inşa eder
Traps/
  CollapsingPlatform.cs, FallingObstacle.cs, AppearingSpikes.cs, Crusher.cs,
  FakePlatform.cs, FleeingDoor.cs
World/
  Hazard.cs, ExitDoor.cs, CameraFollow.cs
```

### Tasarım Notları
- **Zemin kontrolü çarpışma normalleriyle** yapılır; hiçbir Physics Layer ayarına
  gerek yoktur (gerçekten kur-çalıştır).
- Her ölümde seviye baştan inşa edildiği için tüm tuzaklar otomatik sıfırlanır.
- Karakter collider köşeleri hafif yuvarlatılır; bitişik blok dikişlerine takılmaz.
