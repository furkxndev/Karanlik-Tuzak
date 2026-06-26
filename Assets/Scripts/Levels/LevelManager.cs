using System.Collections.Generic;
using UnityEngine;

namespace LevelDevilClone
{
    public struct LevelData
    {
        public Vector3 spawn;
        public float minX, maxX, minY, maxY;   // Kamera için dünya sınırları
        public string title;
        public string hint;
        public bool gravityFlip;               // Level 3 için
        public bool invertControls;            // Level 5 için
    }

    /// <summary>
    /// Beş seviyeyi ve tüm tuzakları tamamen kod ile inşa eder. Her ölümde
    /// seviye baştan kurulduğu için bütün tuzaklar otomatik sıfırlanır.
    /// </summary>
    public class LevelManager
    {
        private Transform _root;
        private Transform _player;

        public int Count => 5;

        public LevelData Build(int index, Transform player)
        {
            Clear();
            _root = new GameObject("Level_" + (index + 1)).transform;
            _player = player;

            switch (index)
            {
                case 0: return BuildLevel1();
                case 1: return BuildLevel2();
                case 2: return BuildLevel3();
                case 3: return BuildLevel4();
                default: return BuildLevel5();
            }
        }

        public void Clear()
        {
            if (_root != null) Object.Destroy(_root.gameObject);
            _root = null;
        }

        // =====================================================================
        //  LEVEL 1 — Yanıltıcı Başlangıç
        // =====================================================================
        private LevelData BuildLevel1()
        {
            // Uzun, güvenli görünen başlangıç zemini
            Ground(-4, 30, 0);

            // Çıkıştan hemen önceki kısa zemin parçası: gerçek zeminle aynı görünür
            // ama basınca çöker (troll #1). Yürürsen üstündeyken düşer; öğrenince atlar.
            MakeCollapsingBridge(30, 34, 0);

            // Çöken parçanın altındaki ölüm boşluğu
            DeathZone(32, 12, -10);

            // Çıkış zemini
            Ground(34, 46, 0);

            // Yukarıdan düşen ölümcül blok (troll #2): çıkışa yaklaşınca tetiklenir
            var fbRb = MakeHazardBlock(new Vector2(40, 11), new Vector2(2.2f, 2.2f));

            var trigger = new GameObject("FallTrigger");
            trigger.transform.SetParent(_root);
            trigger.transform.position = new Vector2(37f, 1.5f);
            var tcol = trigger.AddComponent<BoxCollider2D>();
            tcol.isTrigger = true;
            tcol.size = new Vector2(1.5f, 4f);
            var fo = trigger.AddComponent<FallingObstacle>();
            fo.block = fbRb;

            // Çıkış kapısı
            Exit(new Vector2(43f, 1.2f));

            return new LevelData
            {
                spawn = new Vector3(-1, 2, 0),
                minX = -4, maxX = 46, minY = -2, maxY = 9,
                title = "Level 1 — Sahte Guven",
                hint = "Cikisa cok yaklastin... ama acele etme demedik.",
                gravityFlip = false
            };
        }

        // =====================================================================
        //  LEVEL 2 — Görünmez Tehlikeler
        // =====================================================================
        private LevelData BuildLevel2()
        {
            // A: uzun güvenli başlangıç (koşu mesafesi)
            Ground(-4, 10, 0);

            // B: SAHTE zemin — basınca içinden düşersin (üstünden atlamalısın)
            MakeFakeGround(10, 13.5f, 0);

            // C: güvenli görünen geniş zemin ama ortasına basınca diken fışkırır
            Ground(13.5f, 21, 0);
            MakeAppearingSpikes(parentX: 16f, parentXEnd: 18f, baseY: 0f, triggerX: 14.6f);

            // D: ikinci SAHTE zemin (çıkıştan hemen önce)
            MakeFakeGround(21, 24.5f, 0);

            // E: çıkış zemini
            Ground(24.5f, 38, 0);

            // Tüm seviyenin altında ölüm bölgesi
            DeathZone(16, 50, -9);

            Exit(new Vector2(32, 1.2f));

            return new LevelData
            {
                spawn = new Vector3(-1, 2, 0),
                minX = -4, maxX = 38, minY = -2, maxY = 9,
                title = "Level 2 — Guvenli Gorunen",
                hint = "Her zemin gercek degil. Her bosluk da bos degil.",
                gravityFlip = false
            };
        }

