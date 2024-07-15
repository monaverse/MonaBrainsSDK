#if BRAINS_NORMCORE
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaVariablesFloatNetworkModel
    {
        [RealtimeProperty(1, true, true)]
        private float _value;

        [RealtimeProperty(2, true, true)]
        private int _key;
    }
}
#endif