using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// LEVEL 3 TROLL: Çıkış kapısı oyuncudan kaçar. Oyuncu belirli mesafeye
    /// yaklaşınca kapı önceden tanımlı bir sonraki noktaya ışınlanır/kayar.
    /// Belirli sayıda kaçtıktan sonra son noktada durur ki bölüm bitirilebilsin.
    /// </summary>
    public class FleeingDoor : MonoBehaviour
    {
        public ExitDoor door;
        public Transform[] stops;        // Kaçış noktaları (son nokta = ulaşılabilir)
        public float triggerRange = 3.0f;
        public float moveSpeed = 14f;

        private int _index;
        private Transform _player;
        private Vector3 _targetPos;
        private bool _settled;

        public void Init(Transform player)
        {
            _player = player;
            _index = 0;
            if (stops != null && stops.Length > 0)
            {
                _targetPos = stops[0].position;
                transform.position = _targetPos;
            }
            if (door) door.armed = false; // Son noktaya kadar kapı pasif
        }

        private void Update()
        {
            if (_player == null) return;

            // Hedefe yumuşak kayış.
            transform.position = Vector3.MoveTowards(transform.position,
                _targetPos, moveSpeed * Time.deltaTime);

            if (_settled) return;

            float dist = Mathf.Abs(_player.position.x - transform.position.x);
            float yDist = Mathf.Abs(_player.position.y - transform.position.y);

            if (dist < triggerRange && yDist < 2.5f)
            {
                if (_index < stops.Length - 1)
                {
                    _index++;
                    _targetPos = stops[_index].position;
                    // Son noktaya ulaşıldıysa artık kapı çalışır ve durur.
                    if (_index == stops.Length - 1)
                    {
                        _settled = true;
                        if (door) door.armed = true;
                    }
                }
            }
        }
    }
}
