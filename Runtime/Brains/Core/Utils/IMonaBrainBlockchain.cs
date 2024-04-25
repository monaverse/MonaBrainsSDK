

using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IMonaBrainBlockchain
    {
        void SetWalletAddress(string address);
        void RegisterContract(string address, int tokenCount = -1, string tokenType = "ERC1155");
        Task<Token> OwnsToken(string collectionAddress, string tokenId);
        Task<List<Token>> OwnsTokens(string collectionAddress);
        Task<List<Token>> OwnsTokensWithAvatar();
        Task<List<Token>> OwnsTokensWithObject();
        Task<List<Token>> OwnsTokensWithTexture();
        Task<List<Token>> OwnsTokensWithTrait(string trait, string value);
        Task<List<Token>> OwnsTokensWithTrait(string trait, float value);
    }
}