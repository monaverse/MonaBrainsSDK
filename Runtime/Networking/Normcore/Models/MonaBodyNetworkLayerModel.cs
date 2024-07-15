#if BRAINS_NORMCORE
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaBodyNetworkLayerModel
    {
        [RealtimeProperty(1, true, true)]
        private int _trigger;

        [RealtimeProperty(2, true, true)]
        private string _layer;

        [RealtimeProperty(3, true, true)]
        private bool _includeChildren;

        // Used to fire an event on all clients
        public void SetLayer(string layer, bool includeChildren)
        {
            this.trigger++;
            this.layer = layer;
            this.includeChildren = includeChildren;
        }

        // An event that consumers of this model can subscribe to in order to respond to the event
        public delegate void EventHandler(string layer, bool includeChildren);
        public event EventHandler eventDidFire;

        // A RealtimeCallback method that fires whenever we read any values from the server
        [RealtimeCallback(RealtimeModelEvent.OnDidRead)]
        private void DidRead()
        {
            if (eventDidFire != null && trigger != 0)
                eventDidFire(layer, includeChildren);
        }
    }
}
#endif