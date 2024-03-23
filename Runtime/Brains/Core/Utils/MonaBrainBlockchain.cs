using System.Collections;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Utils
{
    public class MonaBrainBlockchain : MonoBehaviour, IMonaBrainBlockchain
    {

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
