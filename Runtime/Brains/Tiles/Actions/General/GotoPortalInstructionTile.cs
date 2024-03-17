using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Utils;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class GotoPortalInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "GotoPortal";
        public const string NAME = "Goto Portal";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(GotoPortalInstructionTile);

        [SerializeField] private string _portalName;
        [SerializeField] private string _portalNameValueName;
        [BrainProperty(true)] public string PortalName { get => _portalName; set => _portalName = value; }
        [BrainPropertyValueName("PortalName", typeof(IMonaVariablesBoolValue))] public string ValueValueName { get => _portalNameValueName; set => _portalNameValueName = value; }

        private IMonaBrain _brain;

        public GotoPortalInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!string.IsNullOrEmpty(_portalNameValueName))
                _portalName = _brain.Variables.GetString(_portalNameValueName);

            if (_brain != null)
            {
                MonaPortal.GoTo(_portalName);
                return Complete(InstructionTileResult.Success);
            }
            return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

    }
}