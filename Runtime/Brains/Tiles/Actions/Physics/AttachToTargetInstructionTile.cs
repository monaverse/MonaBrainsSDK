﻿using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Core.Body.Enums;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Brains.Tiles.Actions.Physics.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.Physics
{
    [Serializable]
    public class AttachToTargetInstructionTile : InstructionTile, IAttachToTargetInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile
    {
        public const string ID = "AttachToTarget";
        public const string NAME = "Attach To Target";
        public const string CATEGORY = "Physics";
        public override Type TileType => typeof(AttachToTargetInstructionTile);

        [SerializeField] private MonaBrainTargetResultType _source = MonaBrainTargetResultType.OnConditionTarget;
        [SerializeField] private string _target;

        [BrainProperty(true)] public MonaBrainTargetResultType Source { get => _source; set => _source = value; }
        [BrainPropertyValueName("Source", typeof(IMonaVariablesBodyValue))] public string Target { get => _target; set => _target = value; }

        [SerializeField]
        private Vector3 _offset = Vector3.zero;
        [BrainProperty(false)]
        public Vector3 Offset { get => _offset; set => _offset = value; }

        [SerializeField]
        private Vector3 _scale = Vector3.one;
        [BrainProperty(false)]
        public Vector3 Scale { get => _scale; set => _scale = value; }

        [SerializeField]
        private bool _pinDontParent = false;
        [BrainProperty(false)]
        public bool PinDontParent { get => _pinDontParent; set => _pinDontParent = value; }

        private IMonaBrain _brain;

        public AttachToTargetInstructionTile() { }

        public void Preload(IMonaBrain brainInstance) 
        {
            _brain = brainInstance;
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            IMonaBody body = GetTarget();

            if (body != null)
            {
                if (_brain.HasPlayerTag(body.MonaTags))
                    _brain.Body.SetLayer(MonaCoreConstants.LAYER_LOCAL_PLAYER, true);
                _brain.Body.SetScale(_scale, true);

                if (_pinDontParent)
                {
                    _brain.Body.PinToParent(body.ActiveTransform, () => {
                        if (body.ActiveTransform.parent != null)
                            return body.ActiveTransform.position + body.ActiveTransform.parent.TransformDirection(_offset);
                        else
                            return body.ActiveTransform.position + _offset;

                    }, () => body.ActiveTransform.rotation);
                }
                else
                    _brain.Body.SetTransformParent(body.ActiveTransform);

                if(body.ActiveTransform.parent != null)
                    _brain.Body.TeleportPosition(body.ActiveTransform.position + body.ActiveTransform.parent.TransformDirection(_offset), true);
                else
                    _brain.Body.TeleportPosition(body.ActiveTransform.position + _offset, true);

                _brain.Body.SetRotation(body.ActiveTransform.rotation, true);
            }
            return Complete(InstructionTileResult.Success);
        }

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }

        private IMonaBody GetTarget()
        {
            var body = GetSource();
            if (!string.IsNullOrEmpty(_target))
            {
                var variable = _brain.Variables.GetVariable(_target);
                if (variable is IMonaVariablesBrainValue)
                    body = ((IMonaVariablesBrainValue)variable).Value.Body;
                else if (variable is IMonaVariablesBodyValue)
                    body = ((IMonaVariablesBodyValue)variable).Value;
            }
            return body;
        }

        private IMonaBody GetSource()
        {
            switch (_source)
            {
                case MonaBrainTargetResultType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainTargetResultType.OnMessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainTargetResultType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
            }
            return null;
        }
    }
}