using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// LEVEL 4 TROLL: Tavandan ritmik olarak inip kalkan ezici piston. Hızlı
    /// slam ile aşağı iner, bir an bekler, yavaşça geri çekilir. Dokunmak ölümcül
    /// (Hazard ile birlikte kullanılır). Her piston farklı faz ile çalışır;
    /// oyuncu doğru zamanda altından geçmelidir.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Crusher : MonoBehaviour
    {
        public float topY = 5f;
        public float bottomY = 2.1f;
        public float period = 2.2f;
        public float phase = 0f;

        private Rigidbody2D _rb;
        private float _x;
        private bool _slammed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _x = transform.position.x;
        }

        private void FixedUpdate()
        {
            float p = Mathf.Repeat((Time.time + phase) / period, 1f);
            _rb.MovePosition(new Vector2(_x, Evaluate(p)));

            // Dibe vurduğu an (cycle başına bir kez): ses + toz + kamera sarsıntısı.
            const float slam = 0.12f;
            if (!_slammed && p >= slam && p < slam + 0.06f)
            {
                _slammed = true;
                AudioManager.PlayCrusher();

                // Sadece ekrana yakın ezicilerde görsel efekt (spam önleme).
                bool onScreen = CameraFollow.Active == null ||
                    Mathf.Abs(_x - CameraFollow.Active.transform.position.x) < 12f;
                if (onScreen)
                {
                    Vector2 impact = new Vector2(_x, bottomY - 2f + 0.15f);
                    Fx.Dust(impact, 1.3f);
                    Fx.Impact(impact, SpriteFactory.HazardBright, 8);
                    CameraFollow.Shake(0.16f, 0.22f);
                }
            }
            if (p > 0.85f) _slammed = false;
        }

        // Faz eğrisi: ani iniş -> dipte bekle -> yumuşak çıkış -> tepede bekle.
        private float Evaluate(float p)
        {
            const float slam = 0.12f;   // hızlı iniş
            const float hold = 0.28f;   // dipte bekleme
            const float rise = 0.28f;   // çıkış
            if (p < slam)
            {
                float k = p / slam;
                return Mathf.Lerp(topY, bottomY, k * k); // ease-in (sert vuruş)
            }
            if (p < slam + hold) return bottomY;
            if (p < slam + hold + rise)
            {
                float k = (p - slam - hold) / rise;
                return Mathf.Lerp(bottomY, topY, Mathf.SmoothStep(0f, 1f, k));
            }
            return topY;
        }
    }
}
