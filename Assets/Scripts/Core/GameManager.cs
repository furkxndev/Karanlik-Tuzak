using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LevelDevilClone
{
    /// <summary>
    /// Oyunun merkezi: kamerayı, karakteri, dokunmatik arayüzü ve seviye akışını
    /// kurar/yönetir. Ölüm ve seviye geçişlerini idare eder.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private InputState _input;
        private LevelManager _levels;
        private TouchUIBuilder _ui;
        private PlayerController _player;
        private CameraFollow _cam;
        private Camera _camera;

        private MainMenuBuilder _menu;
        private bool _started;    // Ana menüden oyuna geçildi mi

        private int _currentLevel;
        private LevelData _data;
        private bool _busy;       // Ölüm/geçiş animasyonu sırasında kilit
        private bool _won;

        public void Initialize()
        {
            Instance = this;
            _input = new InputState();
            _levels = new LevelManager();

            SetupCamera();

            var audioGo = new GameObject("AudioManager");
            audioGo.AddComponent<AudioManager>().Init(_camera);

            BuildBackground();
            CreatePlayer();

            _ui = new TouchUIBuilder(_input);
            _ui.onMenuPressed = ReturnToMenu;
            _ui.Build();

            // Doğrudan oyuna girmek yerine önce ana menü.
            ShowMainMenu();
        }

        // ---- Ana menü ----
        private void ShowMainMenu()
        {
            _started = false;
            // Karakteri ve kontrolleri menüde gizle; canlı atmosferik arka plan görünür kalır.
            if (_player != null) _player.gameObject.SetActive(false);
            _ui.SetControlsActive(false);

            var menuGo = new GameObject("MainMenu");
            _menu = menuGo.AddComponent<MainMenuBuilder>();
            _menu.Show(StartGame);
        }

        /// <summary>Oyun içi üst MENÜ butonundan çağrılır: oyunu durdurup ana menüye dön.</summary>
        public void ReturnToMenu()
        {
            if (_menu != null) return;   // zaten menüdeyiz
            StopAllCoroutines();
            _busy = false;

            // Geçiş/ölüm flaşı kalıntılarını temizle.
            if (_ui != null)
            {
                if (_ui.flash != null) _ui.flash.color = new Color(0.55f, 0.07f, 0.07f, 0f);
                if (_ui.transition != null)
                {
                    var c = _ui.transition.color; c.a = 0f; _ui.transition.color = c;
                    _ui.transition.raycastTarget = false;
                }
                if (_ui.transitionLabel != null)
                {
                    var c = _ui.transitionLabel.color; c.a = 0f; _ui.transitionLabel.color = c;
                }
            }

            if (_player != null)
            {
                _player.ControlEnabled = false;
                _player.Body.velocity = Vector2.zero;
                _player.Body.simulated = true;
            }

            ShowMainMenu();
        }

        /// <summary>Ana menüdeki Oynat butonu tarafından çağrılır.</summary>
        public void StartGame()
        {
            if (_started) return;
            _started = true;
            _menu = null;
            if (_player != null) _player.gameObject.SetActive(true);
            _ui.SetControlsActive(true);
            LoadLevel(0);
        }

        private void Update()
        {
            if (_input != null) _input.PollKeyboard();

            if (!_started) return;   // Menü açıkken oyun girdisi yok

            // Hızlı yeniden başlat (editör/klavye) — R
            if (Input.GetKeyDown(KeyCode.R) && !_busy)
                RespawnNow();
        }

        // ---- Kurulum ----
        private void SetupCamera()
        {
            _camera = Camera.main;
            if (_camera == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                _camera = go.AddComponent<Camera>();
            }
            _camera.orthographic = true;
            _camera.orthographicSize = 6f;
            _camera.backgroundColor = SpriteFactory.Background;
            _camera.clearFlags = CameraClearFlags.SolidColor;
            _camera.transform.position = new Vector3(0, 3, -10);

            _cam = _camera.GetComponent<CameraFollow>();
            if (_cam == null) _cam = _camera.gameObject.AddComponent<CameraFollow>();
        }

        private void BuildBackground()
        {
            var bgGo = new GameObject("Background");
            var bg = bgGo.AddComponent<BackgroundBuilder>();
            bg.Build(_camera);
        }

        private void CreatePlayer()
        {
            var go = new GameObject("Player");
            go.transform.position = new Vector3(0, 2, 0);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3.5f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.62f, 0.96f);
            // Köşeleri hafif yuvarla: bitişik zemin bloklarının dikişlerine takılmayı önler.
            col.edgeRadius = 0.03f;
            // Sürtünmesiz materyal: duvarlara yapışıp takılı kalmayı önler.
            var mat = new PhysicsMaterial2D("PlayerMat") { friction = 0f, bounciness = 0f };
            col.sharedMaterial = mat;

            _player = go.AddComponent<PlayerController>();
            _player.jumpForce = 16.5f;
            _player.moveSpeed = 8.5f;
            _player.coyoteTime = 0.12f;
            _player.jumpBufferTime = 0.12f;
            _player.Init(_input);

            // Level Devil tarzı çok parçalı karakter (kafa+gövde, kollar/eller, bacaklar).
            var anim = CharacterBuilder.Build(go.transform);
            anim.player = _player;
            _player.SetVisual(anim.transform);
        }

        // ---- Seviye akışı ----
        private void LoadLevel(int index)
        {
            _currentLevel = index;
            _data = _levels.Build(index, _player.transform);
            _ui.SetLevel(index, _levels.Count);

            // Karakteri başlangıca yerleştir.
            _player.transform.position = _data.spawn;
            _player.Body.velocity = Vector2.zero;
            _player.ResetGravity();
            _player.gravityFlipMode = _data.gravityFlip;
            _player.invertControls = _data.invertControls;
            _player.ControlEnabled = true;

            // Kamera sınırları. Yatay takip eder, dikeyde seviye merkezine sabitlenir
            // (seviyeler kameradan kısa olduğu için dikey kayma istenmez).
            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
            _cam.minX = _data.minX + halfW;
            _cam.maxX = _data.maxX - halfW;
            if (_cam.maxX < _cam.minX)
                _cam.maxX = _cam.minX = (_data.minX + _data.maxX) * 0.5f;

            float centerY = (_data.minY + _data.maxY) * 0.5f;
            _cam.minY = _cam.maxY = centerY;
            _cam.yOffset = 0f;
            _cam.SnapTo(_player.transform);
        }

        public void KillPlayer()
        {
            if (_busy || _won) return;
            StartCoroutine(DeathRoutine());
        }

        private IEnumerator DeathRoutine()
        {
            _busy = true;
            AudioManager.PlayDeath();
            Fx.Impact(_player.transform.position, SpriteFactory.HazardBright, 16);
            Fx.Debris(_player.transform.position, SpriteFactory.CharBody, 8);
            CameraFollow.Shake(0.28f, 0.32f);
            _player.ControlEnabled = false;
            _player.Body.velocity = Vector2.zero;
            _player.Body.simulated = false;

            // Kırmızı ölüm flaşı.
            yield return FlashColor(new Color(0.7f, 0.1f, 0.1f), 0.65f, 0.28f);

            RebuildCurrent();
            _player.Body.simulated = true;
            _busy = false;
        }

        private void RebuildCurrent()
        {
            LoadLevel(_currentLevel);
        }

        private void RespawnNow()
        {
            _player.Body.velocity = Vector2.zero;
            RebuildCurrent();
        }

        public void LevelComplete()
        {
            if (_busy || _won) return;
            StartCoroutine(NextLevelRoutine());
        }

        private IEnumerator NextLevelRoutine()
        {
            _busy = true;
            AudioManager.PlayWin();
            _player.ControlEnabled = false;
            _player.Body.velocity = Vector2.zero;

            bool finale = _currentLevel + 1 >= _levels.Count;
            int next = finale ? 0 : _currentLevel + 1;

            // Bölümler arası geçiş animasyonu.
            yield return TransitionRoutine(next);

            _busy = false;
        }

        /// <summary>Ekranı örter, sıradaki bölümün adını gösterir, yeni bölümü yükler,
        /// sonra örtüyü açar — akıcı bir bölüm geçişi.</summary>
        private IEnumerator TransitionRoutine(int nextIndex)
        {
            if (_ui?.transition == null)
            {
                LoadLevel(nextIndex);
                yield break;
            }

            // Örtüyü kapat.
            _ui.transition.raycastTarget = true;
            yield return FadeImage(_ui.transition, 0f, 1f, 0.35f);

            // Bölüm adını belirir.
            if (_ui.transitionLabel != null)
            {
                _ui.transitionLabel.text = "BÖLÜM " + (nextIndex + 1);
                yield return FadeText(_ui.transitionLabel, 0f, 1f, 0.22f);
            }
            yield return new WaitForSeconds(0.45f);

            // Bölümü değiştir (örtü kapalıyken).
            LoadLevel(nextIndex);
            yield return new WaitForSeconds(0.12f);

            // Etiketi soldur + örtüyü aç.
            if (_ui.transitionLabel != null)
                StartCoroutine(FadeText(_ui.transitionLabel, 1f, 0f, 0.3f));
            yield return FadeImage(_ui.transition, 1f, 0f, 0.4f);
            _ui.transition.raycastTarget = false;
        }

        private IEnumerator FadeImage(Image img, float a0, float a1, float dur)
        {
            float t = 0f;
            Color c = img.color;
            while (t < dur)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(a0, a1, t / dur);
                img.color = c;
                yield return null;
            }
            c.a = a1; img.color = c;
        }

        private IEnumerator FadeText(Text txt, float a0, float a1, float dur)
        {
            float t = 0f;
            Color c = txt.color;
            while (t < dur)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(a0, a1, t / dur);
                txt.color = c;
                yield return null;
            }
            c.a = a1; txt.color = c;
        }

        // ---- Flaş yardımcıları ----
        private IEnumerator FlashColor(Color rgb, float peak, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(peak, 0f, t / duration);
                SetFlash(rgb, a);
                yield return null;
            }
            SetFlash(rgb, 0f);
        }

        private void SetFlash(Color rgb, float a)
        {
            if (_ui?.flash == null) return;
            _ui.flash.color = new Color(rgb.r, rgb.g, rgb.b, a);
        }
    }
}
