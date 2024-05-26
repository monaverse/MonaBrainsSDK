using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core;
using UnityEngine;

public static class Utils
{ 
    public static string CidToIpfsUrl(this string cid, bool useGateway = false)
    {
        string ipfsRaw = $"ipfs://{cid}";
        return useGateway ? ipfsRaw.ReplaceIPFS() : ipfsRaw;
    }

    public static string ReplaceIPFS(this string uri)
    {
        string gateway = MonaGlobalBrainRunner.Instance.DefaultIPFSGateway;
        if (gateway == null || string.IsNullOrEmpty(gateway))
            gateway = MonaCoreConstants.DEFAULT_IPFS_GATEWAY;
        if (!string.IsNullOrEmpty(uri) && uri.StartsWith("ipfs://"))
            return uri.Replace("ipfs://", gateway);
        else
            return uri;
    }

}