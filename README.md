# Karanlık Tuzak

"Level Devil" tarzı, karanlık atmosferli bir **2D mobil troll platform oyunu**.
Unity 2022.3 (URP 2D) ile yapıldı. Tüm dünya, karakter, kontroller ve seviyeler
**kod ile runtime'da** otomatik oluşur — sahne kurulumu, sprite import'u veya prefab
gerektirmez. Sprite'lar, sesler ve geometri dahil her şey çalışma anında üretilir.

## Özellikler

- 🎮 5 seviye, her biri kendine özgü troll mekaniğiyle
- 📱 Dokunmatik kontroller (Android / iOS) + editörde klavye testi
- 🌑 Karanlık-kasvetli ama derinlikli atmosfer; yazısız sade arayüz
- 🔊 Tamamen **prosedürel ses** (hiç ses dosyası yok, her şey runtime sentezlenir)
- ⚙️ Unity **2022.3** (URP 2D)

## Çalıştırma

1. **En kolay yol:** Unity'de projeyi aç, `Assets/Scenes/SampleScene.unity` sahnesini
   aç ve **Play**'e bas. `GameBootstrap` içindeki `[RuntimeInitializeOnLoadMethod]`
   sayesinde oyun boş sahnede bile otomatik başlar.
2. **Manuel:** Boş bir sahnede yeni bir `GameObject` oluştur, üzerine
   `Assets/Scripts/Core/GameBootstrap.cs` scriptini ekle ve Play'e bas.

> Not: Unity'yi ilk açtığında editör scriptleri import edip `.meta` dosyalarını
> otomatik üretir; derleme hatası olmamalı.

### Mobil Build (Android / iOS)
- `File > Build Settings > Android (veya iOS) > Switch Platform`
- Sahneyi Build listesine ekle. Dokunmatik UI butonları otomatik kurulur.
- `Application.targetFrameRate = 60` ayarı kodda yapılır.

## Kontroller

| Aksiyon | Dokunmatik | Klavye (editör testi) |
|---|---|---|
| Sol / Sağ | Sol alttaki ◄ ► butonları (basılı tut) | ← → veya A / D |
| Zıpla | Sağ alttaki ▲ butonu | Space / ↑ / W |
| Yeniden başlat | — | R |

Zıplama değişkendir: basılı tutunca daha yükseğe, erken bırakınca daha alçak.
Coyote-time ve jump-buffer ile akıcı bir his sağlanır.

## Seviyeler ve Troll Mekanikleri

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
  (`invertControls`). Önceki tüm tuzakların karışımı bir final gauntlet'i.

## Görünüm ve Atmosfer

- **Yazısız arayüz:** Ekranda hiçbir metin yoktur. Dokunmatik butonlar ok ikonlarıyla
  çizilir; geri bildirim ekran flaşıyla verilir (kırmızı = ölüm, yeşil = bölüm geçildi).
- **Palet:** Karanlık-kasvetli ama derinlikli; gradient gökyüzü, parallax siluetler ve sis.
- **Renk uyumu:** Tehlikeler sıcak mercan/turuncu tonunda (net okunur), çıkış kapısı ise
  camgöbeği bir portaldır (parıltı halesi + yuvarlak çerçeve + iç çekirdek).
- **Görsel efektler (juice):** Çarpma anlarında toz/kıvılcım partikülleri (`Fx`),
  kamera sarsıntısı (`CameraFollow.Shake`) ve uyarı nabzı (`Pulse`).

## Ses (Tamamen Prosedürel)

`AudioManager.cs` tüm sesleri runtime'da sentezler:
- **Karakter:** zıplama, iniş, ayak sesleri, yerçekimi tersleme "whoosh"u
- **Engeller:** diken fışkırması, zemin çökmesi, ezici çarpması, sahte zemin/düşen blok
- **Olaylar:** ölüm, bölüm geçme arpeji, UI buton tıklaması
- **Atmosfer:** kusursuz döngülenen düşük fon uğultusu (drone)

## Mimari (`Assets/Scripts`)

```
Core/
  GameBootstrap.cs    - Tek giriş noktası; her şeyi kurar (auto-boot)
  GameManager.cs      - Kamera, karakter, UI, seviye akışı, ölüm/respawn
  SpriteFactory.cs    - Tüm sprite'ları ve paleti runtime üretir
  AudioManager.cs     - Tüm sesleri ve fon uğultusunu prosedürel sentezler
  InputState.cs       - Dokunmatik + klavye girdi durumu
Player/
  PlayerController.cs - Rigidbody2D + BoxCollider2D fizik; coyote/buffer/yerçekimi ters
UI/
  TouchUIBuilder.cs   - Dokunmatik butonlar, HUD, ölüm flaşı (uGUI)
Levels/
  LevelManager.cs     - 5 seviyeyi ve tüm geometriyi kod ile inşa eder
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
