using System;
using System.Collections;
using System.Threading.Tasks;
using UniGLTF;
using UnityEngine;
using UnityEngine.Networking;
using VRM;
using UnityGLTF;
using UnityGLTF.Loader;

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
        private string _url;

        public void Load(string url, Action<GameObject> callback)
        {
            Debug.Log($"{nameof(Load)} VRM: {url}");
            _url = url;
            GetVrmData((byte[] avatarData) =>
            {
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
                    callback?.Invoke(avatarObject);
                }
                catch(Exception e)
                {
                    Debug.Log($"could not load VRM data {e.Message}");
                    System.IO.MemoryStream stream = new System.IO.MemoryStream(avatarData);
                    GLTF.Schema.GLTFRoot gLTFRoot;
                    GLTF.GLTFParser.ParseJson(stream, out gLTFRoot);

                    UnityGLTF.GLTFSceneImporter sceneImporter = new UnityGLTF.GLTFSceneImporter(gLTFRoot, stream, new ImportOptions()
                    {
                        DataLoader = new StreamLoader(stream),
                        AnimationMethod = AnimationMethod.MecanimHumanoid

                    });
                    sceneImporter.LoadScene(-1, true, (obj, info) =>
                    {
                        callback?.Invoke(obj);
                    });
                }

            });
        }

        public void GetVrmData(Action<byte[]> callback)
        {
            try
            {
                StartCoroutine(DoGetVrmData(callback));
            }
            catch(Exception e)
            {
                Debug.LogError($"{nameof(GetVrmData)} Exception Occured {e.Message}");
                callback(null);
            }
}

        public IEnumerator DoGetVrmData(Action<byte[]> callback)
        {
            var request = UnityWebRequest.Get(_url);
            byte[] data;

            yield return request.SendWebRequest();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError($"{nameof(BrainsVrmLoader)}.{nameof(GetVrmData)} - Request error: {request.error} {request.result} {_url}");
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