#if BRAINS_NORMCORE
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaVariablesStringNetworkModel
    {
        [RealtimeProperty(1, true, true)]
        private string _value;

        [RealtimeProperty(2, true, true)]
        private int _key;

    }
}
#endif