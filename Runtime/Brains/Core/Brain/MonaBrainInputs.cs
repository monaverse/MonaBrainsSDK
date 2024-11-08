using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mona.SDK.Brains.Core.Brain
{
    public class MonaBrainInputs : MonoBehaviour
    {
        private Inputs _inputs;

        private Inputs GetInput()
        {
            if (_inputs == null)
                _inputs = new Inputs();
            return _inputs;
        }

        public virtual void Enable() => GetInput().Player.Enable();
        public virtual void Disable() => GetInput().Player.Disable();
        public virtual bool Enabled => GetInput().Player.enabled;
        public virtual InputAction Action => GetInput().Player.Action;
        public virtual InputAction Look => GetInput().Player.Look;
        public virtual InputAction Move => GetInput().Player.Move;
        public virtual InputAction Jump => GetInput().Player.Jump;
        public virtual InputAction Sprint => GetInput().Player.Sprint;
        public virtual InputAction SwitchCamera => GetInput().Player.SwitchCamera;
        public virtual InputAction Respawn => GetInput().Player.Respawn;
        public virtual InputAction Debug => GetInput().Player.Debug;
        public virtual InputAction ToggleUI => GetInput().Player.ToggleUI;
        public virtual InputAction EmoteWheel => GetInput().Player.EmoteWheel;
        public virtual InputAction EmojiTray => GetInput().Player.EmojiTray;
        public virtual InputAction ToggleNametags => GetInput().Player.ToggleNametags;
        public virtual InputAction Escape => GetInput().Player.Escape;
        public virtual InputAction ToggleMouseCapture => GetInput().Player.ToggleMouseCapture;
        public virtual InputAction OpenChat => GetInput().Player.OpenChat;
    }
}
