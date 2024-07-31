using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Input;
using System;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Control
{
    public interface IInstruction
    {
        event Action<IInstruction> OnReset;
        event Action<int> OnRefresh;
        event Action OnDeselect;
        event Action OnSelect;

        bool IsRunning();
        bool HasEndTile(IMonaBrainPage page);
        bool HasAnimationTiles();
        bool HasRigidbodyTiles();
        bool HasUsePhysicsTileSetToTrue();
        bool HasOnMessageTile(string message);

        List<IInstructionTile> InstructionTiles { get; }
        List<IBlockchainInstructionTile> BlockchainTiles { get; }
        IInstructionTile CurrentTile { get; }
        MonaInput InstructionInput { get; set; }
        List<IMonaBody> InstructionBodies { get; set; }
        bool Muted { get; set; }
        bool HasElseTile { get; }
        InstructionTileResult Result { get; set; }

        List<Token> Tokens { get; set; }
        IMonaBrainPage Page { get; }

        void ToggleMuteTile(int i);
        void ToggleMute();

        void Preload(IMonaBrain brain, IMonaBrainPage page);
        void Execute(InstructionEventTypes eventType, bool elseOkay, out bool executing, InstructionEvent evt);
        void AddTile(IInstructionTile tile, int i, IMonaBrainPage page);
        void ReplaceTile(int i, IInstructionTile tile);
        void DeleteTile(int i);
        void MoveTileRight(int i);
        void MoveTileLeft(int i);
        bool HasConditional();
        void Select();
        void Deselect();
        void Unload(bool destroy = false);
        void Pause();
        void Resume();
        void SetActive(bool active);
    }
}