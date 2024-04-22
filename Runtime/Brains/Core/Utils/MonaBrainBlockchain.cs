using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Utils
{
    public class MonaBrainBlockchain : MonoBehaviour, IMonaBrainBlockchain
    {
        [SerializeField]
        protected string _walletAddress;

        public virtual void SetWalletAddress(string address) => _walletAddress = address;

        protected List<string> _contracts = new List<string>();
        public List<string> Contracts { get => _contracts; }

        public virtual void RegisterContract(string address)
        {
            if (!_contracts.Contains(address))
                _contracts.Add(address);
        }

        public async virtual Task<Token> OwnsToken(string collectionAddress, string tokenId)
        {
            return default;
        }

        public async virtual Task<List<Token>> OwnsTokens(string collectionAddress)
        {
            return null;
        }

        public virtual List<Token> OwnsTokensWithAvatar()
        {
            throw new System.NotImplementedException();
        }

        public virtual List<Token> OwnsTokensWithObject()
        {
            throw new System.NotImplementedException();
        }

        public virtual List<Token> OwnsTokensWithTexture()
        {
            throw new System.NotImplementedException();
        }

        public virtual List<Token> OwnsTokensWithTrait(string trait, string value)
        {
            throw new System.NotImplementedException();
        }

        public virtual List<Token> OwnsTokensWithTrait(string trait, float value)
        {
            throw new System.NotImplementedException();
        }
    }
}
