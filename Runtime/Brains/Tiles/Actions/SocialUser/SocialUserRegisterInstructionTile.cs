using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.SocialUser
{
    [Serializable]
    public class SocialUserRegisterInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "RegisterNewUser";
        public const string NAME = "Register New User";
        public const string CATEGORY = "User Server";
        public override Type TileType => typeof(SocialUserRegisterInstructionTile);

        [SerializeField] private string _username;
        [SerializeField] private string _usernameName;
        [BrainProperty(true)] public string Username { get => _username; set => _username = value; }
        [BrainPropertyValueName("Username", typeof(IMonaVariablesStringValue))] public string UsernameName { get => _usernameName; set => _usernameName = value; }

        [SerializeField] private string _password;
        [SerializeField] private string _passwordName;
        [BrainProperty(true)] public string Password { get => _password; set => _password = value; }
        [BrainPropertyValueName("Password", typeof(IMonaVariablesStringValue))] public string PasswordName { get => _passwordName; set => _passwordName = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        public SocialUserRegisterInstructionTile() { }

        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainSocialPlatformUser _socialPlatformUser;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_usernameName))
                _username = _brain.Variables.GetString(_usernameName);

            if (!string.IsNullOrEmpty(_passwordName))
                _password = _brain.Variables.GetString(_passwordName);

            if (_socialPlatformUser == null)
            {
                _socialPlatformUser = _globalBrainRunner.BrainSocialUser;
                if (_socialPlatformUser == null) return Complete(InstructionTileResult.Success);
            }

            _socialPlatformUser.RegisterNewUser(_username, _password, out bool success);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, success);

            return Complete(InstructionTileResult.Success);
        }
    }
}