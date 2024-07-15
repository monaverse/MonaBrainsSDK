#if BRAINS_NORMCORE
using Normal.Realtime;
using Normal.Realtime.Serialization;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class BrainsRoomModel
    {
        [RealtimeProperty(1, true, true)]
        private int _PLAYER_ID = 0;

        [RealtimeProperty(2, true, true)]
        private RealtimeSet<BrainsPlayerModel> _players;

    }

}
#endif