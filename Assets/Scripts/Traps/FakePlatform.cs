using System.Collections;
using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// LEVEL 2 TROLL: Gerçek platform gibi görünür ama oyuncu üstüne basınca
    /// kısa bir gecikmeyle collider kapanır; oyuncu içinden geçip düşer ve parça solar.
    /// </summary>
    public class FakePlatform : MonoBehaviour
    {
        public float delay = 0.12f;
        public bool restoreAfter = false;  // İstenirse bir süre sonra geri gelir
        public float restoreTime = 1.5f;

        private SpriteRenderer _sr;
        private Collider2D _col;
        private bool _triggered;

        public void Setup(SpriteRenderer sr, Collider2D col)
        {
            _sr = sr;
            _col = col;
        }

        private void OnCollisionEnter2D(Collision2D c)
        {
            if (_triggered) return;
            if (c.collider.GetComponent<PlayerController>() == null) return;
            foreach (var contact in c.contacts)
                if (contact.normal.y < -0.5f) { StartCoroutine(Drop()); return; }
        }

        private IEnumerator Drop()
        {
            _triggered = true;
            yield return new WaitForSeconds(delay);

            AudioManager.PlayDrop();
            _col.enabled = false;
            // Tüm görsel parçaları (taban + gövde + pervaz) birlikte solar.
            var renderers = GetComponentsInChildren<SpriteRenderer>();
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                float a = Mathf.Lerp(1f, 0.12f, t);
                foreach (var r in renderers)
                {
                    var c = r.color; c.a = a; r.color = c;
                }
                yield return null;
            }

            if (restoreAfter)
            {
                yield return new WaitForSeconds(restoreTime);
                _col.enabled = true;
                foreach (var r in renderers)
                {
                    var c = r.color; c.a = 1f; r.color = c;
                }
                _triggered = false;
            }
        }
    }
}
