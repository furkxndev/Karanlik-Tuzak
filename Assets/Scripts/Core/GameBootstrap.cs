using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Tek giriş noktası. Boş bir sahnede Play'e basıldığında tüm oyunu kurar.
    ///
    /// KULLANIM (iki seçenekten biri yeterli):
    ///  1) Hiçbir şey yapma — RuntimeInitializeOnLoadMethod sayesinde herhangi
    ///     bir sahnede otomatik başlar.
    ///  2) Boş bir GameObject oluştur, üzerine bu scripti ekle. (Manuel kontrol)
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private static bool _booted;

        // Boş sahnede bile otomatik başlatma.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBoot()
        {
            if (_booted) return;
            if (FindObjectOfType<GameBootstrap>() != null) return; // Sahnede zaten varsa o başlatır
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            if (_booted) { Destroy(gameObject); return; }
            _booted = true;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            // 2D yerçekimini biraz güçlendir (daha tatmin edici düşüş).
            Physics2D.gravity = new Vector2(0f, -9.81f);

            var gmGo = new GameObject("GameManager");
            var gm = gmGo.AddComponent<GameManager>();
            gm.Initialize();
        }
    }
}
