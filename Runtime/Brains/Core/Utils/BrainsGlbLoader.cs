using System;
using System.Collections;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using UnityEngine.Networking;
using VRM;
using UnityGLTF;
using UnityGLTF.Loader;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Structs;
using System.Runtime.ExceptionServices;
using Mona.SDK.Core.Body;

namespace Mona.SDK.Brains.Core.Utils
{
    public static class ByteArrayExtensions
    {
        public static float SizeInMB(this byte[] byteArray)
        {
            if (byteArray == null || byteArray.Length == 0)
            {
                return 0f;
            }

            return (float)byteArray.Length / (1024 * 1024);
        }
    }

    public sealed class BrainsGlbLoader : MonoBehaviour
    {
        public static Dictionary<string, List<GameObject>> Pool = new Dictionary<string, List<GameObject>>();
        public static Dictionary<string, List<GameObject>> Used = new Dictionary<string, List<GameObject>>();

        public void ReturnToPool(string url, GameObject instance)
        {
            if (!Pool.ContainsKey(url))
                Pool[url] = new List<GameObject>();

            if (!Used.ContainsKey(url))
                Used[url] = new List<GameObject>();

            if (!Pool[url].Contains(instance) && instance != null)
                Pool[url].Add(instance);

            if (Used[url].Contains(instance) && instance != null)
                Used[url].Remove(instance);

            if (instance != null)
            {
                instance.transform.SetParent(transform, true);
                instance.SetActive(false);
                Debug.Log($"{nameof(ReturnToPool)} {url}", instance.gameObject);
            }
            else
                Debug.Log($"{nameof(ReturnToPool)} {url} returned empty");
        }

        public GameObject GetFromPool(string url)
        {
            if (!Pool.ContainsKey(url))
                Pool[url] = new List<GameObject>();

            if (!Used.ContainsKey(url))
                Used[url] = new List<GameObject>();

            if (Pool[url].Count > 0)
            {
                var instance = Pool[url][0];
                Pool[url].RemoveAt(0);
                //instance.SetActive(true);
                Debug.Log($"{nameof(GetFromPool)} {url}", instance.gameObject);

                if (!Used[url].Contains(instance) && instance != null)
                    Used[url].Add(instance);

                return instance;                
            }
            return null;
        }

        private int _cached;
        public void CacheTokens(List<Token> tokens, Action callback, int poolSize = 1)
        {
            Action<GameObject> loadCallback = (obj) =>
            {
                _cached++;
                Debug.Log($"CACHED: " + _cached);
                if (_cached >= tokens.Count)
                    callback?.Invoke();
            };

            for(var i = 0;i < tokens.Count; i++)
            {
                Load(tokens[i].AssetUrl, false, loadCallback, poolSize, true);
            }
        }

        public void HandleDestroyed(BrainsGlb obj)
        {
            //Debug.LogError($"{nameof(BrainsGlbLoader)} glb destroyed {obj.Url} {obj.gameObject.name}");
            obj.OnDestroyed -= HandleDestroyed;
            if (Pool[obj.Url].Contains(obj.gameObject))
                Pool[obj.Url].Remove(obj.gameObject);
        }

        public void Load(string url, bool importAnimation, Action<GameObject> callback, int poolSize = 1, bool returnToPool = false)
        {
            Debug.Log($"{nameof(Load)} GLB URL: {url}");

            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError($"{nameof(BrainsGlbLoader)}.{nameof(Load)} url is empty");
                callback?.Invoke(null);
                return;
            }

