#if BRAINS_NORMCORE
using Normal.Realtime;
using UnityEngine;

namespace Mona.Networking
{
    public class CustomRealtimePrefabDelegate : MonoBehaviour, IRealtimePrefabLoadDelegate, IRealtimePrefabInstantiateDelegate
    {
        public GameObject LoadRealtimePrefab(RealtimePrefabMetadata prefabMetadata)
        {
            return Resources.Load<GameObject>(prefabMetadata.prefabName);
        }

        public GameObject InstantiateRealtimePrefab(GameObject prefab)
        {
            return UnityEngine.Object.Instantiate(prefab);
        }

        public void DestroyRealtimePrefab(GameObject prefabInstance)
        {
            UnityEngine.Object.Destroy(prefabInstance);
        }
    }
}
#endif