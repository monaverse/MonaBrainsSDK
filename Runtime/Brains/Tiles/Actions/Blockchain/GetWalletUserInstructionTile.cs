using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Brains.Actions.Blockchain.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Core.Body;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Blockchain
{
    [Serializable]
    public class GetWalletUserInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IActionInstructionTile, INeedAuthorityInstructionTile, IBlockchainInstructionTile
    {
        public const string ID = "GetWalletUser";
        public const string NAME = "Get Wallet User";
        public const string CATEGORY = "Blockchain";
        public override Type TileType => typeof(GetWalletUserInstructionTile);

        [SerializeField] private string _setToName;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string SetToName { get => _setToName; set => _setToName = value; }

        [SerializeField] private string _setToEmail;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string SetToEmail { get => _setToEmail; set => _setToEmail = value; }

        private IMonaBrain _brain;
        IMonaBrainBlockchain _blockchain;

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }
        public GetWalletUserInstructionTile() { }

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _instruction = instruction;
            _blockchain = MonaGlobalBrainRunner.Instance.Blockchain;                
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            if (_blockchain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_setToName))
                _brain.Variables.Set(_setToName, _blockchain.WalletUsername);

            if (!string.IsNullOrEmpty(_setToEmail))
                _brain.Variables.Set(_setToEmail, _blockchain.WalletUserEmail);

            return Complete(InstructionTileResult.Success);
        }
    }
}