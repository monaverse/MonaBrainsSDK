using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.Network.Interfaces;
using Mona.SDK.Core.State;
using UnityEngine;

namespace Mona.SDK.Brains.Core.State
{
    public interface IMonaBrainVariables : IMonaVariables
    {
        void SetGameObject(GameObject gameObject, IMonaBrain brain);

        void Set(string variableName, IMonaBrain value);

        IMonaBrain GetBrain(string variableName);

        void SetNetworkVariables(INetworkMonaVariables state);

        void SyncValuesOnNetwork();
    }

}