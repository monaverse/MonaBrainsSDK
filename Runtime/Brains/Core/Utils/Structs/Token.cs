
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Core.Utils.Structs
{
    public record TokenFile
    {
        public string Url { get; set; }
        public string Filetype { get; set; }
    }

    public struct Token
    {
        public string Id;
        public string Contract;
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
        public TokenAssetType AssetType;
        public string AssetUrl;
        public List<TokenFile> Files;

        public override bool Equals(object obj)
        {
            if (!(obj is Token)) return false;
            return base.Equals(obj);
        }

        public bool Equals(Token other)
        {
            return Id == other.Id;
        }
    }
}