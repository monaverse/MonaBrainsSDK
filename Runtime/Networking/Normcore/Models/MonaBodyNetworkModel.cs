#if BRAINS_NORMCORE
using UnityEngine;
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaBodyNetworkModel
    {
        [RealtimeProperty(1, true, true)]
        private MonaBodyNetworkIdentifierModel _identifier;

        [RealtimeProperty(2, true, true)]
        private MonaBodyNetworkLayerModel _currentLayer;

        [RealtimeProperty(3, true, true)]
        private bool _active;

        [RealtimeProperty(4, true, true)]
        private bool _paused;

        [RealtimeProperty(5, true, true)]
        private bool _visible;

        [RealtimeProperty(6, true, true)]
        private Vector3 _scale;

        [RealtimeProperty(7, true, true)]
        private Color _color;

        [RealtimeProperty(8, true, true)]
        private BrainsPlayerModel _player;
    }
}
#endif