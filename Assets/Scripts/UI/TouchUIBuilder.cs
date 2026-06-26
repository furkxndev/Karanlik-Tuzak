using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LevelDevilClone
{
    /// <summary>
    /// Yalnızca dokunmatik kontrolleri (Sol / Sağ / Zıpla) ve ölüm flaşını kurar.
    /// Hiçbir yazı yoktur — butonlar ok sprite'larıyla çizilir (profesyonel, sade görünüm).
    /// Sol-Sağ basılı tutmayı, Zıpla anlık basışı destekler. Klavye de çalışır.
    /// </summary>
    public class TouchUIBuilder
    {
        public Image flash;        // Ölüm anı kırmızı parlaması
        public Canvas canvas;

        // Üst HUD + level geçiş katmanı
        public Text levelLabel;            // "BÖLÜM 1/5"
        public Image transition;           // Tam ekran geçiş örtüsü
        public Text transitionLabel;       // Geçiş sırasında "BÖLÜM X"
        public System.Action onMenuPressed; // Üstteki MENÜ butonu

        private GameObject _controlsRoot;   // Sol/Sağ/Zıpla + HUD (menüde gizlenir)
        private readonly InputState _input;
        private Font _font;

        private Font UIFont
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _font;
            }
        }

        public TouchUIBuilder(InputState input)
        {
            _input = input;
        }

        public void Build()
        {
            EnsureEventSystem();

            // ---- Canvas ----
            var canvasGo = new GameObject("GameCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // ---- Ölüm flaşı (tam ekran, başta görünmez) ----
            var flashGo = new GameObject("DeathFlash", typeof(RectTransform));
            flashGo.transform.SetParent(canvasGo.transform, false);
            var frt = flashGo.GetComponent<RectTransform>();
            frt.anchorMin = Vector2.zero; frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero; frt.offsetMax = Vector2.zero;
            flash = flashGo.AddComponent<Image>();
            flash.color = new Color(0.55f, 0.07f, 0.07f, 0f);
            flash.raycastTarget = false;

            // ---- Kontrol butonları (tek kök altında — menüde toplu gizlenir) ----
            _controlsRoot = new GameObject("Controls", typeof(RectTransform));
            _controlsRoot.transform.SetParent(canvasGo.transform, false);
            var crt = _controlsRoot.GetComponent<RectTransform>();
            crt.anchorMin = Vector2.zero; crt.anchorMax = Vector2.one;
            crt.offsetMin = Vector2.zero; crt.offsetMax = Vector2.zero;

            float size = 240f;
            float pad = 60f;
            CreateHoldButton("BtnLeft", AnchorBL, new Vector2(pad + 100, pad + size * 0.5f),
                new Vector2(size, size), 90f,
                () => { _input.Left = true; AudioManager.PlayClick(); },
                () => _input.Left = false);
            CreateHoldButton("BtnRight", AnchorBL, new Vector2(pad + size + 80, pad + size * 0.5f),
                new Vector2(size, size), -90f,
                () => { _input.Right = true; AudioManager.PlayClick(); },
                () => _input.Right = false);
            CreateHoldButton("BtnJump", AnchorBR, new Vector2(-(pad + size * 0.6f), pad + size * 0.6f),
                new Vector2(size * 1.2f, size * 1.2f), 0f,
                () => _input.PressJump(), () => _input.ReleaseJump());

            BuildHud();
            BuildTransition();
        }

        // ---- Üst HUD: MENÜ butonu + bölüm göstergesi ----
        private void BuildHud()
        {
            // MENÜ butonu (sol üst)
            var menu = new GameObject("BtnMenu", typeof(RectTransform));
            menu.transform.SetParent(_controlsRoot.transform, false);
            var mrt = menu.GetComponent<RectTransform>();
            mrt.anchorMin = mrt.anchorMax = new Vector2(0f, 1f);
            mrt.pivot = new Vector2(0f, 1f);
            mrt.sizeDelta = new Vector2(230, 96);
            mrt.anchoredPosition = new Vector2(40, -44);

            var mimg = menu.AddComponent<Image>();
            mimg.sprite = SpriteFactory.RoundedButton;
            mimg.type = Image.Type.Sliced;
            mimg.color = new Color(0.10f, 0.10f, 0.14f, 0.7f);

            var mlabel = CreateLabel(menu.transform, "‹ MENÜ", 40,
                new Color(SpriteFactory.UiText.r, SpriteFactory.UiText.g, SpriteFactory.UiText.b, 0.9f),
                TextAnchor.MiddleCenter);
            var lrt = mlabel.rectTransform;
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
            mlabel.raycastTarget = false;

            var trigger = menu.AddComponent<EventTrigger>();
            AddEvent(trigger, EventTriggerType.PointerDown,
                _ => menu.transform.localScale = Vector3.one * 0.95f);
            AddEvent(trigger, EventTriggerType.PointerUp,
                _ => menu.transform.localScale = Vector3.one);
            AddEvent(trigger, EventTriggerType.PointerExit,
                _ => menu.transform.localScale = Vector3.one);
            AddEvent(trigger, EventTriggerType.PointerClick, _ =>
            {
                menu.transform.localScale = Vector3.one;
                AudioManager.PlayClick();
                onMenuPressed?.Invoke();
            });

            // Bölüm göstergesi (üst orta)
            var lvlGo = new GameObject("LevelLabel", typeof(RectTransform));
            lvlGo.transform.SetParent(_controlsRoot.transform, false);
            var lvrt = lvlGo.GetComponent<RectTransform>();
            lvrt.anchorMin = lvrt.anchorMax = new Vector2(0.5f, 1f);
            lvrt.pivot = new Vector2(0.5f, 1f);
            lvrt.sizeDelta = new Vector2(600, 80);
            lvrt.anchoredPosition = new Vector2(0, -48);

            levelLabel = lvlGo.AddComponent<Text>();
            levelLabel.font = UIFont;
            levelLabel.text = "BÖLÜM 1";
            levelLabel.fontSize = 50;
            levelLabel.fontStyle = FontStyle.Bold;
            levelLabel.color = new Color(SpriteFactory.UiText.r, SpriteFactory.UiText.g,
                SpriteFactory.UiText.b, 0.92f);
            levelLabel.alignment = TextAnchor.MiddleCenter;
            levelLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            levelLabel.verticalOverflow = VerticalWrapMode.Overflow;
            levelLabel.raycastTarget = false;
            var sh = lvlGo.AddComponent<Shadow>();
            sh.effectColor = new Color(0f, 0f, 0f, 0.5f);
            sh.effectDistance = new Vector2(2, -3);
        }

        // ---- Level geçiş örtüsü (tam ekran, en üstte) ----
        private void BuildTransition()
        {
            var go = new GameObject("Transition", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            transition = go.AddComponent<Image>();
            var bg = SpriteFactory.Hex("12111F"); bg.a = 0f;
            transition.color = bg;
            transition.raycastTarget = false;

            transitionLabel = CreateLabel(go.transform, "", 96, new Color(1f, 1f, 1f, 0f),
                TextAnchor.MiddleCenter);
            var trt = transitionLabel.rectTransform;
            trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(900, 200);
            trt.anchoredPosition = Vector2.zero;
            transitionLabel.fontStyle = FontStyle.Bold;
        }

        private Text CreateLabel(Transform parent, string content, int size, Color color, TextAnchor anchor)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.font = UIFont;
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.alignment = anchor;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            return txt;
        }

        /// <summary>Bölüm göstergesini güncelle (örn. "BÖLÜM 2/5").</summary>
        public void SetLevel(int index, int count)
        {
            if (levelLabel != null) levelLabel.text = "BÖLÜM " + (index + 1) + "/" + count;
        }

        /// <summary>Dokunmatik kontrolleri topluca göster/gizle (menü ↔ oyun geçişi).</summary>
        public void SetControlsActive(bool active)
        {
            if (_controlsRoot != null) _controlsRoot.SetActive(active);
        }

        private static readonly Vector2 AnchorBL = new Vector2(0f, 0f);
        private static readonly Vector2 AnchorBR = new Vector2(1f, 0f);

        private void CreateHoldButton(string name, Vector2 anchor, Vector2 anchoredPos,
            Vector2 size, float arrowRotation, System.Action onDown, System.Action onUp)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(_controlsRoot.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            // Yuvarlak yarı saydam buton tabanı
            var img = go.AddComponent<Image>();
            img.color = SpriteFactory.UiPanel;
            img.sprite = SpriteFactory.Circle;

            // Ok ikonu (üçgen sprite, döndürülmüş) — yazı yerine
            var arrowGo = new GameObject("Arrow", typeof(RectTransform));
            arrowGo.transform.SetParent(go.transform, false);
            var art = arrowGo.GetComponent<RectTransform>();
            art.anchorMin = art.anchorMax = new Vector2(0.5f, 0.5f);
            art.pivot = new Vector2(0.5f, 0.5f);
            art.sizeDelta = size * 0.42f;
            art.anchoredPosition = Vector2.zero;
            art.localRotation = Quaternion.Euler(0, 0, arrowRotation);
            var aimg = arrowGo.AddComponent<Image>();
            aimg.sprite = SpriteFactory.Triangle;
            aimg.color = new Color(0.85f, 0.85f, 0.92f, 0.92f);
            aimg.raycastTarget = false;

            // Basılı tut / bırak olayları
            var trigger = go.AddComponent<EventTrigger>();
            AddEvent(trigger, EventTriggerType.PointerDown, _ => onDown());
            AddEvent(trigger, EventTriggerType.PointerUp, _ => onUp());
            AddEvent(trigger, EventTriggerType.PointerExit, _ => onUp());
        }

        private static void AddEvent(EventTrigger trigger, EventTriggerType type,
            System.Action<BaseEventData> cb)
        {
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(data => cb(data));
            trigger.triggers.Add(entry);
        }

        private void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
