#if BRAINS_NORMCORE
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Network.Interfaces;
using System;

namespace Mona.Networking
{
    public interface INetworkMonaBodyServer
    {
        bool Active { get; }
        MonaBody MonaBody { get; }
        void SetSpaceNetworkSettings(IMonaNetworkSettings settings);
        void SetMonaBody(MonaBody monaBody);
        void PlayerLeft(int player);
        void RegisterNetworkVariables(INetworkMonaVariables variables);
    }
}
#endif