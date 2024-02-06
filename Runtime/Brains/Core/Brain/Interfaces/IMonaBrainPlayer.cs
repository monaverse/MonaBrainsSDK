using Mona.SDK.Core.Body;
using Mona.SDK.Core.Network.Interfaces;

namespace Mona.SDK.Brains.Core.Brain.Interfaces
{
    public interface IMonaBrainPlayer
    {
        public IMonaBody PlayerBody { get; }
        public IMonaBody PlayerCamera { get; }
        public int PlayerId { get; }
        public IMonaNetworkSettings NetworkSettings { get; }
    }
}