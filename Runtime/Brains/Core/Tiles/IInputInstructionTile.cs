using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Tiles
{
    /* Mona Brains Input
     * - Listen for Local Input
     * - When Local Input Occurs, notify the IMonaBody
     * - The IMonaBody will notify the network (if present)
     * - The IMonaBody will send out MonaInputEvent locally
     * - If IMonaBody receives remote input via the Network, then it will send out MonaInputEvent locally
     * - This Interface will listen for MonaInputEvent and use them to evaluate input in the Do method.
     */
    public interface IInputInstructionTile
    { 
        
    }
}
