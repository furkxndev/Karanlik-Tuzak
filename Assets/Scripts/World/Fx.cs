using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Hafif, kod tabanlı partikül patlamaları (toz, kıvılcım, enkaz). Her efekt
    /// kısa ömürlü bir GameObject'tir ve kendini yok eder. Profesyonel "juice" için.
    /// </summary>
    public static class Fx
    {
        public static void Dust(Vector2 pos, float scale = 1f)
        {
            Spawn(pos, new Color(0.82f, 0.82f, 0.9f, 0.55f), 9, 1.5f, 4.0f,
                0.22f * scale, 0.55f, -7f, 0.9f, SpriteFactory.SoftCircle,
                new Vector2(0f, 0.5f));
        }

        public static void Impact(Vector2 pos, Color color, int count = 12)
        {
            Spawn(pos, color, count, 4f, 9f, 0.14f, 0.45f, -16f, 0.88f,
                SpriteFactory.Square, Vector2.zero);
        }

        public static void Debris(Vector2 pos, Color color, int count = 10)
        {
            Spawn(pos, color, count, 2f, 6f, 0.2f, 0.8f, -18f, 0.95f,
                SpriteFactory.RoundedSquare, new Vector2(0f, 0.3f));
        }

        private static void Spawn(Vector2 pos, Color color, int count, float speedMin,
            float speedMax, float size, float life, float gravity, float drag,
            Sprite sprite, Vector2 bias)
        {
            var go = new GameObject("Fx");
            go.transform.position = pos;
            var b = go.AddComponent<ParticleBurst>();
            b.Init(color, count, speedMin, speedMax, size, life, gravity, drag, sprite, bias);
        }
    }

    public class ParticleBurst : MonoBehaviour
    {
        private Transform[] _parts;
        private SpriteRenderer[] _sr;
        private Vector2[] _vel;
        private float[] _spin;
        private float _age, _life, _grav, _drag;
        private Color _color;

        public void Init(Color color, int count, float speedMin, float speedMax,
            float size, float life, float gravity, float drag, Sprite sprite, Vector2 bias)
        {
            _life = life; _grav = gravity; _drag = drag; _color = color;
            _parts = new Transform[count];
            _sr = new SpriteRenderer[count];
            _vel = new Vector2[count];
            _spin = new float[count];

            for (int i = 0; i < count; i++)
            {
                var p = new GameObject("p");
                p.transform.SetParent(transform, false);
                float s = size * Random.Range(0.6f, 1.2f);
                p.transform.localScale = new Vector3(s, s, 1f);
                p.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

                var sr = p.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = color;
                sr.sortingOrder = 20;

                Vector2 dir = (Random.insideUnitCircle.normalized + bias).normalized;
                _vel[i] = dir * Random.Range(speedMin, speedMax);
                _spin[i] = Random.Range(-360f, 360f);
                _parts[i] = p.transform;
                _sr[i] = sr;
            }
        }

        private void Update()
        {
            _age += Time.deltaTime;
            float u = _age / _life;
            if (u >= 1f) { Destroy(gameObject); return; }

            float dt = Time.deltaTime;
            float alpha = 1f - u;
            for (int i = 0; i < _parts.Length; i++)
            {
                _vel[i] += Vector2.up * _grav * dt;
                _vel[i] *= Mathf.Pow(_drag, dt * 60f);
                _parts[i].localPosition += (Vector3)(_vel[i] * dt);
                _parts[i].localRotation *= Quaternion.Euler(0, 0, _spin[i] * dt);
                var c = _color; c.a = _color.a * alpha; _sr[i].color = c;
            }
        }
    }
}
