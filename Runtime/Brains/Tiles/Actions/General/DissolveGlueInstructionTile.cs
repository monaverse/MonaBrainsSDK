using Mona.Brains.Core.Enums;
using Mona.Brains.Core.Tiles;
using Mona.Brains.Core;
using UnityEngine;
using System;
using Mona.Brains.Core.Brain;
using Mona.Core.Body.Enums;
using Mona.Core.Body;
using Mona.Brains.Tiles.Actions.General.Interfaces;

namespace Mona.Brains.Tiles.Actions.General
{
    [Serializable]
    public class DissolveGlueInstructionTile : InstructionTile, IDissolveGlueInstructionTile, IActionInstructionTile
    {
        public const string ID = "DissolveGlue";
        public const string NAME = "Dissolve Glue";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(DissolveGlueInstructionTile);

        [SerializeField]
        private string _target;
        [BrainProperty]
        public string Target { get => _target; set => _target = value; }

        [SerializeField]
        private MonaPlayerBodyParts _part;
        [BrainPropertyEnum(false)]
        public MonaPlayerBodyParts Part { get => _part; set => _part = value; }

        [SerializeField]
        private Vector3 _offset;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        [SerializeField]
        private bool _letFall;
        [BrainProperty(false)]
        public bool LetFall { get => _letFall; set => _letFall = value; }

        [SerializeField]
        private bool _isListening;
        [BrainProperty(false)]
        public bool IsListening { get => _isListening; set => _isListening = value; }

        private IMonaBrain _brain;

        private MonaPlayerBodyParts _lastPart;
        private string _lastPartString;

        private bool _stateGlued
        {
            get => _brain.State.GetBool(MonaBrainConstants.RESULT_GLUED);
            set => _brain.State.Set(MonaBrainConstants.RESULT_GLUED, value);
        }

        public DissolveGlueInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            IMonaBody body;
            if (_part != MonaPlayerBodyParts.None)
            {
                var originPlayer = _brain.Player;
                if (originPlayer.PlayerBody == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_PLAYER);

                body = originPlayer.PlayerBody.FindChildByTag(GetPartString(_part));
                if (body == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_PART);
            }
            else
            {
                body = _target != null ? _brain.State.GetBody(_target) : _brain.Body;
                if (body == null) return Complete(InstructionTileResult.Failure, MonaBrainConstants.ERROR_MISSING_TARGET);
            }

            _stateGlued = false;

            body.SetLayer("LocalPlayer", true, true);
            body.SetPosition(body.ActiveTransform.position + body.ActiveTransform.rotation * _offset, true, true);
            body.SetRotation(body.ActiveTransform.rotation, true, true);
            body.SetScale(_scale, true);
            if (_letFall) body.SetKinematic(false, true);

            return Complete(InstructionTileResult.Success);
        }

        private string GetPartString(MonaPlayerBodyParts part)
        {
            //reduce garbage generation to one frame
            if (_lastPart != part)
            {
                _lastPart = part;
                _lastPartString = part.ToString();
            }
            return _lastPartString;
        }
    }
}