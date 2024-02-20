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

            new MonaTagItem(HumanBodyBones.Head.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Hips.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftUpperLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightUpperLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftLowerLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightLowerLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftFoot.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightFoot.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Spine.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Chest.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Neck.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Head.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftShoulder.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightShoulder.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftUpperArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightUpperArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftLowerArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightLowerArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftHand.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightHand.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftToes.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightToes.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftEye.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightEye.ToString(), true, false),

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

        public bool HasTag(string tag)
        {
            for (var i = 0; i < _tags.Count; i++)
            {
                if (_tags[i].Tag == tag)
                    return true;
            }
            return false;
        }
    }
}
