#if BRAINS_NORMCORE
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class BrainsPlayerModel
    {
        [RealtimeProperty(1, true, true)]
        private int _trigger;

        [RealtimeProperty(2, true, true)]
        private int _playerID;

        [RealtimeProperty(3, true, true)]
        private int _clientID;

        [RealtimeProperty(4, true, true)]
        private string _name;

        // Used to fire an event on all clients
        public void SetPlayer(int playerID, int clientID, string name)
        {
            this.trigger++;
            this.playerID = playerID;
            this.clientID = clientID;
            this.name = name;
        }

        // An event that consumers of this model can subscribe to in order to respond to the event
        public delegate void EventHandler(int playerID, int clientID, string name);
        public event EventHandler eventDidFire;

        // A RealtimeCallback method that fires whenever we read any values from the server
        [RealtimeCallback(RealtimeModelEvent.OnDidRead)]
        private void DidRead()
        {
            if (eventDidFire != null && trigger != 0)
                eventDidFire(playerID, clientID, name);
        }
    }

}
#endif