        // =====================================================================
        //  LEVEL 3 — Ters Köşe (yerçekimi tersleme + kaçan kapı)
        // =====================================================================
        private LevelData BuildLevel3()
        {
            const float ceilY = 8f;   // Tavanın alt yüzeyi

            // Zemin İKİ parçaya bölünür: ortada gerçek bir ölüm boşluğu var.
            Ground(-4, 26, 0);
            Ground(31, 44, 0);
            // Kesintisiz tavan (koridorun üstü)
            Block("Ceiling", new Vector2(20, ceilY + 1.5f), new Vector2(48, 3),
                SpriteFactory.Ground, 0);
            Block("CeilingEdge", new Vector2(20, ceilY + 0.06f), new Vector2(48, 0.12f),
                SpriteFactory.GroundEdge, 1, solid: false);

            // Zemindeki dikenler -> burada tavanda olmalısın (geniş güvenli alanlar)
            MakeStaticSpikes(9, 12, 0f, pointUp: true);
            // Tavandaki dikenler -> burada zeminde olmalısın
            MakeStaticSpikes(18, 21, ceilY, pointUp: false);
            // Zemindeki ölüm boşluğu (26..31) -> tavandan geçmelisin
            DeathZone(28.5f, 5f, -2f);

            // Kaçan çıkış kapısı (troll): birkaç kez kaçar, sonra son noktada durur
            var doorObj = Exit(new Vector2(34, 1.2f));
            var fleeing = doorObj.gameObject.AddComponent<FleeingDoor>();
            fleeing.door = doorObj;

            var stops = new List<Transform>();
            stops.Add(MakeMarker(new Vector2(34, 1.2f)));
            stops.Add(MakeMarker(new Vector2(38, 1.2f)));
            stops.Add(MakeMarker(new Vector2(41.5f, 1.2f)));
            fleeing.stops = stops.ToArray();
            fleeing.Init(_player);

            return new LevelData
            {
                spawn = new Vector3(-1, 2, 0),
                minX = -4, maxX = 44, minY = -2, maxY = 11,
                title = "Level 3 — Ters Kose",
                hint = "Zipla tusu artik YERCEKIMINI ters cevirir. Tavanda yuru!",
                gravityFlip = true
            };
        }

        // =====================================================================
        //  LEVEL 4 — Ezici Koridor + Yalancı Kapı
        // =====================================================================
        private LevelData BuildLevel4()
        {
            // Düz koridor zemini
            Ground(-4, 50, 0);

            // Üç ritmik ezici piston (farklı fazlar -> zamanlama bulmacası)
            MakeCrusher(12f, topY: 5.0f, bottomY: 2.0f, period: 2.0f, phase: 0.0f);
            MakeCrusher(18f, topY: 5.0f, bottomY: 2.0f, period: 2.0f, phase: 0.9f);
            MakeCrusher(24f, topY: 5.0f, bottomY: 2.0f, period: 2.0f, phase: 1.5f);

            // Aralarda ufak bir nefes alanı, sonra ikinci ezici grup
            MakeCrusher(31f, topY: 5.0f, bottomY: 2.0f, period: 1.7f, phase: 0.3f);
            MakeCrusher(36f, topY: 5.0f, bottomY: 2.0f, period: 1.7f, phase: 1.1f);

            // YALANCI çıkış kapısı: gerçeğiyle aynı görünür ama öldürür (troll)
            Exit(new Vector2(42f, 1.2f), decoy: true);

            // Gerçek çıkış biraz ileride
            Exit(new Vector2(47.5f, 1.2f));

            return new LevelData
            {
                spawn = new Vector3(-1, 2, 0),
                minX = -4, maxX = 50, minY = -2, maxY = 9,
                title = "Level 4 — Ezici Koridor",
                hint = "Zamanlama her sey. Ve her kapi cikis degildir.",
                gravityFlip = false
            };
        }

