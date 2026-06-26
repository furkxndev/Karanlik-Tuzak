using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Bir SpriteRenderer'ın alfasını (ve isteğe bağlı ölçeğini) nabız gibi
    /// dalgalandırır. Tehlike şeritleri, parıltılar ve uyarı işaretleri için.
    /// </summary>
    public class Pulse : MonoBehaviour
    {
        public float speed = 3.5f;
        public float minAlpha = 0.35f;
        public float maxAlpha = 1f;
        public float scaleAmount = 0f;   // 0 = ölçek sabit
        public float phase = 0f;

        private SpriteRenderer _sr;
        private Color _base;
        private Vector3 _baseScale;

        private void Start()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _base = _sr.color;
            _baseScale = transform.localScale;
        }

        private void Update()
        {
            float t = Mathf.Sin(Time.time * speed + phase) * 0.5f + 0.5f;
            if (_sr != null)
            {
                var c = _base;
                c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
                _sr.color = c;
            }
            if (scaleAmount > 0f)
                transform.localScale = _baseScale * (1f + scaleAmount * (t - 0.5f));
        }
    }
}
