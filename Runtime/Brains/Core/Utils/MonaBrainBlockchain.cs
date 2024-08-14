using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Utils
{
    public enum MonaSDKFilterType
    {
        Trait=0,
        Contract=1
    }

    [Serializable]
    public class MonaSDKFilter
    {
        public MonaSDKFilterType FilterType;
        public bool Exclude;
        public string Contract;
        public string TraitName;
        public string TraitValue;
    }

    public class MonaBrainBlockchain : MonoBehaviour, IMonaBrainBlockchain
    {
        [SerializeField]
        protected string _walletAddress;

        [SerializeField]
        public List<MonaSDKFilter> Filters = new List<MonaSDKFilter>();

        public virtual void SetWalletAddress(string address) => _walletAddress = address;

        protected bool _walletConnected;
        public bool WalletConnected { get => _walletConnected; set => _walletConnected = value; }

        protected string _walletUsername;
        public string WalletUsername { get => _walletUsername; }

        protected string _walletUserEmail;
        public string WalletUserEmail { get => _walletUserEmail; }

        public virtual Task<Token> OwnsToken(string collectionAddress, string tokenId)
        {
            return default;
        }

        public virtual Task<List<Token>> OwnsTokens(string collectionAddress)
        {
            return null;
        }

        public virtual Task<List<Token>> OwnsTokens()
        {
            return null;
        }
    }
}
