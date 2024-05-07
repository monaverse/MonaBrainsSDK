using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Utils
{
    [Serializable]
    public class RegisteredContract
    {
        public string Address;
        public int StartToken;
        public int TokenCount;
        public string TokenType;
        public bool Active;
        public bool RequireOwnership = true;
        public string Chain;
    }

    public class MonaBrainBlockchain : MonoBehaviour, IMonaBrainBlockchain
    {
        [SerializeField]
        protected string _walletAddress;

        public virtual void SetWalletAddress(string address) => _walletAddress = address;

        [SerializeField]
        protected List<RegisteredContract> _contracts = new List<RegisteredContract>();
        public List<RegisteredContract> Contracts { get => _contracts; }

        protected bool _walletConnected;
        public bool WalletConnected { get => _walletConnected; set => _walletConnected = value; }

        public virtual void RegisterContract(string address, int tokenCount = -1, string tokenType = "ERC1155")
        {
            if (_contracts.Find(x => x.Address == address) == null)
                _contracts.Add(new RegisteredContract() { Address = address, TokenCount = tokenCount, TokenType = tokenType });
        }

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

        public virtual Task<List<Token>> OwnsTokensWithAvatar()
        {
            return null;
        }

        public virtual Task<List<Token>> OwnsTokensWithArtifact()
        {
            return null;
        }

        public virtual Task<List<Token>> OwnsTokensWithTexture()
        {
            return null;
        }

        public virtual Task<List<Token>> OwnsTokensWithTrait(string trait, string value)
        {
            return null;
        }

        public virtual Task<List<Token>> OwnsTokensWithTrait(string trait, float value)
        {
            return null;
        }
    }
}
