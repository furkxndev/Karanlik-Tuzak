using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Karakterin uzuvlarını duruma göre animasyonlar: yürürken kollar/bacaklar
    /// salınır, havadayken toplanır, dururken hafif nefes alır. PlayerController'dan
    /// hız ve zemin durumunu okur.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        public Transform legL, legR, armL, armR, body;
        public PlayerController player;

        public float walkAmp = 38f;     // derece
        public float armAmp = 28f;
        public float walkFreq = 1.6f;

        private float _phase;
        private float _prevStepSin;
        private Vector3 _bodyBaseScale = Vector3.one;
        private Vector3 _bodyBasePos;

        private void Start()
        {
            if (body != null)
            {
                _bodyBaseScale = body.localScale;
                _bodyBasePos = body.localPosition;
            }
        }

        private void Update()
        {
            if (player == null) return;

            float vx = player.Velocity.x;
            float speed = Mathf.Abs(vx);
            bool grounded = player.IsGrounded;
            float dt = Time.deltaTime;

            if (grounded && speed > 0.6f)
            {
                // Yürüyüş: adım fazını hıza göre ilerlet.
                _phase += dt * walkFreq * (2f + speed);
                float s = Mathf.Sin(_phase);
                // Ayak yere bastığında (sinüs sıfır geçişi) adım sesi.
                if (Mathf.Sign(s) != Mathf.Sign(_prevStepSin) && _prevStepSin != 0f)
                    AudioManager.PlayStep();
                _prevStepSin = s;
                SetRot(legL, s * walkAmp);
                SetRot(legR, -s * walkAmp);
                SetRot(armL, -s * armAmp);
                SetRot(armR, s * armAmp);

                // Hafif zıplama hissi (yürürken yukarı-aşağı)
                if (body != null)
                    body.localPosition = _bodyBasePos + Vector3.up * Mathf.Abs(s) * 0.03f;
            }
            else if (!grounded)
            {
                // Havada: bacaklar hafif toplanır, kollar yukarı kalkar.
                SmoothRot(legL, -18f, dt);
                SmoothRot(legR, 18f, dt);
                SmoothRot(armL, 32f, dt);
                SmoothRot(armR, -32f, dt);
                if (body != null)
                    body.localPosition = Vector3.Lerp(body.localPosition, _bodyBasePos, dt * 8f);
            }
            else
            {
                // Dururken: uzuvlar nötre döner, gövde nefes alır.
                _phase = 0f;
                SmoothRot(legL, 0f, dt);
                SmoothRot(legR, 0f, dt);
                SmoothRot(armL, 6f, dt);
                SmoothRot(armR, -6f, dt);
                if (body != null)
                {
                    float breath = Mathf.Sin(Time.time * 2f) * 0.015f;
                    body.localScale = _bodyBaseScale + new Vector3(-breath, breath, 0f);
                    body.localPosition = Vector3.Lerp(body.localPosition, _bodyBasePos, dt * 6f);
                }
            }
        }

        private static void SetRot(Transform t, float deg)
        {
            if (t != null) t.localRotation = Quaternion.Euler(0, 0, deg);
        }

        private static void SmoothRot(Transform t, float deg, float dt)
        {
            if (t == null) return;
            t.localRotation = Quaternion.Lerp(t.localRotation,
                Quaternion.Euler(0, 0, deg), dt * 10f);
        }
    }
}
