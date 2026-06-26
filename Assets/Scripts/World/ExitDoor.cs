using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Seviye çıkışı. Oyuncu değdiğinde bölüm tamamlanır. Hafif yanıp sönen
    /// kasvetli bir parıltıya sahiptir.
    /// </summary>
    public class ExitDoor : MonoBehaviour
    {
        public bool armed = true;        // FleeingDoor gibi mekanikler geçici olarak kapatabilir
        private SpriteRenderer _glow;

        public void Setup(SpriteRenderer glow)
        {
            _glow = glow;
        }

        private void Update()
        {
            if (_glow != null)
            {
                float t = 0.45f + 0.25f * Mathf.Sin(Time.time * 2.2f);
                var c = SpriteFactory.ExitGlow;
                _glow.color = new Color(c.r, c.g, c.b, t);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!armed) return;
            if (other.GetComponent<PlayerController>() != null)
                GameManager.Instance.LevelComplete();
        }
    }
}
