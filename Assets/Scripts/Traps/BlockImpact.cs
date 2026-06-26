using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Düşen ölümcül blok bir yüzeye çarptığında toz, kıvılcım, ses ve kamera
    /// sarsıntısı üretir (ilk çarpışmada bir kez). Oyuncuya çarpma Hazard'a bırakılır.
    /// </summary>
    public class BlockImpact : MonoBehaviour
    {
        private bool _done;

        private void OnCollisionEnter2D(Collision2D c)
        {
            if (_done) return;
            if (c.collider.GetComponent<PlayerController>() != null) return;

            _done = true;
            Vector2 p = transform.position;
            Fx.Dust(p, 1.3f);
            Fx.Impact(p, SpriteFactory.HazardBright, 12);
            AudioManager.PlayCrusher();

            bool onScreen = CameraFollow.Active == null ||
                Mathf.Abs(p.x - CameraFollow.Active.transform.position.x) < 12f;
            if (onScreen) CameraFollow.Shake(0.15f, 0.22f);
        }
    }
}
