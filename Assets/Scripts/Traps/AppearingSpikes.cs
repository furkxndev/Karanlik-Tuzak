using System.Collections;
using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// LEVEL 2 TROLL: Görünmez/pasif dikenler. Oyuncu tetik bölgesine girince
    /// (platforma atlayınca) aniden yukarı fırlar ve ölümcül olur.
    /// </summary>
    public class AppearingSpikes : MonoBehaviour
    {
        public Transform spikesRoot;       // Yukarı çıkacak diken grubu
        public SpriteRenderer[] renderers; // Görünürlüğü kontrol için
        public Hazard[] hazards;           // Aktiflik kontrolü için
        public float hiddenY;
        public float shownY;
        public float popSpeed = 22f;

        private bool _triggered;

        public void Init()
        {
            SetVisible(false);
            foreach (var h in hazards) if (h) h.active = false;
            if (spikesRoot) spikesRoot.localPosition =
                new Vector3(spikesRoot.localPosition.x, hiddenY, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (other.GetComponent<PlayerController>() == null) return;
            _triggered = true;
            StartCoroutine(Pop());
        }

        private IEnumerator Pop()
        {
            AudioManager.PlaySpike();
            SetVisible(true);
            foreach (var h in hazards) if (h) h.active = true;

            while (spikesRoot != null &&
                   spikesRoot.localPosition.y < shownY - 0.01f)
            {
                var p = spikesRoot.localPosition;
                p.y = Mathf.MoveTowards(p.y, shownY, popSpeed * Time.deltaTime);
                spikesRoot.localPosition = p;
                yield return null;
            }
        }

        private void SetVisible(bool v)
        {
            foreach (var r in renderers) if (r) r.enabled = v;
        }
    }
}