        // =====================================================================
        //  LEVEL 5 — Ters Kontroller (Final)
        // =====================================================================
        private LevelData BuildLevel5()
        {
            // Tüm öğrenilen tuzakların karışımı + kontroller TERS (sol<->sağ).
            Ground(-4, 8, 0);
            MakeFakeGround(8, 11, 0);          // sahte zemin
            Ground(11, 19, 0);
            MakeAppearingSpikes(parentX: 14f, parentXEnd: 16f, baseY: 0f, triggerX: 12.6f);
            MakeCollapsingBridge(19, 23, 0);   // çöken köprü
            Ground(23, 30, 0);
            MakeCrusher(26f, topY: 5.0f, bottomY: 2.0f, period: 1.8f, phase: 0.5f); // ezici
            MakeFakeGround(30, 33, 0);         // son sahte zemin
            Ground(33, 46, 0);

            DeathZone(20, 62, -9);

            // Çıkış (ters kontrollerle ulaşması yeterince zor)
            Exit(new Vector2(41, 1.2f));

            return new LevelData
            {
                spawn = new Vector3(-1, 2, 0),
                minX = -4, maxX = 46, minY = -2, maxY = 9,
                title = "Level 5 — Ters Kontroller",
                hint = "Sol artik sag, sag artik sol. Bol sans.",
                invertControls = true
            };
        }

        // =====================================================================
        //  YARDIMCI İNŞA METOTLARI
        // =====================================================================
        private GameObject Block(string name, Vector2 center, Vector2 size,
            Color color, int order = 0, bool solid = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root);
            go.transform.position = center;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square;
            sr.color = color;
            sr.sortingOrder = order;

            if (solid)
            {
                var col = go.AddComponent<BoxCollider2D>();
                col.size = Vector2.one;
            }
            return go;
        }

        /// <summary>Katmanlı zemin görseli (koyu taban + gövde + parlak üst pervaz).
        /// Çarpışma collider'ı taban blokta. Oluşturulan tüm SpriteRenderer'lar
        /// renk değiştirebilmek için (sahte zemin solması) döndürülür.</summary>
        private GameObject GroundVisual(float xStart, float xEnd, float topY, string label)
        {
            float w = xEnd - xStart;
            float cx = (xStart + xEnd) * 0.5f;
            // Taban (solid) — koyu yüz
            var baseGo = Block(label, new Vector2(cx, topY - 1.5f), new Vector2(w, 3f),
                SpriteFactory.GroundFace, 0);
            // Üst gövde bandı (biraz daha açık)
            Block(label + "Body", new Vector2(cx, topY - 0.28f), new Vector2(w, 0.56f),
                SpriteFactory.Ground, 1, solid: false).transform.SetParent(baseGo.transform, true);
            // Parlak üst pervaz
            Block(label + "Edge", new Vector2(cx, topY - 0.05f), new Vector2(w, 0.1f),
                SpriteFactory.GroundEdge, 2, solid: false).transform.SetParent(baseGo.transform, true);
            return baseGo;
        }

        private void Ground(float xStart, float xEnd, float topY)
        {
            GroundVisual(xStart, xEnd, topY, "Ground");
        }

        private void MakeFakeGround(float xStart, float xEnd, float topY)
        {
            // Gerçek zeminle BİREBİR aynı görünür — asıl tuzak budur.
            var go = GroundVisual(xStart, xEnd, topY, "FakeGround");
            var fake = go.AddComponent<FakePlatform>();
            // Çöktüğünde tüm parçalar solsun diye gövde rengini taban SR üzerinden
            // değil, FakePlatform yalnızca collider'ı kapatıp tabanı soldurur.
            fake.Setup(go.GetComponent<SpriteRenderer>(), go.GetComponent<BoxCollider2D>());
        }

        private void DeathZone(float cx, float width, float cy)
        {
            var go = new GameObject("DeathZone");
            go.transform.SetParent(_root);
            go.transform.position = new Vector2(cx, cy);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(width, 3f);
            go.AddComponent<Hazard>();
        }

