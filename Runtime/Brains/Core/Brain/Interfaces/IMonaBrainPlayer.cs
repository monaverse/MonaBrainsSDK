using Mona.SDK.Brains.Core.Brain.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Network.Interfaces;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Brain.Interfaces
{
    public interface IMonaBrainPlayer
    {
        public IMonaBody PlayerBody { get; }
        public IMonaBody PlayerCamera { get; }
        public int PlayerId { get; }
        public IMonaNetworkSettings NetworkSettings { get; }
        public List<MonaRemotePlayer> OtherPlayers { get; }
    }
}