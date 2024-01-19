using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using System;
using UnityEngine;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.Broadcasting.Interfaces;
using Mona.SDK.Core.Body;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Broadcasting
{
    [Serializable]
    public class BroadcastMessageToTagInstructionTile : BroadcastMessageInstructionTile, IBroadcastMessageToTagInstructionTile
    {
        public const string ID = "BroadcastMessageToTag";
        public const string NAME = "Broadcast Message\n To Tag";
        public const string CATEGORY = "Broadcasting";
        public override Type TileType => typeof(BroadcastMessageToTagInstructionTile);

        [SerializeField]
        private string _message;

        [BrainProperty(true)]
        public string Message { get => _message; set => _message = value; }

        [SerializeField]
        private string _Tag;

        [BrainPropertyMonaTag(true)]
        public string Tag { get => _Tag; set => _Tag = value; }
        
        private IMonaBrain _brain;
        private Dictionary<IMonaBody, IMonaBrainRunner> _runnerCache = new Dictionary<IMonaBody, IMonaBrainRunner>();

        public BroadcastMessageToTagInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        private IMonaBrainRunner GetCachedRunner(IMonaBody body)
        {
            if (!_runnerCache.ContainsKey(body))
                _runnerCache.Add(body, body.Transform.GetComponent<IMonaBrainRunner>());
            return _runnerCache[body];
        }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_Tag);
            
            for(var i = 0;i < bodies.Count; i++)
            {
                var body = bodies[i];
                var runner = GetCachedRunner(body);
                if (runner != null)
                {
                    for(var j = 0; j < runner.BrainInstances.Count; j++)
                        BroadcastMessage(_brain, _message, runner.BrainInstances[j]);
                }
            }

            return Complete(InstructionTileResult.Success);
        }
    }
}