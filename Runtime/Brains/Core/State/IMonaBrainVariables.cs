using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Network;
using Mona.SDK.Core.Network.Interfaces;
using Mona.SDK.Core.State;
using UnityEngine;
using System;

namespace Mona.SDK.Brains.Core.State
{
    public interface IMonaBrainVariables : IMonaVariables
    {
        event Action OnStateAuthorityChanged;

        INetworkMonaVariables NetworkVariables { get; }

        void SetGameObject(GameObject gameObject, IMonaBrain brain);

        void Set(string variableName, IMonaBrain value);
        void SetInternal(string variableName, Vector3 value);

        IMonaBrain GetBrain(string variableName);
        Vector3 GetInternalVector3(string variableName);

        void SetNetworkVariables(INetworkMonaVariables state);
        bool HasControl();

        void SyncValuesOnNetwork();

        void SaveResetDefaults();

    }

}