            var instance = GetFromPool(url);
            if (instance == null)
            {
                if (!Used.ContainsKey(url) || Used[url].Count == 0)
                {
                    GetVrmData(url, (byte[] avatarData) =>
                    {
                        Debug.Log($"{nameof(BrainsGlbLoader)} {url} loaded from url and created new");

                        if (avatarData == null)
                        {
                            Debug.LogError("Invalid URL: Failed to download VRM file");
                            callback?.Invoke(null);
                            return;
                        }
                        if (avatarData.SizeInMB() > MonaBrainConstants.AVATAR_MAXIMUM_FILESIZE_MB)
                        {
                            Debug.LogError($"VRM file is above our maximum supported size ({MonaBrainConstants.AVATAR_MAXIMUM_FILESIZE_MB} Megabytes)");
                            callback?.Invoke(null);
                            return;
                        }

                        var glbData = new GlbBinaryParser(avatarData, "_character")
                            .Parse();

                        if (glbData == null)
                        {
                            Debug.LogError("Failed to parse VRM file, mesh not a parsable GLB");
                            callback?.Invoke(null);
                            return;
                        }

                        try
                        {
                            var vrmData = new VRMData(glbData);

                            var context = new VRMImporterContext(vrmData);

                            var runtimeGltfInstance = context.Load();
                            runtimeGltfInstance.EnableUpdateWhenOffscreen();
                            runtimeGltfInstance.ShowMeshes();
                            var avatarObject = runtimeGltfInstance.Root;

                            glbData.Dispose();
                            context.Dispose();

                            Used[url].Add(avatarObject);

                            var glb = avatarObject.AddComponent<BrainsGlb>();
                            glb.Url = url;
                            glb.OnDestroyed += HandleDestroyed;

                            if (poolSize > 1)
                            {
                                for (var i = 1; i < poolSize - 1; i++)
                                {
                                    InstantiatedGLTFObject instantiated = glb.GetComponent<InstantiatedGLTFObject>();
                                    var glbInstance = instantiated.Duplicate();
                                    glbInstance.GetComponent<BrainsGlb>().OnDestroyed += HandleDestroyed;
                                    ReturnToPool(url, glbInstance.gameObject);
                                }
                            }

                            if (returnToPool)
                                ReturnToPool(url, avatarObject);

                            Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} load from url {url}");
                            callback?.Invoke(avatarObject);
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} error loading VRM: {e.Message}");
                            System.IO.MemoryStream stream = new System.IO.MemoryStream(avatarData);
                            
                            try
                            {

                                var gltfComponent = gameObject.GetOrAddComponent<GLTFStreamComponent>();
                                gltfComponent.GLTFStream = stream;
                                gltfComponent.Collider = GLTFSceneImporter.ColliderType.None;
                                gltfComponent.ImportNormals = GLTFImporterNormals.Import;
                                gltfComponent.ImportTangents = GLTFImporterNormals.Import;

                                if (importAnimation)
                                {
                                    gltfComponent.AnimationMethod = AnimationMethod.MecanimHumanoid;
                                    gltfComponent.AnimationLoopPose = true;
                                }

                                gltfComponent.OnImportComplete = (obj, info) =>
                                {
                                    Debug.Log($"{nameof(BrainsGlbLoader)} {obj == null}");

                                    if (obj == null)
                                        throw (info.SourceException);

                                    if (obj.transform.childCount == 1 && obj.transform.GetChild(0).GetComponent<IMonaBody>() != null)
                                    {
                                        InstantiatedGLTFObject instantiated = obj.GetComponent<InstantiatedGLTFObject>();
                                        obj = obj.transform.GetChild(0).gameObject;
                                        obj.transform.parent = instantiated.transform.parent;
                                        obj.AddComponent<InstantiatedGLTFObject>().CachedData = instantiated.CachedData;
                                        DestroyImmediate(instantiated.gameObject);
                                    }

                                    Used[url].Add(obj);

                                    Debug.Log($"{nameof(BrainsGlbLoader)} add BrainsGlb component {obj}");

                                    var glb = obj.AddComponent<BrainsGlb>();
                                    glb.Url = url;
                                    glb.OnDestroyed += HandleDestroyed;

                                    if (poolSize > 1)
                                    {
                                        for (var i = 0; i < poolSize - 1; i++)
                                        {
                                            Debug.Log($"{nameof(BrainsGlbLoader)} create new instance and pool");
                                            InstantiatedGLTFObject instantiated = glb.GetComponent<InstantiatedGLTFObject>();
                                            var glbInstance = instantiated.Duplicate();
                                            glbInstance.GetComponent<BrainsGlb>().OnDestroyed += HandleDestroyed;
                                            ReturnToPool(url, glbInstance.gameObject);
                                        }
                                    }

                                    Debug.Log($"{nameof(BrainsGlbLoader)} return to pool {returnToPool} {obj}");
                                    if (returnToPool)
                                        ReturnToPool(url, obj);

                                    Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} load from url (not vrm) {url}");
                                    callback?.Invoke(obj);
                                };

                                gltfComponent.Load();
                            }
                            catch(Exception e2)
                            {
                                Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} error loading glb {e2.Message}");
                                callback?.Invoke(null);
                            }
                        }

                    });
                }
                else
                {
                    var inst = Used[url].Find(x => x != null && x.gameObject != null);
                    if (inst == null)
                        inst = Pool[url].Find(x => x != null && x.gameObject != null);
                    if (inst != null)
                    {
                        Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} instantiate from pool source {url}");
                        InstantiatedGLTFObject instantiated = inst.GetComponent<InstantiatedGLTFObject>();
                        if(instantiated != null)
                            callback?.Invoke(instantiated.Duplicate().gameObject);
                        else
                            callback?.Invoke(Instantiate(inst));
                    }
                    else
                    {
                        Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} cannot find instance {url}");
                    }
                }
            }
            else
            {
                Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} fetched from pool {url}");
                callback?.Invoke(instance);
            }
        }

        public void GetVrmData(string url, Action<byte[]> callback)
        {
            try
            {
                StartCoroutine(DoGetVrmData(url, callback));
            }
            catch(Exception e)
            {
                Debug.LogError($"{nameof(GetVrmData)} Exception Occured {e.Message}");
                callback(null);
            }
        }

        public IEnumerator DoGetVrmData(string url, Action<byte[]> callback)
        {
            var request = UnityWebRequest.Get(url);
            byte[] data;

            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"{nameof(BrainsGlbLoader)}.{nameof(GetVrmData)} - Request error: {request.error} {request.result} {url}");
                    data = null;
                    break;
                case UnityWebRequest.Result.Success:
                    data = request.downloadHandler.data;
                    break;
                default:
                    Debug.LogError($"{nameof(BrainsGlbLoader)}.{nameof(GetVrmData)} - Request error: {request.error}");
                    data = null;
                    break;
            }

            request.Dispose();
            callback?.Invoke(data);
        }
    }
}