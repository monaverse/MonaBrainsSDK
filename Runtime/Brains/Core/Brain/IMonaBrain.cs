using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.ScriptableObjects;
using Mona.SDK.Brains.Core.State;
using System;
using System.Collections.Generic;
using UnityEngine;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Core.Assets.Interfaces;

namespace Mona.SDK.Brains.Core.Brain
{
    public interface IMonaBrain : IMonaTagged
    {
        event Action<string, IMonaBrain> OnStateChanged;
        event Action OnMigrate;

        Guid Guid { get; set; }

        string Name { get; set; }
        MonaBrainPropertyType PropertyType { get; set; }

        string LocalId { get; }
        int Priority { get; }
        string BrainState { get; set;  }
        bool LoggingEnabled { get; set; }

        IMonaBrainPlayer Player { get; }

        IMonaBrainVariables Variables { get; }
        IMonaBody Body { get; }
        GameObject GameObject { get; }

        IMonaBrainPage CorePage { get; }
        List<IMonaBrainPage> StatePages { get; }
        
        IMonaBrainVariables DefaultVariables { get; }

        List<string> MonaTags { get; }
        List<IMonaAssetProvider> MonaAssets { get; }

        IMonaAssetItem GetMonaAsset(string id);
        List<IMonaAssetItem> GetAllMonaAssets();

        Transform Root { get; }

        void AddTag(string tag);
        void RemoveTag(string tag);
        bool HasPlayerTag();
        bool HasPlayerTag(List<string> monaTags);

        bool HasMessage(string message);
        void SetMonaBrainPlayer(IMonaBrainPlayer player);
        MonaBroadcastMessageEvent GetMessage(string message);

        void Preload(GameObject gameObject, IMonaBrainRunner runner, int index);
        void Begin();
        void Pause();
        void Resume();
        void Unload();

        IInstructionTileSet TileSet { get; set; }
        IMonaTags MonaTagSource { get; set; }
    }
}
