using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Control
{
    public interface IMonaBrainPage
    {
        List<IInstruction> Instructions { get; }
        string Name { get; set; }
        bool IsCore { get; }
        bool IsActive { get; }
        bool HasAnimationTiles();
        bool HasRigidbodyTiles();
        bool HasUsePhysicsTileSetToTrue();
        bool HasOnMessageTile(string message);
        void SetActive(bool active);
        void SetIsCore(bool core);
        void Preload(IMonaBrain brain);
        void ExecuteInstructions(InstructionEventTypes eventType, InstructionEvent evt = default);
        void Unload();
        void Pause();
        void Resume();
    }
}