        private ExitDoor Exit(Vector2 pos, bool decoy = false)
        {
            var go = new GameObject(decoy ? "DecoyDoor" : "ExitDoor");
            go.transform.SetParent(_root);
            go.transform.position = pos;

            // Yumuşak parıltı halesi (arkada, nabız gibi atar)
            var glow = MakeChild(go.transform, "DoorGlow", pos, new Vector2(2.8f, 3.6f),
                SpriteFactory.ExitGlow, 2, SpriteFactory.SoftCircle);
            // Yuvarlatılmış dış çerçeve
            MakeChild(go.transform, "DoorFrame", pos, new Vector2(1.7f, 2.9f),
                SpriteFactory.ExitFrame, 3, SpriteFactory.RoundedSquare);
            // İç portal paneli
            MakeChild(go.transform, "DoorPanel", pos, new Vector2(1.2f, 2.4f),
                SpriteFactory.ExitPanel, 4, SpriteFactory.RoundedSquare);
            // Parlak iç çekirdek
            MakeChild(go.transform, "DoorCore", pos + new Vector2(0f, 0.1f),
                new Vector2(0.7f, 1.6f), SpriteFactory.ExitGlow, 5, SpriteFactory.SoftCircle);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 2.6f);

            if (decoy)
            {
                // Gerçek çıkışla birebir aynı görünür ama dokununca öldürür (troll).
                go.AddComponent<Hazard>();
                return null;
            }

