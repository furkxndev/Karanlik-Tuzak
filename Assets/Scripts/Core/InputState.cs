using UnityEngine;

namespace LevelDevilClone
{
    /// <summary>
    /// Dokunmatik UI butonları ve klavye (editör testi) tarafından beslenen
    /// merkezi girdi durumu. PlayerController buradan okur.
    /// </summary>
    public class InputState
    {
        public bool Left;
        public bool Right;
        public bool JumpHeld;       // Zıplama butonu basılı tutuluyor mu (değişken zıplama yüksekliği için)
        private bool _jumpQueued;   // Bu karede zıplamaya basıldı (jump buffer için tüketilir)

        public void PressJump()
        {
            _jumpQueued = true;
            JumpHeld = true;
        }

        public void ReleaseJump()
        {
            JumpHeld = false;
        }

        /// <summary>Zıplama isteğini bir kez tüketir (true dönerse zıpla).</summary>
        public bool ConsumeJump()
        {
            if (_jumpQueued)
            {
                _jumpQueued = false;
                return true;
            }
            return false;
        }

        /// <summary>Klavye desteği — editörde test için. Mobilde de zararsız.</summary>
        public void PollKeyboard()
        {
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) Left = true;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) Right = true;
            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A)) Left = false;
            if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D)) Right = false;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.W))
                PressJump();
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow) ||
                Input.GetKeyUp(KeyCode.W))
                ReleaseJump();
        }
    }
}
