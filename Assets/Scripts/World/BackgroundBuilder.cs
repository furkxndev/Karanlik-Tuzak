using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Atmosferik, çok katmanlı arka planı kod ile kurar: gradient gökyüzü,
    /// uzak/yakın siluet sıradağları (parallax), sürüklenen sis ve vignette.
    /// Karanlık ama düz değil — hafif morumsu derinlik hissi verir.
    /// </summary>
    public class BackgroundBuilder : MonoBehaviour
    {
        private Camera _cam;
        private Transform _camT;
        private SpriteRenderer _sky;

        public void Build(Camera cam)
        {
            _cam = cam;
            _camT = cam.transform;

            // --- Gökyüzü: kameraya bağlı, her zaman ekranı kaplar ---
            var skyGo = new GameObject("Sky");
            skyGo.transform.SetParent(_camT, false);
            skyGo.transform.localPosition = new Vector3(0, 0, 20f);
            _sky = skyGo.AddComponent<SpriteRenderer>();
            _sky.sprite = SpriteFactory.Gradient(
                SpriteFactory.SkyTop, SpriteFactory.SkyMid, SpriteFactory.SkyBottom);
            _sky.sortingOrder = -100;
            FitSkyToCamera();

            // --- Ufuk parıltısı (sönük, geniş) ---
            var glow = new GameObject("HorizonGlow");
            glow.transform.SetParent(_camT, false);
            glow.transform.localPosition = new Vector3(0, -1.5f, 19f);
            var gsr = glow.AddComponent<SpriteRenderer>();
            gsr.sprite = SpriteFactory.SoftCircle;   // renklendirilebilir yumuşak parıltı
            gsr.color = new Color(SpriteFactory.Horizon.r, SpriteFactory.Horizon.g,
                SpriteFactory.Horizon.b, 0.4f);
            gsr.sortingOrder = -98;
            float h = _cam.orthographicSize * 2f;
            glow.transform.localScale = new Vector3(h * _cam.aspect * 1.6f, h * 1.2f, 1f);

            // --- Uzak siluet sıradağları (yavaş parallax) ---
            BuildSilhouetteLayer("FarMountains", SpriteFactory.SilhouetteFar,
                yBase: -2.0f, height: 9f, step: 7f, jitter: 3f, z: 15f, parallax: 0.82f, order: -90);

            // --- Yakın siluet sütunları (daha hızlı parallax) ---
            BuildSilhouetteLayer("NearPillars", SpriteFactory.SilhouetteNear,
                yBase: -3.0f, height: 7f, step: 5.5f, jitter: 1.5f, z: 10f, parallax: 0.62f, order: -80);

            // --- Sürüklenen sis bulutları ---
            BuildFog();

            // --- Vignette (ekran kenarları koyu) — kameraya bağlı ---
            var vig = new GameObject("Vignette");
            vig.transform.SetParent(_camT, false);
            vig.transform.localPosition = new Vector3(0, 0, 5f);
            var vsr = vig.AddComponent<SpriteRenderer>();
            vsr.sprite = SpriteFactory.Vignette;
            vsr.color = new Color(0f, 0f, 0f, 0.3f);
            vsr.sortingOrder = 50;   // dünya üstünde, UI altında
            float vh = _cam.orthographicSize * 2f;
            vig.transform.localScale = new Vector3(vh * _cam.aspect * 1.15f, vh * 1.15f, 1f);
        }

        private void FitSkyToCamera()
        {
            float h = _cam.orthographicSize * 2f;
            float w = h * _cam.aspect;
            // Sprite'ın taban boyutuna göre ölçekle (gradient tabanı 2x256 birim).
            Vector2 baseSize = _sky.sprite.bounds.size;
            _sky.transform.localScale = new Vector3(
                (w * 1.25f) / baseSize.x,
                (h * 1.25f) / baseSize.y, 1f);
        }

        private void BuildSilhouetteLayer(string name, Color color, float yBase,
            float height, float step, float jitter, float z, float parallax, int order)
        {
            var root = new GameObject(name);
            root.transform.position = new Vector3(0, 0, z);
            var pl = root.AddComponent<ParallaxLayer>();
            pl.cam = _camT;
            pl.factor = parallax;

            // Determinist "rastgele" (her seferinde aynı manzara).
            int i = 0;
            for (float x = -40f; x <= 80f; x += step)
            {
                float hgt = height + PseudoRandom(i * 1.7f) * jitter;
                float w = step * (0.75f + PseudoRandom(i * 0.9f) * 0.4f);
                var col = new GameObject("Spire");
                col.transform.SetParent(root.transform);
                col.transform.position = new Vector3(x, yBase + hgt * 0.5f, z);
                col.transform.localScale = new Vector3(w, hgt, 1f);
                var sr = col.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.RoundedSquare;  // yumuşak tepeli sütun
                sr.color = color;
                sr.sortingOrder = order;
                i++;
            }
        }

        private void BuildFog()
        {
            var root = new GameObject("Fog");
            root.transform.position = new Vector3(0, 0, 12f);
            var pl = root.AddComponent<ParallaxLayer>();
            pl.cam = _camT;
            pl.factor = 0.7f;

            for (int i = 0; i < 14; i++)
            {
                var f = new GameObject("FogPuff");
                f.transform.SetParent(root.transform);
                float x = -40f + i * 9f + PseudoRandom(i * 2.3f) * 6f;
                float y = -1f + PseudoRandom(i * 3.1f) * 7f;
                float s = 4f + PseudoRandom(i * 1.3f) * 5f;
                f.transform.position = new Vector3(x, y, 12f);
                f.transform.localScale = new Vector3(s * 1.6f, s, 1f);
                var sr = f.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.SoftCircle;   // renklendirilebilir yumuşak leke
                sr.color = new Color(SpriteFactory.Fog.r, SpriteFactory.Fog.g,
                    SpriteFactory.Fog.b, 0.06f + PseudoRandom(i * 4.7f) * 0.06f);
                sr.sortingOrder = -70;
                var drift = f.AddComponent<FogDrift>();
                drift.speed = 0.15f + PseudoRandom(i * 5.5f) * 0.25f;
                drift.amplitude = 0.4f + PseudoRandom(i * 6.1f) * 0.6f;
                drift.phase = PseudoRandom(i * 7.9f) * 6.28f;
            }
        }

        // 0..1 arası determinist sözde-rastgele (Math.Random kullanmadan).
        private static float PseudoRandom(float seed)
        {
            float v = Mathf.Sin(seed * 12.9898f) * 43758.5453f;
            return v - Mathf.Floor(v);
        }
    }

    /// <summary>Sis lekelerine hafif salınım verir.</summary>
    public class FogDrift : MonoBehaviour
    {
        public float speed = 0.2f;
        public float amplitude = 0.5f;
        public float phase;
        private Vector3 _start;

        private void Start() => _start = transform.localPosition;

        private void Update()
        {
            float t = Time.time * speed + phase;
            transform.localPosition = _start + new Vector3(Mathf.Sin(t) * amplitude,
                Mathf.Cos(t * 0.7f) * amplitude * 0.4f, 0f);
        }
    }
}
