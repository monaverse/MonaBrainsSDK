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

        public void CacheTokens(List<Token> tokens, Action callback, int poolSize = 1)
        {
            for(var i = 0;i < tokens.Count; i++)
            {
                Load(tokens[i].AssetUrl, false, (obj) => {}, poolSize, true);
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
            Debug.Log($"{nameof(Load)} VRM: {url}");

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
                            return;
                        }
                        if (avatarData.SizeInMB() > MonaBrainConstants.AVATAR_MAXIMUM_FILESIZE_MB)
                        {
                            Debug.LogError($"VRM file is above our maximum supported size ({MonaBrainConstants.AVATAR_MAXIMUM_FILESIZE_MB} Megabytes)");
                            return;
                        }

                        var glbData = new GlbBinaryParser(avatarData, "_character")
                            .Parse();

                        if (glbData == null)
                        {
                            Debug.LogError("Failed to parse VRM file, mesh not a parsable GLB");
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
                                    var glbInstance = Instantiate(glb);
                                    glbInstance.OnDestroyed += HandleDestroyed;
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
                            GLTF.Schema.GLTFRoot gLTFRoot;
                            GLTF.GLTFParser.ParseJson(stream, out gLTFRoot);

                            var options = new ImportOptions()
                            {
                                DataLoader = new StreamLoader(stream),
                                AnimationMethod = AnimationMethod.None
                            };

                            if (importAnimation)
                            {
                                options.AnimationMethod = AnimationMethod.MecanimHumanoid;
                                options.AnimationLoopPose = true;
                            }

                            try
                            {
                                UnityGLTF.GLTFSceneImporter sceneImporter = new UnityGLTF.GLTFSceneImporter(gLTFRoot, stream, options);
                                sceneImporter.LoadScene(-1, true, (obj, info) =>
                                {
                                    Used[url].Add(obj);

                                    var glb = obj.AddComponent<BrainsGlb>();
                                    glb.Url = url;
                                    glb.OnDestroyed += HandleDestroyed;

                                    if (poolSize > 1)
                                    {
                                        for (var i = 0; i < poolSize - 1; i++)
                                        {
                                            var glbInstance = Instantiate(glb);
                                                glbInstance.OnDestroyed += HandleDestroyed;
                                            ReturnToPool(url, glbInstance.gameObject);
                                        }
                                    }

                                    if (returnToPool)
                                        ReturnToPool(url, obj);

                                    Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} load from url (not vrm) {url}");
                                    callback?.Invoke(obj);
                                });
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
                    Debug.Log($"{nameof(BrainsGlbLoader)} {nameof(Load)} instantiate from pool source {url}");
                    callback?.Invoke(Instantiate(Used[url][0]));
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