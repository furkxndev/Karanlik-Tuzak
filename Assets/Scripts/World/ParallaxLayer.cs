using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Katmanı kameranın yatay hareketine göre kaydırır. factor=1 -> kamerayla
    /// birlikte hareket eder (çok uzak), factor=0 -> dünyada sabit kalır (yakın).
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        public Transform cam;
        public float factor = 0.5f;

        private float _startX;
        private float _y;
        private float _z;

        private void Start()
        {
            _startX = transform.position.x;
            _y = transform.position.y;
            _z = transform.position.z;
        }

        private void LateUpdate()
        {
            if (cam == null) return;
            transform.position = new Vector3(_startX + cam.position.x * factor, _y, _z);
        }
    }
}
