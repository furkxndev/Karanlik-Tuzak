using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Tüm sprite'ları runtime'da kod ile üretir. Böylece projeye hiçbir
    /// görsel asset import etmeye gerek kalmaz; her şey karanlık tema paletiyle
    /// otomatik oluşur.
    /// </summary>
    public static class SpriteFactory
    {
        private static Sprite _square;
        private static Sprite _triangle;
        private static Sprite _circle;
        private static Sprite _rounded;
        private static Sprite _vignette;
        private static Sprite _softCircle;

        // ---- Aydınlatılmış alacakaranlık paleti (atmosferik ama açık) ----
        public static readonly Color Background  = Hex("23223A");
        public static readonly Color Ground      = Hex("33334A");
        public static readonly Color GroundEdge  = Hex("5A5A7A");
        public static readonly Color GroundFace  = Hex("262638");
        public static readonly Color Platform    = Hex("3A3A52");
        public static readonly Color FakeColor    = Hex("3A3A52");
        // Tehlike: mor-mavi arka planın tamamlayıcısı sıcak mercan/turuncu-kırmızı
        public static readonly Color Hazard      = Hex("A8412A");
        public static readonly Color HazardBright = Hex("EC7148");
        public static readonly Color HazardDeep  = Hex("6E2A22");
        // Çıkış: arka planla uyumlu (analog) camgöbeği portal
        public static readonly Color ExitGlow    = Hex("6FE6E0");
        public static readonly Color ExitPanel   = Hex("2C7E8C");
        public static readonly Color ExitFrame   = Hex("203A45");

        // ---- Arka plan (aydınlık, renkli alacakaranlık) ----
        public static readonly Color SkyTop      = Hex("514C78");  // açık dusk moru
        public static readonly Color SkyMid      = Hex("3A3658");
        public static readonly Color SkyBottom   = Hex("262338");
        public static readonly Color Horizon     = Hex("8A6AA0");  // sıcak morumsu ufuk parıltısı
        public static readonly Color SilhouetteFar  = Hex("3A3658");
        public static readonly Color SilhouetteNear = Hex("2A2742");
        public static readonly Color Fog         = Hex("6A6498");

        // ---- Karakter (Level Devil tarzı küçük figür) ----
        public static readonly Color CharBody    = Hex("D2D2DC");
        public static readonly Color CharOutline = Hex("3A3A47");
        public static readonly Color CharLimb    = Hex("B9B9C6");
        public static readonly Color CharEye     = Hex("17171D");
        public static readonly Color CharBlush   = Hex("8A1F2A");

        // ---- UI ----
        public static readonly Color UiPanel     = new Color(0.10f, 0.10f, 0.14f, 0.55f);
        public static readonly Color UiText      = Hex("D6D6DE");

        /// <summary>Düz dolu kare sprite (1 birim = 1 unit, PPU = 1).</summary>
        public static Sprite Square
        {
            get
            {
                if (_square == null)
                {
                    var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    tex.SetPixel(0, 0, Color.white);
                    tex.filterMode = FilterMode.Point;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    _square = Sprite.Create(tex, new Rect(0, 0, 1, 1),
                        new Vector2(0.5f, 0.5f), 1f);
                    _square.name = "sf_square";
                }
                return _square;
            }
        }

        /// <summary>Yukarı bakan üçgen (diken için).</summary>
        public static Sprite Triangle
        {
            get
            {
                if (_triangle == null)
                {
                    const int s = 32;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                    for (int y = 0; y < s; y++)
                    {
                        // Tabanı altta, tepesi üstte olan üçgen.
                        float halfWidth = (1f - (float)y / s) * (s * 0.5f);
                        float center = s * 0.5f;
                        for (int x = 0; x < s; x++)
                        {
                            bool inside = Mathf.Abs(x - center) <= halfWidth;
                            tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                        }
                    }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    _triangle = Sprite.Create(tex, new Rect(0, 0, s, s),
                        new Vector2(0.5f, 0.5f), s);
                    _triangle.name = "sf_triangle";
                }
                return _triangle;
            }
        }

        /// <summary>Dolu daire (parçacık / detay için).</summary>
        public static Sprite Circle
        {
            get
            {
                if (_circle == null)
                {
                    const int s = 32;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                    Vector2 c = new Vector2(s / 2f, s / 2f);
                    float r = s / 2f - 1f;
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                            tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
                        }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.Apply();
                    _circle = Sprite.Create(tex, new Rect(0, 0, s, s),
                        new Vector2(0.5f, 0.5f), s);
                    _circle.name = "sf_circle";
                }
                return _circle;
            }
        }

        /// <summary>Yuvarlatılmış köşeli kare — karakter gövdesi/uzuvları için.</summary>
        public static Sprite RoundedSquare
        {
            get
            {
                if (_rounded == null)
                {
                    const int s = 64;
                    float r = 18f;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float dx = Mathf.Max(Mathf.Abs(x + 0.5f - s / 2f) - (s / 2f - r), 0f);
                            float dy = Mathf.Max(Mathf.Abs(y + 0.5f - s / 2f) - (s / 2f - r), 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            float a = Mathf.Clamp01(r - d);           // kenar yumuşatma
                            tex.SetPixel(x, y, new Color(1, 1, 1, a));
                        }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    _rounded = Sprite.Create(tex, new Rect(0, 0, s, s),
                        new Vector2(0.5f, 0.5f), s);
                    _rounded.name = "sf_rounded";
                }
                return _rounded;
            }
        }

        /// <summary>9-slice'a uygun (border'lı) yuvarlak köşeli buton sprite'ı.
        /// Geniş/yüksek butonlarda köşeler bozulmaz.</summary>
        private static Sprite _roundedBtn;
        public static Sprite RoundedButton
        {
            get
            {
                if (_roundedBtn == null)
                {
                    const int s = 64;
                    const float r = 22f;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float dx = Mathf.Max(Mathf.Abs(x + 0.5f - s / 2f) - (s / 2f - r), 0f);
                            float dy = Mathf.Max(Mathf.Abs(y + 0.5f - s / 2f) - (s / 2f - r), 0f);
                            float d = Mathf.Sqrt(dx * dx + dy * dy);
                            float a = Mathf.Clamp01(r - d);
                            tex.SetPixel(x, y, new Color(1, 1, 1, a));
                        }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    _roundedBtn = Sprite.Create(tex, new Rect(0, 0, s, s),
                        new Vector2(0.5f, 0.5f), s, 0, SpriteMeshType.FullRect,
                        new Vector4(r, r, r, r));
                    _roundedBtn.name = "sf_roundedbtn";
                }
                return _roundedBtn;
            }
        }

        /// <summary>Dikey 3 renkli gradient (gökyüzü). Esnetilerek kullanılır.</summary>
        public static Sprite Gradient(Color top, Color mid, Color bottom)
        {
            const int h = 256;
            var tex = new Texture2D(2, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                float t = (float)y / (h - 1);
                Color c = t < 0.5f
                    ? Color.Lerp(bottom, mid, t * 2f)
                    : Color.Lerp(mid, top, (t - 0.5f) * 2f);
                tex.SetPixel(0, y, c);
                tex.SetPixel(1, y, c);
            }
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            var sp = Sprite.Create(tex, new Rect(0, 0, 2, h), new Vector2(0.5f, 0.5f), 1f);
            sp.name = "sf_gradient";
            return sp;
        }

        /// <summary>Radyal vignette — merkez şeffaf, kenarlar koyu. Ekran kaplaması için.</summary>
        public static Sprite Vignette
        {
            get
            {
                if (_vignette == null)
                {
                    const int s = 256;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                    Vector2 c = new Vector2(s / 2f, s / 2f);
                    float maxD = s * 0.72f;
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), c) / maxD;
                            float a = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.45f) / 0.55f));
                            tex.SetPixel(x, y, new Color(0, 0, 0, a));
                        }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    _vignette = Sprite.Create(tex, new Rect(0, 0, s, s),
                        new Vector2(0.5f, 0.5f), s);
                    _vignette.name = "sf_vignette";
                }
                return _vignette;
            }
        }

        /// <summary>Beyaz yumuşak daire (merkez opak → kenar şeffaf). Renklendirilebilir;
        /// parıltı ve sis lekeleri için.</summary>
        public static Sprite SoftCircle
        {
            get
            {
                if (_softCircle == null)
                {
                    const int s = 128;
                    var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
                    Vector2 c = new Vector2(s / 2f, s / 2f);
                    float r = s / 2f;
                    for (int y = 0; y < s; y++)
                        for (int x = 0; x < s; x++)
                        {
                            float d = Vector2.Distance(new Vector2(x, y), c) / r;
                            float a = Mathf.Clamp01(1f - d);
                            a = a * a;   // merkeze doğru yumuşak yoğunlaşma
                            tex.SetPixel(x, y, new Color(1, 1, 1, a));
                        }
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.Apply();
                    _softCircle = Sprite.Create(tex, new Rect(0, 0, s, s),
                        new Vector2(0.5f, 0.5f), s);
                    _softCircle.name = "sf_softcircle";
                }
                return _softCircle;
            }
        }

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
