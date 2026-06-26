using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Sade ama akıcı karakter. Sadece Rigidbody2D + BoxCollider2D kullanır.
    /// "Level Devil" hissi için: coyote time, jump buffer, değişken zıplama
    /// yüksekliği ve hızlı düşüş. Level 3 için yerçekimini ters çevirme modu var.
    /// Zemin kontrolü çarpışma normalleriyle yapılır; bu sayede hiçbir Layer
    /// ayarı yapmaya gerek kalmaz (kur-çalıştır).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Hareket")]
        public float moveSpeed = 7.5f;
        public float acceleration = 90f;
        public float deceleration = 110f;

        [Header("Zıplama")]
        public float jumpForce = 15.5f;
        public float coyoteTime = 0.10f;       // Platformdan ayrıldıktan sonra hala zıplayabilme süresi
        public float jumpBufferTime = 0.10f;   // Yere değmeden önce basılan zıplamayı hatırlama
        public float lowJumpMultiplier = 3.0f; // Butonu erken bırakınca kısa zıplama
        public float fallMultiplier = 2.6f;    // Daha tatmin edici/ağır düşüş

        [Header("Yerçekimi Tersleme (Level 3)")]
        public bool gravityFlipMode = false;   // True ise zıplama butonu yerçekimini ters çevirir

        [Header("Ters Kontroller (Level 5)")]
        public bool invertControls = false;    // True ise sol/sağ yer değiştirir

        private Rigidbody2D _rb;
        private BoxCollider2D _col;
        private Transform _visual;
        private Vector3 _visualBaseScale = Vector3.one;
        private int _facing = 1;
        private InputState _input;

        private bool _grounded;
        private bool _wasGrounded;
        private float _prevVelY;
        private float _coyoteCounter;
        private float _jumpBufferCounter;
        private int _gravityDir = -1;          // -1 = aşağı (normal), +1 = yukarı (ters)
        private bool _controlEnabled = true;
        private float _baseGravityScale;

        public bool ControlEnabled { get => _controlEnabled; set => _controlEnabled = value; }
        public Rigidbody2D Body => _rb;

        // Animator/diğer sistemler için durum.
        public Vector2 Velocity => _rb != null ? _rb.velocity : Vector2.zero;
        public bool IsGrounded => _grounded;
        public int GravityDir => _gravityDir;

        public void Init(InputState input)
        {
            _input = input;
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _baseGravityScale = _rb.gravityScale;
        }

        /// <summary>Görsel kök (karakter rig'i) atanır; çevirme bunun üstünden yapılır.</summary>
        public void SetVisual(Transform v)
        {
            _visual = v;
            if (v != null) _visualBaseScale = v.localScale;
            ApplyVisualFlip();
        }

        private void ApplyVisualFlip()
        {
            if (_visual == null) return;
            _visual.localScale = new Vector3(
                _visualBaseScale.x * _facing,
                _visualBaseScale.y * (_gravityDir < 0 ? 1f : -1f),
                _visualBaseScale.z);
        }

        private void Update()
        {
            if (_input == null) return;

            // Jump buffer'ı doldur.
            if (_input.ConsumeJump())
                _jumpBufferCounter = jumpBufferTime;
            else
                _jumpBufferCounter -= Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (_input == null || !_controlEnabled)
            {
                if (_rb != null) _rb.velocity = new Vector2(0f, _rb.velocity.y);
                return;
            }

            HandleHorizontal();
            HandleJump();
            HandleBetterGravity();
            HandleLandingSound();
        }

        private void HandleLandingSound()
        {
            // Havadan zemine yeni geçişte ve yeterince hızlı düşerken iniş sesi + toz.
            if (_grounded && !_wasGrounded && Mathf.Abs(_prevVelY) > 3f)
            {
                AudioManager.PlayLand();
                Fx.Dust((Vector2)transform.position + new Vector2(0f, _gravityDir * 0.5f), 0.7f);
            }
            _wasGrounded = _grounded;
            _prevVelY = _rb.velocity.y;
        }

        private void HandleHorizontal()
        {
            float dir = (_input.Right ? 1f : 0f) - (_input.Left ? 1f : 0f);
            if (invertControls) dir = -dir;
            float targetSpeed = dir * moveSpeed;
            float diff = targetSpeed - _rb.velocity.x;
            float rate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
            float movement = Mathf.Clamp(diff * rate * Time.fixedDeltaTime,
                -Mathf.Abs(diff), Mathf.Abs(diff));
            _rb.velocity = new Vector2(_rb.velocity.x + movement, _rb.velocity.y);

            // Bakış yönü (tüm karakteri çevirir).
            if (dir != 0f)
            {
                int newFacing = dir < 0f ? -1 : 1;
                if (newFacing != _facing) { _facing = newFacing; ApplyVisualFlip(); }
            }
        }

        private void HandleJump()
        {
            _coyoteCounter = _grounded ? coyoteTime : _coyoteCounter - Time.fixedDeltaTime;

            bool wantsJump = _jumpBufferCounter > 0f;

            if (gravityFlipMode)
            {
                // LEVEL 3 TERS KÖŞE: Zıplama yerine yerçekimini ters çevirir.
                // Ama sadece bir yüzeye yapışıkken (yerden) tetiklenebilir.
                if (wantsJump && _coyoteCounter > 0f)
                {
                    FlipGravity();
                    _jumpBufferCounter = 0f;
                    _coyoteCounter = 0f;
                }
                return;
            }

            // Normal zıplama.
            if (wantsJump && _coyoteCounter > 0f)
            {
                // Yerçekimi yönünün tersine fırla.
                _rb.velocity = new Vector2(_rb.velocity.x, -_gravityDir * jumpForce);
                _jumpBufferCounter = 0f;
                _coyoteCounter = 0f;
                AudioManager.PlayJump();
            }
        }

        private void HandleBetterGravity()
        {
            float vy = _rb.velocity.y;
            // "Aşağı" hareket = yerçekimi yönünde hareket.
            bool fallingDown = (_gravityDir < 0 && vy < 0f) || (_gravityDir > 0 && vy > 0f);
            bool risingUp    = (_gravityDir < 0 && vy > 0f) || (_gravityDir > 0 && vy < 0f);

            if (fallingDown)
            {
                _rb.velocity += (Vector2)(Vector2.up * _gravityDir) *
                    Mathf.Abs(Physics2D.gravity.y) * _baseGravityScale *
                    (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }
            else if (risingUp && !_input.JumpHeld && !gravityFlipMode)
            {
                _rb.velocity += (Vector2)(Vector2.up * _gravityDir) *
                    Mathf.Abs(Physics2D.gravity.y) * _baseGravityScale *
                    (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
            }
        }

        private void FlipGravity()
        {
            _gravityDir = -_gravityDir;
            _rb.gravityScale = _baseGravityScale * (-_gravityDir); // gravityDir=-1 -> +scale
            // Görseli ters çevir ki kafa üstü dururken doğal görünsün.
            ApplyVisualFlip();
            // Yapışmayı koparmak için YENİ yerçekimi yönünde küçük bir itki ver.
            // Yerçekimi yönü = (0, _gravityDir): -1 aşağı, +1 yukarı.
            _rb.velocity = new Vector2(_rb.velocity.x, _gravityDir * jumpForce * 0.5f);
            AudioManager.PlayFlip();
        }

        public void ResetGravity()
        {
            _gravityDir = -1;
            _rb.gravityScale = _baseGravityScale;
            ApplyVisualFlip();
        }

        // ---- Zemin kontrolü: çarpışma normallerinden ----
        private void OnCollisionStay2D(Collision2D c) => EvaluateGround(c);
        private void OnCollisionEnter2D(Collision2D c) => EvaluateGround(c);
        private void OnCollisionExit2D(Collision2D c) => _grounded = false;

        private void EvaluateGround(Collision2D c)
        {
            for (int i = 0; i < c.contactCount; i++)
            {
                Vector2 n = c.GetContact(i).normal;
                // Normal, yerçekiminin tersine bakıyorsa zemindeyiz.
                if (_gravityDir < 0 && n.y > 0.5f) { _grounded = true; return; }
                if (_gravityDir > 0 && n.y < -0.5f) { _grounded = true; return; }
            }
        }
    }
}
