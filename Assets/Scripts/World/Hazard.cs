using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Oyuncuya değdiğinde onu öldüren her şey: dikenler, düşen engeller,
    /// ölüm bölgesi (haritanın altı) vb. Hem trigger hem normal çarpışmayı dinler.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        public bool active = true;

        private void OnTriggerEnter2D(Collider2D other) => TryKill(other);
        private void OnCollisionEnter2D(Collision2D c) => TryKill(c.collider);

        private void TryKill(Collider2D other)
        {
            if (!active) return;
            if (other.GetComponent<PlayerController>() != null)
                GameManager.Instance.KillPlayer();
        }
    }
}
