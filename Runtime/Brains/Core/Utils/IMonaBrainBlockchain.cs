

using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public enum TokenAssetType
    {
        Avatar=0,
        Object=1,
        Texture=2
    }

    public struct TokenAsset
    {
        public TokenAssetType AssetType;
        public string AssetUri;
    }

    public struct TokenMetadata
    {

    }

    public struct Token
    {
        public string Address;
        public TokenMetadata Metadata;
        public List<TokenAsset> Assets;
    }

    public interface IMonaBrainBlockchain
    {
        List<Token> OwnsTokens(string collectionAddress);
        List<Token> OwnsTokensWithAvatar();
        List<Token> OwnsTokensWithObject();
        List<Token> OwnsTokensWithTexture();
        List<Token> OwnsTokensWithTrait(string trait, string value);
        List<Token> OwnsTokensWithTrait(string trait, float value);
    }
}