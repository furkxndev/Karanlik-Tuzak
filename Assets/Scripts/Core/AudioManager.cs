using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Tüm ses efektlerini ve atmosferik fon uğultusunu runtime'da kod ile üretir
    /// (hiçbir ses dosyası import etmeye gerek yok). Statik Play* metotlarıyla
    /// her yerden çağrılır; AudioManager yoksa sessizce yok sayılır.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I;

        private AudioSource _sfx;
        private AudioSource _ambient;

        private AudioClip _jump, _land, _step, _death, _win, _flip,
                          _spike, _collapse, _crusher, _drop, _click;

        private const int Rate = 44100;

        public void Init(Camera cam)
        {
            I = this;

            // Dinleyici garantisi
            if (FindObjectOfType<AudioListener>() == null && cam != null)
                cam.gameObject.AddComponent<AudioListener>();

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.volume = 0.85f;

            _ambient = gameObject.AddComponent<AudioSource>();
            _ambient.loop = true;
            _ambient.playOnAwake = false;
            _ambient.volume = 0.13f;

            GenerateClips();

            _ambient.clip = BuildAmbient(2f);
            _ambient.Play();
        }

        private void GenerateClips()
        {
            // (ad, süre, başlangıç frekansı, bitiş frekansı, dalga[0=sine,1=square],
            //  decay, gürültü oranı, ses)
            _jump     = Build("jump",     0.14f, 420, 760, 0, 7f, 0.0f, 0.55f);
            _land     = Build("land",     0.11f, 200, 90,  0, 16f, 0.30f, 0.55f);
            _step     = Build("step",     0.05f, 150, 100, 0, 26f, 0.45f, 0.22f);
            _death    = Build("death",    0.38f, 320, 55,  1, 5f,  0.30f, 0.55f);
            _flip     = Build("flip",     0.20f, 260, 880, 0, 5f,  0.15f, 0.45f);
            _spike    = Build("spike",    0.10f, 820, 1500, 1, 16f, 0.10f, 0.45f);
            _collapse = Build("collapse", 0.42f, 200, 120, 0, 7f,  0.85f, 0.5f);
            _crusher  = Build("crusher",  0.28f, 150, 48,  0, 9f,  0.35f, 0.7f);
            _drop     = Build("drop",     0.26f, 440, 120, 0, 7f,  0.10f, 0.45f);
            _click    = Build("click",    0.035f, 900, 700, 1, 30f, 0.0f, 0.25f);
            _win      = BuildArp("win", new float[] { 523, 659, 784, 1046 }, 0.11f, 0.45f);
        }

        // ---- Tek sesli procedural synth ----
        private AudioClip Build(string name, float dur, float f0, float f1,
            int wave, float decay, float noiseMix, float vol)
        {
            int n = Mathf.Max(1, (int)(Rate * dur));
            var data = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                float u = t / dur;
                float f = Mathf.Lerp(f0, f1, u);
                phase += 2f * Mathf.PI * f / Rate;

                float s = wave == 1 ? Mathf.Sign(Mathf.Sin(phase)) : Mathf.Sin(phase);
                if (noiseMix > 0f) s = Mathf.Lerp(s, Random.value * 2f - 1f, noiseMix);

                float env = Mathf.Exp(-u * decay);
                float atk = Mathf.Clamp01(t / 0.004f);   // tık sesini önleyen yumuşak giriş
                data[i] = s * env * atk * vol;
            }
            return ToClip(name, data);
        }

        // ---- Çok notalı arpej (kazanma) ----
        private AudioClip BuildArp(string name, float[] freqs, float noteDur, float vol)
        {
            int per = Mathf.Max(1, (int)(Rate * noteDur));
            var data = new float[per * freqs.Length];
            for (int k = 0; k < freqs.Length; k++)
            {
                float phase = 0f;
                for (int i = 0; i < per; i++)
                {
                    float t = (float)i / Rate;
                    phase += 2f * Mathf.PI * freqs[k] / Rate;
                    float env = Mathf.Exp(-(t / noteDur) * 3.5f);
                    float atk = Mathf.Clamp01(t / 0.004f);
                    data[k * per + i] = Mathf.Sin(phase) * env * atk * vol;
                }
            }
            return ToClip(name, data);
        }

        // ---- Sürekli, kusursuz döngülenen atmosfer uğultusu ----
        private AudioClip BuildAmbient(float dur)
        {
            int n = (int)(Rate * dur);
            var data = new float[n];
            // Döngü dikişinde tık olmaması için frekanslar tam sayıda çevrim yapar.
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / Rate;
                float lfo = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * (0.5f) * t); // 1 çevrim / 2s
                float s = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.6f
                        + Mathf.Sin(2f * Mathf.PI * 82.5f * t) * 0.4f
                        + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.2f;
                data[i] = s * 0.4f * lfo;
            }
            return ToClip("ambient", data, true);
        }

        private AudioClip ToClip(string name, float[] data, bool loop = false)
        {
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private void Shot(AudioClip c, float vol, float pitchVar)
        {
            if (c == null || _sfx == null) return;
            _sfx.pitch = 1f + Random.Range(-pitchVar, pitchVar);
            _sfx.PlayOneShot(c, vol);
        }

        // ---- Statik kısayollar (her yerden güvenle çağrılır) ----
        public static void PlayJump()     { if (I) I.Shot(I._jump, 0.7f, 0.06f); }
        public static void PlayLand()     { if (I) I.Shot(I._land, 0.7f, 0.08f); }
        public static void PlayStep()     { if (I) I.Shot(I._step, 0.5f, 0.12f); }
        public static void PlayDeath()    { if (I) I.Shot(I._death, 0.8f, 0.04f); }
        public static void PlayWin()      { if (I) I.Shot(I._win, 0.8f, 0.0f); }
        public static void PlayFlip()     { if (I) I.Shot(I._flip, 0.6f, 0.05f); }
        public static void PlaySpike()    { if (I) I.Shot(I._spike, 0.7f, 0.05f); }
        public static void PlayCollapse() { if (I) I.Shot(I._collapse, 0.7f, 0.05f); }
        public static void PlayCrusher()  { if (I) I.Shot(I._crusher, 0.55f, 0.06f); }
        public static void PlayDrop()     { if (I) I.Shot(I._drop, 0.6f, 0.05f); }
        public static void PlayClick()    { if (I) I.Shot(I._click, 0.5f, 0.05f); }
    }
}
