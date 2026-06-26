using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LevelDevilClone
{
    /// <summary>
    /// Sade ve profesyonel bir ana menü. Tema paletiyle uyumlu: koyu alacakaranlık
    /// zemin, net bir logo başlık ve dikey yığılmış yuvarlak butonlar (OYNA / ÇIKIŞ).
    /// Gereksiz hareket yoktur; yalnızca yumuşak bir açılış geçişi ve buton basış
    /// geri bildirimi. Oynat'a basınca menü solar ve verilen geri-çağrı tetiklenir.
    /// </summary>
    public class MainMenuBuilder : MonoBehaviour
    {
        private static readonly Vector2 C = new Vector2(0.5f, 0.5f);

        private Action _onPlay;
        private CanvasGroup _group;
        private bool _starting;
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

        public void Show(Action onPlay)
        {
            _onPlay = onPlay;
            Build();
            StartCoroutine(FadeIn());
        }

        private void Build()
        {
            // ---- Canvas ----
            var canvasGo = new GameObject("MenuCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();
            _group = canvasGo.AddComponent<CanvasGroup>();
            _group.alpha = 0f;

            var root = canvasGo.transform;

            // ---- Sade koyu zemin (canlı arka planı odak için bastırır) + ince vignette ----
            var dim = MakeImage("Dim", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var dimC = SpriteFactory.Hex("12111F"); dimC.a = 0.78f;
            dim.color = dimC;
            dim.raycastTarget = true; // arkadaki oyuna tık geçmesin

            var vig = MakeImage("Vignette", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            vig.sprite = SpriteFactory.Vignette;
            vig.color = new Color(1f, 1f, 1f, 0.5f);
            vig.raycastTarget = false;

            // ---- Logo başlık — ekranın ÜST kenarına sabit (her oranda tam görünür) ----
            var T = new Vector2(0.5f, 1f);   // üst-orta çapa

            var line1 = MakeText("KARANLIK", root, 80, FontStyle.Bold,
                new Color(SpriteFactory.UiText.r, SpriteFactory.UiText.g, SpriteFactory.UiText.b, 0.85f),
                TextAnchor.MiddleCenter);
            SetRect(line1.rectTransform, T, T, T, new Vector2(1040, 130), new Vector2(0, -200));

            var line2 = MakeText("TUZAK", root, 150, FontStyle.Bold,
                SpriteFactory.HazardBright, TextAnchor.MiddleCenter);
            SetRect(line2.rectTransform, T, T, T, new Vector2(1040, 200), new Vector2(0, -300));
            AddShadow(line2, new Color(0.40f, 0.14f, 0.11f, 0.65f), new Vector2(4, -6));

            var rule = MakeImage("Rule", root, T, T, Vector2.zero, Vector2.zero);
            SetRect(rule.rectTransform, T, T, T, new Vector2(300, 4), new Vector2(0, -505));
            rule.color = new Color(SpriteFactory.Hazard.r, SpriteFactory.Hazard.g, SpriteFactory.Hazard.b, 0.85f);
            rule.raycastTarget = false;

            var tagline = MakeText("5 BÖLÜM  ·  ÖLÜMCÜL TUZAKLAR", root, 34, FontStyle.Normal,
                new Color(SpriteFactory.UiText.r, SpriteFactory.UiText.g, SpriteFactory.UiText.b, 0.5f),
                TextAnchor.MiddleCenter);
            SetRect(tagline.rectTransform, T, T, T, new Vector2(960, 50), new Vector2(0, -548));

            // ---- Butonlar (dikey yığın, merkez) ----
            MakeButton("OYNA", root, new Vector2(0, -80), new Vector2(620, 150),
                SpriteFactory.ExitPanel, SpriteFactory.Hex("EAFBFB"), 60, true, StartPressed);

            MakeButton("ÇIKIŞ", root, new Vector2(0, -280), new Vector2(620, 132),
                new Color(0.16f, 0.16f, 0.22f, 0.92f),
                new Color(SpriteFactory.UiText.r, SpriteFactory.UiText.g, SpriteFactory.UiText.b, 0.85f),
                52, false, Quit);

            // ---- Sürüm etiketi (sağ alt, çok sönük) ----
            var ver = MakeText("v1.0", root, 30, FontStyle.Normal,
                new Color(1f, 1f, 1f, 0.25f), TextAnchor.LowerRight);
            SetRect(ver.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(200, 40), new Vector2(-40, 36));
        }

        private void MakeButton(string label, Transform parent, Vector2 pos, Vector2 size,
            Color fill, Color textColor, int fontSize, bool primary, Action onClick)
        {
            var rt = MakeRect("Btn_" + label, parent, C, C, C, size, pos);

            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteFactory.RoundedButton;
            img.type = Image.Type.Sliced;
            img.color = fill;

            if (primary)
                AddShadow(rt.gameObject, new Color(0f, 0f, 0f, 0.35f), new Vector2(0, -6));

            var txt = MakeText(label, rt, fontSize, FontStyle.Bold, textColor, TextAnchor.MiddleCenter);
            SetRect(txt.rectTransform, Vector2.zero, Vector2.one, C, Vector2.zero, Vector2.zero);
            txt.raycastTarget = false;

            // Basış geri bildirimi + tıklama
            AddTrigger(rt.gameObject, EventTriggerType.PointerDown, _ => rt.localScale = Vector3.one * 0.96f);
            AddTrigger(rt.gameObject, EventTriggerType.PointerUp, _ => rt.localScale = Vector3.one);
            AddTrigger(rt.gameObject, EventTriggerType.PointerExit, _ => rt.localScale = Vector3.one);
            AddTrigger(rt.gameObject, EventTriggerType.PointerClick, _ =>
            {
                rt.localScale = Vector3.one;
                AudioManager.PlayClick();
                onClick();
            });
        }

        private void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void StartPressed()
        {
            if (_starting) return;
            _starting = true;
            AudioManager.PlayWin();
            StartCoroutine(StartRoutine());
        }

        private IEnumerator FadeIn()
        {
            float d = 0.4f, t = 0f;
            while (t < d)
            {
                t += Time.deltaTime;
                _group.alpha = Mathf.Clamp01(t / d);
                yield return null;
            }
            _group.alpha = 1f;
        }

        private IEnumerator StartRoutine()
        {
            float d = 0.35f, t = 0f;
            while (t < d)
            {
                t += Time.deltaTime;
                _group.alpha = 1f - t / d;
                yield return null;
            }
            _onPlay?.Invoke();
            Destroy(gameObject);
        }

        // ---- uGUI yardımcıları ----
        private RectTransform MakeRect(string name, Transform parent, Vector2 aMin, Vector2 aMax,
            Vector2 pivot, Vector2 size, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            SetRect(go.GetComponent<RectTransform>(), aMin, aMax, pivot, size, pos);
            return go.GetComponent<RectTransform>();
        }

        private static void SetRect(RectTransform rt, Vector2 aMin, Vector2 aMax,
            Vector2 pivot, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
        }

        private Image MakeImage(string name, Transform parent, Vector2 aMin, Vector2 aMax,
            Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = aMin; rt.anchorMax = aMax;
            if (aMin == Vector2.zero && aMax == Vector2.one)
            {
                rt.offsetMin = offMin; rt.offsetMax = offMax;
            }
            return go.AddComponent<Image>();
        }

        private Text MakeText(string content, Transform parent, int size, FontStyle style,
            Color color, TextAnchor anchor)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            txt.font = UIFont;
            txt.text = content;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = anchor;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.raycastTarget = false;
            return txt;
        }

        private static void AddShadow(GameObject go, Color color, Vector2 dist)
        {
            var sh = go.AddComponent<Shadow>();
            sh.effectColor = color;
            sh.effectDistance = dist;
        }

        private static void AddShadow(Text txt, Color color, Vector2 dist)
        {
            AddShadow(txt.gameObject, color, dist);
        }

        private static void AddTrigger(GameObject go, EventTriggerType type, Action<BaseEventData> cb)
        {
            var trigger = go.GetComponent<EventTrigger>();
            if (trigger == null) trigger = go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(data => cb(data));
            trigger.triggers.Add(entry);
        }
    }
}
