#if BRAINS_NORMCORE
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaBodyNetworkIdentifierModel
    {
        [RealtimeProperty(1, true, true)]
        private int _trigger;

        [RealtimeProperty(2, true, true)]
        private string _localId;

        [RealtimeProperty(3, true, true)]
        private string _prefabId;

        [RealtimeProperty(4, true, true)]
        private bool _locallyOwnedMonaBody;

        // Used to fire an event on all clients
        public void SetIdentifier(string localId, string prefabId, bool locallyOwnedMonaBody)
        {
            this.trigger++;
            this.localId = localId;
            this.prefabId = prefabId;
            this.locallyOwnedMonaBody = locallyOwnedMonaBody;
        }

        // An event that consumers of this model can subscribe to in order to respond to the event
        public delegate void EventHandler(string localId, string prefabId, bool locallyOwnedMonaBody);
        public event EventHandler eventDidFire;

        // A RealtimeCallback method that fires whenever we read any values from the server
        [RealtimeCallback(RealtimeModelEvent.OnDidRead)]
        private void DidRead()
        {
            if (eventDidFire != null && trigger != 0)
                eventDidFire(localId, prefabId, locallyOwnedMonaBody);
        }
    }
}
#endif