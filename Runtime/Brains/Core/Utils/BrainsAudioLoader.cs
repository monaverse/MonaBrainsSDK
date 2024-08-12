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
    public sealed class BrainsAudioLoader : MonoBehaviour
    {
        
        public void GetAudioData(string url, Action<AudioClip> callback)
        {
            try
            {
                StartCoroutine(DoGetAudioData(url, callback));
            }
            catch(Exception e)
            {
                Debug.LogError($"{nameof(GetAudioData)} Exception Occured {e.Message}");
                callback(null);
            }
        }

        public IEnumerator DoGetAudioData(string url, Action<AudioClip> callback)
        {
            AudioClip clip = null;
            // Start the download
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(www.error);
                }
                else
                {
                    // Get the downloaded audio clip
                    clip = DownloadHandlerAudioClip.GetContent(www);
                }
                www.Dispose();
            }
            
            callback?.Invoke(clip);
        }
    }
}