using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Utils
{
    public class MonaBrainBlockchain : MonoBehaviour, IMonaBrainBlockchain
    {
        protected string _walletAddress;

        public void SetWalletAddress(string address) => _walletAddress = address;

        protected List<string> _contracts = new List<string>();
        public List<string> Contracts { get => _contracts; }

        public virtual void RegisterContract(string address)
        {
            if (!_contracts.Contains(address))
                _contracts.Add(address);
        }

        public virtual List<Token> OwnsTokens(string collectionAddress)
        {
            throw new System.NotImplementedException();
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
