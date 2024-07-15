#if BRAINS_NORMCORE
using UnityEngine;
using Normal.Realtime;

namespace Mona.Networking
{
    [RealtimeModel(createMetaModel: true)]
    public partial class MonaVariablesVector3NetworkModel
    {
        [RealtimeProperty(1, true, true)]
        private Vector3 _value;

        [RealtimeProperty(2, true, true)]
        private int _key;
    }
}
#endif