            var door = go.AddComponent<ExitDoor>();
            door.Setup(glow.GetComponent<SpriteRenderer>());
            return door;
        }

        /// <summary>Belirli bir sprite ile çocuk görsel parça (collider'sız).</summary>
        private GameObject MakeChild(Transform parent, string name, Vector2 worldPos,
            Vector2 size, Color color, int order, Sprite sprite)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return go;
        }

        /// <summary>Detaylı, parıltılı, tehlike desenli düşen ölümcül blok.
        /// Kök ölçeği 1; FallingObstacle bunu dinamik yapıp düşürür.</summary>
        private Rigidbody2D MakeHazardBlock(Vector2 pos, Vector2 size)
        {
            var go = new GameObject("HazardBlock");
            go.transform.SetParent(_root);
            go.transform.position = pos;

            // Arka parıltı (nabız atar)
            var glow = MakeChild(go.transform, "BlockGlow", pos, size * 1.55f,
                new Color(SpriteFactory.HazardBright.r, SpriteFactory.HazardBright.g,
                    SpriteFactory.HazardBright.b, 0.3f), 2, SpriteFactory.SoftCircle);
            var gp = glow.AddComponent<Pulse>();
            gp.speed = 5f; gp.minAlpha = 0.15f; gp.maxAlpha = 0.38f;

            // Gövde + koyu çekirdek
            MakeChild(go.transform, "BlockBody", pos, size,
                SpriteFactory.HazardBright, 4, SpriteFactory.RoundedSquare);
            MakeChild(go.transform, "BlockCore", pos, size * 0.6f,
                SpriteFactory.HazardDeep, 5, SpriteFactory.RoundedSquare);

            // Çapraz tehlike şeritleri (chevron)
            for (int k = -1; k <= 1; k++)
            {
                MakeChild(go.transform, "Chevron",
                    pos + new Vector2(k * size.x * 0.28f, 0f),
                    new Vector2(size.x * 0.16f, size.y * 1.25f),
                    SpriteFactory.HazardDeep, 6, SpriteFactory.RoundedSquare)
                    .transform.localRotation = Quaternion.Euler(0, 0, 38f);
            }

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            go.AddComponent<Hazard>();
            go.AddComponent<BlockImpact>();
            return rb;
        }

        /// <summary>Gerçek zeminle aynı görünen ama basınca çöken köprü.</summary>
        private void MakeCollapsingBridge(float xStart, float xEnd, float topY)
        {
            var bridge = GroundVisual(xStart, xEnd, topY, "CollapseBridge");
            var collapse = bridge.AddComponent<CollapsingPlatform>();
            collapse.Setup(bridge.GetComponent<SpriteRenderer>(),
                bridge.GetComponent<BoxCollider2D>());
        }

        /// <summary>Tavandan ritmik inip kalkan ölümcül ezici piston (metalik, perçinli,
        /// uyarı şeritli ve dişli). Kök ölçeği 1 — görsel parçalar bozulmaz.</summary>
        private void MakeCrusher(float x, float topY, float bottomY, float period, float phase)
        {
            var go = new GameObject("Crusher");
            go.transform.SetParent(_root);
            go.transform.position = new Vector2(x, topY);

            const float w = 1.9f, h = 4f;
            // Koyu gövde tabanı
            MakeChild(go.transform, "Body", new Vector2(x, topY), new Vector2(w, h),
                SpriteFactory.GroundFace, 4, SpriteFactory.RoundedSquare);
            // Metalik yüz (daha açık)
            MakeChild(go.transform, "Face", new Vector2(x, topY + 0.1f), new Vector2(w - 0.22f, h - 0.4f),
                SpriteFactory.Ground, 5, SpriteFactory.RoundedSquare);
            // Üst bevel parıltısı
            MakeChild(go.transform, "Bevel", new Vector2(x, topY + h * 0.5f - 0.45f),
                new Vector2(w - 0.5f, 0.7f), SpriteFactory.GroundEdge, 6, SpriteFactory.RoundedSquare);
            // Perçinler (köşelerde metal studlar)
            float rx = w * 0.5f - 0.32f, ry = h * 0.5f - 0.4f;
            foreach (var off in new[] { new Vector2(-rx, ry), new Vector2(rx, ry),
                                        new Vector2(-rx, -ry + 0.3f), new Vector2(rx, -ry + 0.3f) })
                MakeChild(go.transform, "Rivet", new Vector2(x + off.x, topY + off.y),
                    new Vector2(0.18f, 0.18f), SpriteFactory.GroundFace, 7, SpriteFactory.Circle);

            // Nabız atan tehlike şeridi (alt kenar)
            var stripe = MakeChild(go.transform, "DangerStripe", new Vector2(x, topY - h * 0.5f + 0.55f),
                new Vector2(w, 0.3f), SpriteFactory.HazardBright, 8, SpriteFactory.Square);
            var sp = stripe.AddComponent<Pulse>();
            sp.speed = 6f; sp.minAlpha = 0.4f; sp.maxAlpha = 1f;

            // Diş sırası (aşağı bakan üçgenler)
            int teeth = 4;
            float tw = w / teeth;
            for (int k = 0; k < teeth; k++)
            {
                float tx = x - w * 0.5f + tw * (k + 0.5f);
                float ty = topY - h * 0.5f + 0.05f;
                MakeChild(go.transform, "ToothOutline", new Vector2(tx, ty),
                    new Vector2(tw * 1.1f, 0.55f), SpriteFactory.HazardDeep, 8, SpriteFactory.Triangle)
                    .transform.localRotation = Quaternion.Euler(0, 0, 180);
                MakeChild(go.transform, "Tooth", new Vector2(tx, ty + 0.02f),
                    new Vector2(tw * 0.85f, 0.45f), SpriteFactory.HazardBright, 9, SpriteFactory.Triangle)
                    .transform.localRotation = Quaternion.Euler(0, 0, 180);
            }

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(w, h);
            go.AddComponent<Rigidbody2D>();   // Crusher.Awake bunu Kinematic yapar
            go.AddComponent<Hazard>();
            var cr = go.AddComponent<Crusher>();
            cr.topY = topY; cr.bottomY = bottomY; cr.period = period; cr.phase = phase;
        }

        private Transform MakeMarker(Vector2 pos)
        {
            var go = new GameObject("Stop");
            go.transform.SetParent(_root);
            go.transform.position = pos;
            return go.transform;
        }

        /// <summary>Sabit, her zaman ölümcül diken sırası.</summary>
        private void MakeStaticSpikes(float xStart, float xEnd, float baseY, bool pointUp)
        {
            var root = new GameObject("Spikes");
            root.transform.SetParent(_root);
            root.transform.position = new Vector2((xStart + xEnd) * 0.5f, baseY);

            BuildSpikeVisuals(root.transform, xEnd - xStart, pointUp, out _);

            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(xEnd - xStart, 0.6f);
            col.offset = new Vector2(0f, pointUp ? 0.35f : -0.35f);
            root.AddComponent<Hazard>();
        }

        /// <summary>Level 2: gizli, basınca fışkıran dikenler.</summary>
        private void MakeAppearingSpikes(float parentX, float parentXEnd, float baseY,
            float triggerX)
        {
            float width = parentXEnd - parentX;
            float cx = (parentX + parentXEnd) * 0.5f;

            // Diken kökü (gizli başlar, aşağıdan yukarı fışkırır)
            var spikesRoot = new GameObject("AppearSpikes");
            spikesRoot.transform.SetParent(_root);
            spikesRoot.transform.position = new Vector2(cx, baseY);

            BuildSpikeVisuals(spikesRoot.transform, width, true, out var rends);

            var hcol = spikesRoot.AddComponent<BoxCollider2D>();
            hcol.isTrigger = true;
            hcol.size = new Vector2(width, 0.7f);
            hcol.offset = new Vector2(0f, 0.4f);
            var hazard = spikesRoot.AddComponent<Hazard>();

            // Tetikleyici bölge
            var trig = new GameObject("SpikeTrigger");
            trig.transform.SetParent(_root);
            trig.transform.position = new Vector2(triggerX, baseY + 1f);
            var tcol = trig.AddComponent<BoxCollider2D>();
            tcol.isTrigger = true;
            tcol.size = new Vector2(1.2f, 3f);

            var appear = trig.AddComponent<AppearingSpikes>();
            appear.spikesRoot = spikesRoot.transform;
            appear.renderers = rends;
            appear.hazards = new[] { hazard };
            // Level kökü orijinde olduğundan localPosition == world position.
            appear.hiddenY = baseY - 1.2f;   // Zemin altında saklı başlar
            appear.shownY = baseY;           // Tetiklenince yüzeye fışkırır
            appear.Init();
        }

        private void BuildSpikeVisuals(Transform parent, float width, bool pointUp,
            out SpriteRenderer[] renderers)
        {
            float spikeW = 0.7f;
            int count = Mathf.Max(1, Mathf.RoundToInt(width / spikeW));
            spikeW = width / count;
            float sign = pointUp ? 1f : -1f;
            float rot = pointUp ? 0f : 180f;
            var list = new List<SpriteRenderer>();
            Color tipColor = Color.Lerp(SpriteFactory.HazardBright, Color.white, 0.45f);

            // Arkada yumuşak tehlike parıltısı (nabız atar)
            var glow = AddSpikeSprite(parent, "SpikeGlow",
                new Vector3(0f, sign * 0.32f, 0.1f), new Vector2(width * 1.05f, 1.35f),
                new Color(SpriteFactory.HazardBright.r, SpriteFactory.HazardBright.g,
                    SpriteFactory.HazardBright.b, 0.28f), 2, SpriteFactory.SoftCircle, 0f);
            var pulse = glow.gameObject.AddComponent<Pulse>();
            pulse.speed = 4f; pulse.minAlpha = 0.12f; pulse.maxAlpha = 0.32f;
            list.Add(glow);

            // Taban plakası (koyu metal kaide)
            list.Add(AddSpikeSprite(parent, "SpikeBase",
                new Vector3(0f, sign * 0.05f, 0f), new Vector2(width, 0.24f),
                SpriteFactory.HazardDeep, 3, SpriteFactory.RoundedSquare, 0f));

            for (int i = 0; i < count; i++)
            {
                float lx = -width * 0.5f + spikeW * (i + 0.5f);

                // Koyu anahat (biraz büyük, arkada)
                list.Add(AddSpikeSprite(parent, "SpikeOutline" + i,
                    new Vector3(lx, sign * 0.34f, 0f), new Vector2(spikeW * 1.18f, 0.82f),
                    SpriteFactory.HazardDeep, 3, SpriteFactory.Triangle, rot));
                // Ana diken gövdesi
                list.Add(AddSpikeSprite(parent, "Spike" + i,
                    new Vector3(lx, sign * 0.35f, 0f), new Vector2(spikeW, 0.7f),
                    SpriteFactory.HazardBright, 4, SpriteFactory.Triangle, rot));
                // Parlak uç vurgusu (specular)
                list.Add(AddSpikeSprite(parent, "SpikeTip" + i,
                    new Vector3(lx - spikeW * 0.12f, sign * 0.48f, 0f),
                    new Vector2(spikeW * 0.32f, 0.42f),
                    tipColor, 5, SpriteFactory.Triangle, rot));
            }
            renderers = list.ToArray();
        }

        private SpriteRenderer AddSpikeSprite(Transform parent, string name, Vector3 localPos,
            Vector2 size, Color color, int order, Sprite sprite, float rotZ)
        {
            var s = new GameObject(name);
            s.transform.SetParent(parent, false);
            s.transform.localPosition = localPos;
            s.transform.localScale = new Vector3(size.x, size.y, 1f);
            if (rotZ != 0f) s.transform.localRotation = Quaternion.Euler(0, 0, rotZ);
            var sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
