
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Core.Utils.Structs
{
    public record TokenFile
    {
        public string Url { get; set; }
        public string Filetype { get; set; }
    }

    public record Collection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public string Symbol { get; set; }
        public string ImageUrl { get; set; }
        public int TokenCount { get; set; }
        public string ContractDeployedAt { get; set; }
    }

    public struct Token
    {
        public string ChainId;
        public string Contract;
        public string TokenId;
        public string Kind;
        public string Name;
        public string Image;
        public string ImageSmall;
        public string ImageLarge;
        public string Description;
        public string Media;
        public Dictionary<string, object> Metadata;
        public float RarityScore;
        public int RarityRank;
        public int Supply;
        public Collection Collection;

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
            return ChainId == other.ChainId && Contract == other.Contract && TokenId == other.TokenId;
        }
    }
}