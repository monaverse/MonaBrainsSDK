using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaWalletTokenSelectedEvent
    {
        public Token Token;
        public MonaWalletTokenSelectedEvent(Token token)
        {
            Token = token;
        }
    }
}