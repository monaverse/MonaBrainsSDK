using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.State.Structs;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using System.Collections.Generic;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Tiles.Actions.PathFinding
{
    [Serializable]
    public class BakeNavigationInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "BakeNavigationInstructionTile";
        public const string NAME = "Bake Nav By Tag";
        public const string CATEGORY = "Path Finding";
        public override Type TileType => typeof(BakeNavigationInstructionTile);

        [SerializeField] private string _tag;
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        protected IMonaBrain _brain;
        protected List<NavMeshSurface> _surfaces;

        public BakeNavigationInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;

            _surfaces = new List<NavMeshSurface>();

        }

        public override InstructionTileResult Do()
        {
            if(_surfaces.Count == 0)
            {
                var bodies = MonaBody.FindByTag(_tag);
                for (var i = 0; i < bodies.Count; i++)
                {
                    var body = bodies[i];
                    var surface = body.ActiveTransform.GetComponent<NavMeshSurface>();
                    if (surface == null)
                    {
                        surface = body.ActiveTransform.AddComponent<NavMeshSurface>();
                        surface.collectObjects = CollectObjects.Children;
                    }
                    surface.BuildNavMesh();
                    _surfaces.Add(surface);
                }
            }
            return Complete(InstructionTileResult.Success);
        }
    }
}