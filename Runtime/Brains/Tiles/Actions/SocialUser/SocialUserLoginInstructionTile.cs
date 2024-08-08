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
    public class SocialUserLoginInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UserServerLogin";
        public const string NAME = "User Server Login";
        public const string CATEGORY = "User Server";
        public override Type TileType => typeof(SocialUserLoginInstructionTile);

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

            if (!string.IsNullOrEmpty(_requirePasswordName))
                _requirePassword = _brain.Variables.GetBool(_requirePasswordName);

            if (_socialPlatformUser == null)
            {
                _socialPlatformUser = _globalBrainRunner.BrainSocialUser;
                if (_socialPlatformUser == null) return Complete(InstructionTileResult.Success);
            }

            bool success;

            if (_requirePassword)
                _socialPlatformUser.LoginUser(_username, _password, out success);
            else
                _socialPlatformUser.LoginUser(_username, out success);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, success);

            return Complete(InstructionTileResult.Success);
        }
    }
}