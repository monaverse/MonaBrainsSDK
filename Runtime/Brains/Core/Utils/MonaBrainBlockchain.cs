using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using UnityEngine;
using System;

namespace Mona.SDK.Brains.Core.Utils
{
    [Serializable]
    public class RegisteredContract
    {
        public string Address;
        public int TokenCount;
        public string TokenType;
    }

    public class MonaBrainBlockchain : MonoBehaviour, IMonaBrainBlockchain
    {
        [SerializeField]
        protected string _walletAddress;

        public virtual void SetWalletAddress(string address) => _walletAddress = address;

        [SerializeField]
        protected List<RegisteredContract> _contracts = new List<RegisteredContract>();
        public List<RegisteredContract> Contracts { get => _contracts; }

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

        public virtual Task<List<Token>> OwnsTokensWithAvatar()
        {
            return null;
        }

        public virtual Task<List<Token>> OwnsTokensWithObject()
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
