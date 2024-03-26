using UnityEngine;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.State.Structs;
using System;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class MoveToGlobalInstructionTile : MoveLocalInstructionTile
    {
        public const string ID = "Move To Global";
        public const string NAME = "Move To";
        public const string CATEGORY = "Adv Movement";
        public override Type TileType => typeof(MoveToGlobalInstructionTile);

        public override MoveDirectionType DirectionType => MoveDirectionType.GlobalCoordinates;

        [BrainPropertyShowLabel(nameof(DirectionType), (int)MoveDirectionType.GlobalCoordinates, "Coordinates")]
        [BrainProperty(true)]
        public Vector3 Coordinates { get => _moveToCoordinates; set => _moveToCoordinates = value; }

        [BrainPropertyValueName("Coordinates", typeof(IMonaVariablesVector3Value))]
        public string[] CoordinatesName { get => _coordinatesName; set => _coordinatesName = value; }
    }
}