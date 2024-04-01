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

            new MonaTagItem(MonaBrainConstants.TAG_GAMECONTROLLER, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_CAMERA, true, false),
            
            new MonaTagItem(MonaBrainConstants.TAG_FRIENDLY, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_NEUTRAL, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_ENEMY, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_MINION, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_BOSS, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_TEAM_A, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_TEAM_B, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_TEAM_C, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_TEAM_D, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_CARNIVORE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_HERBIVORE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_OMNIVORE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_VEGITATION, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_CORPSE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_VEHICLE, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_WEAPON, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_DOOR, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_WINDOW, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_LOCK, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_STAIRS, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_TABLE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_CHAIR, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_PICTURE, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_SPAWNER, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_SPAWNED_OBJECT, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_COLLECTIBLE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_KEYITEM, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_COIN, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_KEY, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_HEART, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_POWERUP, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_TREASURE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_INVENTORYITEM, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_FOOD, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_BALL, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_FLAG, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_TRIGGER_VOLUME, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_GROUND, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_WATER, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_HAZARD, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_LAVA, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_FIRE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_ICE, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_WIND, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_POISON, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_SPIKES, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_TRAP, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_DEATHPLANE, false, false),

            new MonaTagItem(MonaBrainConstants.TAG_START, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_GOAL, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_CHECKPOINT, false, false),
            new MonaTagItem(MonaBrainConstants.TAG_WAYPOINT, false, false),

            new MonaTagItem(HumanBodyBones.Head.ToString(), true, false),

            new MonaTagItem(HumanBodyBones.LeftEye.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightEye.ToString(), true, false),

            new MonaTagItem(HumanBodyBones.Neck.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Chest.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Spine.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.Hips.ToString(), true, false),

            new MonaTagItem(HumanBodyBones.LeftShoulder.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftUpperArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftLowerArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftHand.ToString(), true, false),

            new MonaTagItem(HumanBodyBones.RightShoulder.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightUpperArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightLowerArm.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightHand.ToString(), true, false),

            new MonaTagItem(HumanBodyBones.LeftUpperLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftLowerLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftFoot.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.LeftToes.ToString(), true, false),

            new MonaTagItem(HumanBodyBones.RightUpperLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightLowerLeg.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightFoot.ToString(), true, false),
            new MonaTagItem(HumanBodyBones.RightToes.ToString(), true, false),

            
        };

        private IMonaTagItem _default = new MonaTagItem("Default", false, false);

        public List<IMonaTagItem> AllTags => _tags;

        public List<string> Tags {
            get {
                return _tags.ConvertAll<string>(x => x == null ? "Default" : x.Tag);
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
