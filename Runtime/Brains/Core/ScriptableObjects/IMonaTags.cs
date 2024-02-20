using System.Collections.Generic;

namespace Mona.SDK.Brains.Core.ScriptableObjects
{
    public interface IMonaTagItem
    {
        public string Tag { get; set;  }
        public bool IsPlayerTag { get; set; }
        public bool IsEditable { get; }
    }

    public interface IMonaTags
    {
        List<IMonaTagItem> AllTags { get; }
        List<string> Tags { get; }
        IMonaTagItem GetTag(string tag);
        bool HasTag(string tag);
    }
}
