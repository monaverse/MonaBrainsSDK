using Mona.SDK.Core.Body.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mona.SDK.Brains.Core.ScriptableObjects
{
    [Serializable]
    public class MonaTagItem : IMonaTagItem
    {
        [SerializeField] public string _tag;
        [SerializeField] public bool _isPlayerTag;
        [SerializeField] public bool _isEditable = true;

        public MonaTagItem(string tag, bool isPlayerTag, bool isEditable)
        {
            _tag = tag;
            _isPlayerTag = isPlayerTag;
            _isEditable = isEditable;
        }

        public string Tag { get => _tag; set => _tag = value; }
        public bool IsPlayerTag { get => _isPlayerTag; set => _isPlayerTag = value; }
        public bool IsEditable { get => _isEditable; }
    }

    [CreateAssetMenu(menuName = "Mona Brains/Utils/Mona Tags")]
    public class MonaTags : ScriptableObject, IMonaTags
    {
        [SerializeReference]
        private List<IMonaTagItem> _tags = new List<IMonaTagItem>()
        {
        new MonaTagItem(MonaBrainConstants.TAG_DEFAULT, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_PLAYER, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_REMOTE_PLAYER, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_CAMERA, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_HEAD, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_EYES, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_TORSO, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_HIPS, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_LEFT_HAND, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_RIGHT_HAND, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_LEFT_FOOT, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_RIGHT_FOOT, true, false),
            new MonaTagItem(MonaBrainConstants.TAG_ENEMY, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_FRIENDLY, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_COLLECTIBLE, false, false),
        };

        private IMonaTagItem _default = new MonaTagItem("Default", false, false);

        public List<IMonaTagItem> AllTags => _tags;

        public List<string> Tags {
            get {
                return _tags.ConvertAll<string>(x => x.Tag);
            }
        }

        public IMonaTagItem GetTag(string tag)
        {
            for(var i =0;i < _tags.Count;i++)
            {
                if (_tags[i].Tag == tag)
                    return _tags[i];
            }
            return _default;
        }

    }
}
