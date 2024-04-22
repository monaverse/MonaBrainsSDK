using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.Core.Events
{
    public struct MonaWalletConnectedEvent
    {
        public string Address;
        public MonaWalletConnectedEvent(string address)
        {
            Address = address;
        }
    }
}