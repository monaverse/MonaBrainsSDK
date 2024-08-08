using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.SocialUser
{
    [Serializable]
    public class SocialUserUpdatePasswordInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "UserServerUpdatePassword";
        public const string NAME = "Update User Password";
        public const string CATEGORY = "User Server";
        public override Type TileType => typeof(SocialUserUpdatePasswordInstructionTile);

        [SerializeField] private string _newPassword;
        [SerializeField] private string _newPasswordName;
        [BrainProperty(true)] public string NewPassword { get => _newPassword; set => _newPassword = value; }
        [BrainPropertyValueName("NewPassword", typeof(IMonaVariablesStringValue))] public string NewPasswordName { get => _newPasswordName; set => _newPasswordName = value; }

        [SerializeField] private string _storeSuccessOn;
        [BrainPropertyValue(typeof(IMonaVariablesBoolValue), false)] public string StoreSuccessOn { get => _storeSuccessOn; set => _storeSuccessOn = value; }

        public SocialUserUpdatePasswordInstructionTile() { }

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

            if (!string.IsNullOrEmpty(_newPasswordName))
                _newPassword = _brain.Variables.GetString(_newPasswordName);

            if (_socialPlatformUser == null)
            {
                _socialPlatformUser = _globalBrainRunner.BrainSocialUser;
                if (_socialPlatformUser == null) return Complete(InstructionTileResult.Success);
            }

            _socialPlatformUser.ChangeCurrentUserPassword(_newPassword, out bool success);

            if (!string.IsNullOrEmpty(_storeSuccessOn))
                _brain.Variables.Set(_storeSuccessOn, success);

            return Complete(InstructionTileResult.Success);
        }
    }
}