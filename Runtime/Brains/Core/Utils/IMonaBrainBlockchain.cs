

using System.Collections.Generic;
using System.Threading.Tasks;
using Mona.SDK.Brains.Core.Utils.Structs;

namespace Mona.SDK.Brains.Core.Utils.Interfaces
{
    public interface IMonaBrainBlockchain
    {
        bool WalletConnected { get; }
        void SetWalletAddress(string address);
        Task<Token> OwnsToken(string collectionAddress, string tokenId);
        Task<List<Token>> OwnsTokens(string collectionAddress);
        Task<List<Token>> OwnsTokens();
    }
}