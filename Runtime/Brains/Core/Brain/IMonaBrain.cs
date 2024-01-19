﻿using Mona.Core.Body;
using Mona.Brains.Core.Control;
using Mona.Brains.Core.Enums;
using Mona.Brains.Core.ScriptableObjects;
using Mona.Brains.Core.State;
using Mona.Core.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mona.Brains.Core.Events;

namespace Mona.Brains.Core.Brain
{
    public interface IMonaBrain : IMonaTagged
    {
        event Action<string, IMonaBrain> OnStateChanged;
        event Action OnMigrate;

        string Name { get; set; }
        MonaBrainPropertyType PropertyType { get; set; }

        string LocalId { get; }
        int Priority { get; }
        string BrainState { get; set;  }

        IMonaBrainPlayer Player { get; }

        IMonaBrainState State { get; }
        IMonaBody Body { get; }
        GameObject GameObject { get; }

        IMonaBrainPage CorePage { get; }
        List<IMonaBrainPage> StatePages { get; }
        
        IMonaBrainState DefaultState { get; }

        List<string> MonaTags { get; }

        bool HasMessage(string message);
        void SetMonaBrainPlayer(IMonaBrainPlayer player);
        MonaBroadcastMessageEvent GetMessage(string message);

        void Preload(GameObject gameObject);
        void Begin();
        void Unload();

        IInstructionTileSet TileSet { get; set; }
        IMonaTags MonaTagSource { get; set; }
    }
}
