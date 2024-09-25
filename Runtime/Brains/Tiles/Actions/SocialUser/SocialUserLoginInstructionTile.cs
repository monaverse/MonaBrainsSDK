using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Core.Utils;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Tiles.Actions.SocialUser
{
    [Serializable]
    public class SocialUserLoginInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, IPauseableInstructionTile, IActivateInstructionTile
    {
        public const string ID = "UserServerLogin";
        public const string NAME = "User Server Login";
        public const string CATEGORY = "User Server";
        public override Type TileType => typeof(SocialUserLoginInstructionTile);

        [SerializeField] private SocialUserLoginTypes _loginType = SocialUserLoginTypes.Leaderboards;
        [BrainPropertyEnum(true)] public SocialUserLoginTypes LoginType { get => _loginType; set => _loginType = value; }

        [SerializeField] private string _username;
        [SerializeField] private string _usernameName;
        [BrainProperty(true)] public string Username { get => _username; set => _username = value; }
        [BrainPropertyValueName("Username", typeof(IMonaVariablesStringValue))] public string UsernameName { get => _usernameName; set => _usernameName = value; }

        [SerializeField] private string _password;
        [SerializeField] private string _passwordName;
        [BrainProperty(true)] public string Password { get => _password; set => _password = value; }
        [BrainPropertyValueName("Password", typeof(IMonaVariablesStringValue))] public string PasswordName { get => _passwordName; set => _passwordName = value; }

        [SerializeField] private bool _requirePassword = true;
        [SerializeField] private string _requirePasswordName;
        [BrainProperty(false)] public bool RequirePassword { get => _requirePassword; set => _requirePassword = value; }
        [BrainPropertyValueName("RequirePassword", typeof(IMonaVariablesBoolValue))] public string RequirePasswordName { get => _requirePasswordName; set => _requirePasswordName = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        public SocialUserLoginInstructionTile() { }

        private bool _processLogin;
        private bool _active;
        private bool _isRunning;
        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainSocialPlatformUserAsync _socialPlatformUser;
        private IBrainSocialPlatformUserAsync _socialPlatformUser2;
        private BrainProcess _serverProcess;
        private Action<MonaBodyFixedTickEvent> OnFixedTick;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
            SetActive(true);
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                if (_isRunning)
                    LostControl();

                return;
            }

            if (_isRunning)
            {
                AddFixedTickDelegate();
            }
        }

        public override void Unload(bool destroy = false)
        {
            SetActive(false);
            _isRunning = false;
            RemoveFixedTickDelegate();
        }

        public void Pause()
        {
            RemoveFixedTickDelegate();
        }

        public bool Resume()
        {
            UpdateActive();
            return _isRunning;
        }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveFixedTickDelegate();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        private void AddFixedTickDelegate()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void RemoveFixedTickDelegate()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
        }

        private void LostControl()
        {
            _isRunning = false;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick();
        }

        private void FixedTick()
        {
            if (_socialPlatformUser == null || _serverProcess == null || _serverProcess.IsProcessing)
                return;

            _isRunning = false;

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, _serverProcess.WasSuccessful);

            Complete(InstructionTileResult.Success, true);
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if(_loginType == SocialUserLoginTypes.Leaderboards)
                _socialPlatformUser = (IBrainSocialPlatformUserAsync)_globalBrainRunner.BrainLeaderboards;
            else if(_loginType == SocialUserLoginTypes.Storage)
                _socialPlatformUser = (IBrainSocialPlatformUserAsync)_globalBrainRunner.CloudStorage;


            _processLogin = false;

            if (!_isRunning)
            {
                if (!string.IsNullOrEmpty(_usernameName))
                    _username = _brain.Variables.GetString(_usernameName);

                if (!string.IsNullOrEmpty(_passwordName))
                    _password = _brain.Variables.GetString(_passwordName);

                if (!string.IsNullOrEmpty(_requirePasswordName))
                    _requirePassword = _brain.Variables.GetBool(_requirePasswordName);

                if (_socialPlatformUser == null)
                {
                    _socialPlatformUser = _globalBrainRunner.BrainSocialUser;
                    if (_socialPlatformUser == null) return Complete(InstructionTileResult.Success);
                }

                ProcessLogin();

                if(!_processLogin)
                    AddFixedTickDelegate();
            }

            return _processLogin ? Complete(InstructionTileResult.Success) : Complete(InstructionTileResult.Running);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, false);

            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private async Task ProcessLogin()
        {
            if (_requirePassword)
                _serverProcess = await _socialPlatformUser.LoginUser(_username, _password);
            else
                _serverProcess = await _socialPlatformUser.LoginUser(_username);
            _processLogin = true;
        }
    }
}