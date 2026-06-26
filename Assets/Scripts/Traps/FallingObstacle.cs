using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// LEVEL 1 TROLL: Belirli bir x eşiğine gelindiğinde yukarıdan ölümcül bir
    /// blok düşürür. Oyuncu çizgiyi geçen bir tetik bölgesine değince serbest kalır.
    /// </summary>
    public class FallingObstacle : MonoBehaviour
    {
        public Rigidbody2D block;     // Düşecek blok
        public float fallGravity = 4f;
        private bool _released;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_released) return;
            if (other.GetComponent<PlayerController>() == null) return;

            _released = true;
            AudioManager.PlayDrop();
            if (block != null)
            {
                block.bodyType = RigidbodyType2D.Dynamic;
                block.gravityScale = fallGravity;
            }
        }
    }
}
