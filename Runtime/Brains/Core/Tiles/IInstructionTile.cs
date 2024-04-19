using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Tiles
{
    public interface IInstructionTile : IGraphElementData
    {
        event Action<InstructionTileResult, string, IInstructionTile> OnExecute;
        event Action OnMuteChanged;

        string Id { get; set; }
        string Name { get; set; }
        string Category { get; set; }
        bool Muted { get; set; }
        Type TileType { get; }
        InstructionTileResult LastResult { get; set; }
        IInstructionTile NextExecutionTile { get; set; }
        
        IInstructionTileCallback ThenCallback { get; }
        void SetThenCallback(IInstructionTileCallback thenCallback);

        InstructionTileResult Do();
        void Unload(bool destroy = false);
    }
}