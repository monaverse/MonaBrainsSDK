using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Mona.Networking
{
    public class BrainsNetworkPlayerService : MonoBehaviour
    {
        public virtual async Task<string> GetPlayerName()
        {
            return null;
        }

        public virtual async Task<string> GetRoomId()
        {
            return null;
        }

        public virtual async Task<string> GetRoomName()
        {
            return null;
        }
    }
}
