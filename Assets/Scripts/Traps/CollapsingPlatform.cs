using System.Collections;
using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// LEVEL 1 TROLL: Sağlam görünen zemin. Oyuncu üstüne basınca kısa bir
    /// titremeden sonra çöker (collider kapanır, parça aşağı düşer ve solar).
    /// Çıkış kapısının hemen önüne konur ki oyuncu "kazandım" derken düşsün.
    /// </summary>
    public class CollapsingPlatform : MonoBehaviour
    {
        public float shakeTime = 0.35f;
        public float shakeAmount = 0.06f;

        private SpriteRenderer _sr;
        private Collider2D _col;
        private Rigidbody2D _rb;
        private bool _triggered;
        private Vector3 _origin;

        public void Setup(SpriteRenderer sr, Collider2D col)
        {
            _sr = sr;
            _col = col;
            _origin = transform.position;
            _rb = gameObject.GetComponent<Rigidbody2D>();
            if (_rb == null) _rb = gameObject.AddComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Static;
        }

        private void OnCollisionEnter2D(Collision2D c)
        {
            if (_triggered) return;
            var player = c.collider.GetComponent<PlayerController>();
            if (player == null) return;

            // Sadece üstüne basıldıysa tetikle.
            foreach (var contact in c.contacts)
                if (contact.normal.y < -0.5f) { StartCoroutine(Collapse()); return; }
        }

        private IEnumerator Collapse()
        {
            _triggered = true;
            float t = 0f;
            while (t < shakeTime)
            {
                t += Time.deltaTime;
                float ox = Mathf.Sin(t * 60f) * shakeAmount;
                transform.position = _origin + new Vector3(ox, 0f, 0f);
                yield return null;
            }

            // Çök: collider kapanır, parça serbest düşer.
            AudioManager.PlayCollapse();
            _col.enabled = false;
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.gravityScale = 3.5f;
            _rb.AddTorque(Random.Range(-15f, 15f));

            var renderers = GetComponentsInChildren<SpriteRenderer>();
            float fade = 0f;
            while (fade < 1f)
            {
                fade += Time.deltaTime * 1.2f;
                float a = Mathf.Lerp(1f, 0f, fade);
                foreach (var r in renderers)
                {
                    var c = r.color; c.a = a; r.color = c;
                }
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
