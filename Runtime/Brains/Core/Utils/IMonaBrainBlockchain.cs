

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public enum TokenAssetType
    {
        Avatar = 0,
        Object = 1,
        Texture = 2
    }

    public struct TokenAttribute
    {
        public string TraitType;
        public string Value;
        public string DisplayType;
    }

    public struct TokenMetadata
    {
        public string Id;
        public string Image;
        public string ImageData;
        public string ExternaUrl;
        public string Description;
        public string Name;
        public TokenAttribute[] Attributes;
        public string BackgroundColor;
        public string AnimationUrl;
        public string YoutubeUrl;
    }

    public struct TokenArtifact
    {
        public TokenAssetType AssetType;
        public string Uri;
    }

    public struct TokenAvatar
    {
        public string Uri;
    }

    public struct TokenNft
    {
        public TokenMetadata Metadata;

        public int Amount;
        public string Contract;
        public string IpfsUrl;
        public string LegacyContract;
        public string LegacyTokenId;
        public string Network;
        public string TokenHash;
        public string TokenId;
        public string TokenStandard;
        public string TokenUri;
        public string TransactionId;
    }

    public struct Token
    {
        public string Accessibility;
        public string ActiveVersion;
        public string Artist;
        public bool IsChecked;
        public string CollectionId;
        public string Creator;
        public string CreatorId;
        public string Description;
        public string DocumentId;
        public bool Hidden;
        public string Image;
        public string LastSaleEventId;
        public string LastSalePrice;
        public bool Minted;
        public TokenNft Nft;
        public bool Nsfw;
        public string Owner;
        public string[] Owners;
        public string ParentId;
        public string Price;
        public string Promoted;
        public Dictionary<string, object> Properties;
        public string Slug;
        public string SubCollectionId;
        public Dictionary<string, object> Traits;
        public string Versions;

        public List<TokenArtifact> Artifacts;
        public List<TokenAvatar> Avatars;
         
    }

    public interface IMonaBrainBlockchain
    {
        void SetWalletAddress(string address);
        void RegisterContract(string address);
        List<Token> OwnsTokens(string collectionAddress);
        List<Token> OwnsTokensWithAvatar();
        List<Token> OwnsTokensWithObject();
        List<Token> OwnsTokensWithTexture();
        List<Token> OwnsTokensWithTrait(string trait, string value);
        List<Token> OwnsTokensWithTrait(string trait, float value);
    }
}