using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Oyuncuyu yumuşakça takip eden kamera. Seviye sınırları içinde kalır.
    /// Impact anlarında sarsılır (CameraFollow.Active.Shake ile her yerden çağrılır).
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Active;

        public Transform target;
        public float smooth = 8f;
        public float yOffset = 1.5f;

        public float minX, maxX, minY, maxY;
        public bool clamp = true;

        private float _shakeAmt;
        private float _shakeTimer;
        private float _shakeDur;
        private float _seed;

        private void Awake()
        {
            Active = this;
            _seed = Mathf.Repeat(transform.position.x * 0.137f, 100f);
        }

        public static void Shake(float amount, float duration)
        {
            if (Active != null) Active.DoShake(amount, duration);
        }

        private void DoShake(float amount, float duration)
        {
            // En güçlü sarsıntı kazanır; süre sıfırlanır.
            _shakeAmt = Mathf.Max(_shakeAmt, amount);
            _shakeDur = Mathf.Max(_shakeDur, duration);
            _shakeTimer = _shakeDur;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x,
                target.position.y + yOffset, transform.position.z);

            if (clamp)
            {
                desired.x = Mathf.Clamp(desired.x, minX, maxX);
                desired.y = Mathf.Clamp(desired.y, minY, maxY);
            }

            Vector3 pos = Vector3.Lerp(transform.position, desired,
                1f - Mathf.Exp(-smooth * Time.deltaTime));

            // Sarsıntı ofseti (yumuşakça söner).
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float k = _shakeDur > 0f ? _shakeTimer / _shakeDur : 0f;
                float mag = _shakeAmt * k * k;
                float tt = Time.time * 40f + _seed;
                pos.x += (Mathf.PerlinNoise(tt, 0f) - 0.5f) * 2f * mag;
                pos.y += (Mathf.PerlinNoise(0f, tt) - 0.5f) * 2f * mag;
                if (_shakeTimer <= 0f) { _shakeAmt = 0f; _shakeDur = 0f; }
            }

            transform.position = pos;
        }

        public void SnapTo(Transform t)
        {
            target = t;
            if (t == null) return;
            Vector3 p = new Vector3(t.position.x, t.position.y + yOffset, transform.position.z);
            if (clamp)
            {
                p.x = Mathf.Clamp(p.x, minX, maxX);
                p.y = Mathf.Clamp(p.y, minY, maxY);
            }
            transform.position = p;
        }
    }
}
