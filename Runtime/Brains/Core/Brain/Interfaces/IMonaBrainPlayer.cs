using Mona.SDK.Brains.Core.Brain.Structs;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Network.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Brain.Interfaces
{
    public interface IMonaBrainPlayer
    {
        public IMonaBody PlayerBody { get; }
        public IMonaBody PlayerCameraBody { get; }
        public Camera PlayerCamera { get; }
        public int PlayerId { get; }
        public IMonaNetworkSettings NetworkSettings { get; }
        public List<MonaRemotePlayer> OtherPlayers { get; }
        public int GetPlayerIdByBody(IMonaBody body);
    }
}