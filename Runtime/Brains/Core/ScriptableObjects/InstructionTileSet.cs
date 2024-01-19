using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting;
using Mona.SDK.Brains.Tiles.Actions.General;
using Mona.SDK.Brains.Tiles.Actions.Movement;
using Mona.SDK.Brains.Tiles.Actions.Timing;
using Mona.SDK.Brains.Tiles.Conditions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Utils/Tile Set")]
    [Serializable]
    public class InstructionTileSet : ScriptableObject, IInstructionTileSet
    {
        [SerializeField]
        private string _version = "0.0.0";
        public string Version => _version;

        [SerializeField]
        public List<ScriptableObject> _conditionTiles = new List<ScriptableObject>();

        [SerializeField]
        public List<ScriptableObject> _actionTiles = new List<ScriptableObject>();

        public List<IInstructionTileDefinition> ConditionTiles => _conditionTiles.ConvertAll<IInstructionTileDefinition>(x => (IInstructionTileDefinition)x);
        public List<IInstructionTileDefinition> ActionTiles => _actionTiles.ConvertAll<IInstructionTileDefinition>(x => (IInstructionTileDefinition)x);

        public IInstructionTileDefinition Find(string id)
        {
            var def = ConditionTiles.Find((x) => x.Id == id);
            if (def != null) return def;
            def = ActionTiles.Find((x) => x.Id == id);
            if (def != null) return def;
            return null;
        }

    }
}
