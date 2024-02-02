using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Core.Brain.Interfaces
{
    public interface IMonaBrainPlayer
    {
        public IMonaBody PlayerBody { get; }
        public IMonaBody PlayerCamera { get; }
        public int PlayerId { get; }
    }
}