using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Control
{
    public interface IInstruction
    {
        event Action<IInstruction> OnReset;
        event Action<int> OnRefresh;
        event Action OnDeselect;

        bool IsRunning();
        bool HasEndTile();

        List<IInstructionTile> InstructionTiles { get; }
        void Preload(IMonaBrain brain);
        void Execute(InstructionEventTypes eventType, IInstructionEvent evt);
        void AddTile(IInstructionTile tile, int i, bool isCore);
        void ReplaceTile(int i, IInstructionTile tile);
        void DeleteTile(int i);
        void MoveTileRight(int i);
        void MoveTileLeft(int i);
        bool HasConditional();
        void Deselect();
        void Unload();
    }
}