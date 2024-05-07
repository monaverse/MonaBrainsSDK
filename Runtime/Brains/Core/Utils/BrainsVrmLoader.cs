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

    public sealed class BrainsVrmLoader : MonoBehaviour
    {
        public static Dictionary<string, GameObject> PoolSource = new Dictionary<string, GameObject>();
        public static Dictionary<string, List<GameObject>> Pool = new Dictionary<string, List<GameObject>>();

        public void ReturnToPool(string url, GameObject instance)
        {
            if (!Pool.ContainsKey(url))
                Pool[url] = new List<GameObject>();

            if(!Pool[url].Contains(instance))
                Pool[url].Add(instance);

            instance.transform.SetParent(transform, true);
            instance.SetActive(false);
            Debug.Log($"{nameof(ReturnToPool)} {url}", instance.gameObject);
        }

        public GameObject GetFromPool(string url)
        {
            if (!Pool.ContainsKey(url))
                Pool[url] = new List<GameObject>();

            if (Pool[url].Count > 0)
            {
                var instance = Pool[url][0];
                Pool[url].RemoveAt(0);
                instance.SetActive(true);
                Debug.Log($"{nameof(GetFromPool)} {url}", instance.gameObject);
                return instance;
            }
            return null;
        }

        public void Load(string url, bool importAnimation, Action<GameObject> callback, int poolSize = 1)
        {
            Debug.Log($"{nameof(Load)} VRM: {url}");

            var instance = GetFromPool(url);
            if (instance == null)
            {
                if (!PoolSource.ContainsKey(url))
                {
                    GetVrmData(url, (byte[] avatarData) =>
                    {
                        Debug.Log($"{nameof(BrainsVrmLoader)} {url} loaded from url and created new");

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

                            PoolSource[url] = avatarObject;

                            glbData.Dispose();
                            context.Dispose();

                            if (poolSize > 1)
                            {
                                for (var i = 0; i < poolSize; i++)
                                    ReturnToPool(url, Instantiate(avatarObject));
                            }

                            Debug.Log($"{nameof(BrainsVrmLoader)} {nameof(Load)} load from url {url}");
                            callback?.Invoke(avatarObject);
                        }
                        catch (Exception e)
                        {
                            Debug.Log($"could not load VRM data {e.Message}");
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

                            UnityGLTF.GLTFSceneImporter sceneImporter = new UnityGLTF.GLTFSceneImporter(gLTFRoot, stream, options);
                            sceneImporter.LoadScene(-1, true, (obj, info) =>
                            {
                                PoolSource[url] = obj;

                                if (poolSize > 1)
                                {
                                    for (var i = 0; i < poolSize; i++)
                                        ReturnToPool(url, Instantiate(obj));
                                }

                                Debug.Log($"{nameof(BrainsVrmLoader)} {nameof(Load)} load from url (not vrm) {url}");
                                callback?.Invoke(obj);
                            });
                        }

                    });
                }
                else
                {
                    Debug.Log($"{nameof(BrainsVrmLoader)} {nameof(Load)} instantiate from pool source {url}");
                    callback?.Invoke(Instantiate(PoolSource[url]));
                }
            }
            else
            {
                Debug.Log($"{nameof(BrainsVrmLoader)} {nameof(Load)} fetched from pool {url}");
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
                    Debug.LogError($"{nameof(BrainsVrmLoader)}.{nameof(GetVrmData)} - Request error: {request.error} {request.result} {url}");
                    data = null;
                    break;
                case UnityWebRequest.Result.Success:
                    data = request.downloadHandler.data;
                    break;
                default:
                    Debug.LogError($"{nameof(BrainsVrmLoader)}.{nameof(GetVrmData)} - Request error: {request.error}");
                    data = null;
                    break;
            }

            request.Dispose();
            callback?.Invoke(data);
        }
    }
}