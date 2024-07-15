#if BRAINS_NORMCORE
using Normal.Realtime;
using Normal.Realtime.Serialization;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaVariablesNetworkModel
    {
        [RealtimeProperty(1, true, true)]
        private MonaVariablesNetworkIdentifierModel _identifier;

        [RealtimeProperty(2, true, true)]
        private RealtimeDictionary<MonaVariablesFloatNetworkModel> _floats;

        [RealtimeProperty(3, true, true)]
        private RealtimeDictionary<MonaVariablesBoolNetworkModel> _bools;

        [RealtimeProperty(4, true, true)]
        private RealtimeDictionary<MonaVariablesStringNetworkModel> _strings;

        [RealtimeProperty(5, true, true)]
        private RealtimeDictionary<MonaVariablesVector2NetworkModel> _vector2s;

        [RealtimeProperty(6, true, true)]
        private RealtimeDictionary<MonaVariablesVector3NetworkModel> _vector3s;
    }
}
#endif