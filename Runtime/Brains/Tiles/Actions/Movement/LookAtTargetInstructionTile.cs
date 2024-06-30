using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System;
using UnityEngine;

namespace Mona.SDK.Brains.Tiles.Actions.Movement
{
    [Serializable]
    public class LookAtTargetInstructionTile : RotateLocalInstructionTile
    {
        public const string ID = "LookAtTarget";
        public const string NAME = "Look At Target";
        public const string CATEGORY = "Rotation";
        public override Type TileType => typeof(LookAtTargetInstructionTile);

        public override RotateDirectionType DirectionType => RotateDirectionType.None;

        [SerializeField] private MonaBrainTargetResultType _source = MonaBrainTargetResultType.OnConditionTarget;
        [SerializeField] private string _target;

        [BrainProperty(true)] public MonaBrainTargetResultType Source { get => _source; set => _source = value; }
        [BrainPropertyValueName("Source", typeof(IMonaVariablesBodyValue))] public string Target { get => _target; set => _target = value; }

        [BrainProperty(false)] public bool LookStraightAhead { get => _lookStraightAhead; set => _lookStraightAhead = value; }

        [SerializeField] private Vector3 _targetValue;
        [SerializeField] private string[] _targetvalueValueName = new string[4];
        [BrainPropertyShow(nameof(Source), (int)MonaBrainTargetResultType.OnHitTarget)]
        [BrainProperty(true)] public Vector3 TargetValue { get => _targetValue; set => _targetValue = value; }
        [BrainPropertyValueName(nameof(TargetValue), typeof(IMonaVariablesVector3Value))] public string[] TargetValueValueName { get => _targetvalueValueName; set => _targetvalueValueName = value; }

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

        private Quaternion _look;
        protected override Quaternion GetDirectionRotation(RotateDirectionType moveType, float angle, float diff, float progress, bool immediate)
        {
            if (_source == MonaBrainTargetResultType.OnHitTarget && HasVector3Values(_targetvalueValueName))
            {
                var targetVector = GetVector3Value(_brain, _targetvalueValueName);

                var fwd = targetVector - _brain.Body.GetPosition();
                if (_lookStraightAhead)
                    fwd.y = 0;

                var rot = _brain.Body.GetRotation();
                if (immediate)
                {
                    _look = Quaternion.LookRotation(fwd, Vector3.up);
                    _brain.Body.TeleportRotation(_look, true);
                    return Quaternion.identity;
                }
                else
                {
                    if (progress == 0f)
                        _look = Quaternion.LookRotation(fwd, Vector3.up);
                    _brain.Body.SetRotation(Quaternion.Inverse(rot));
                    return Quaternion.RotateTowards(rot, _look, angle);
                }
            }


            IMonaBody body = GetTarget();
            if (body != null)
            {
                var fwd = body.GetPosition() - _brain.Body.GetPosition();
                if (_lookStraightAhead)
                    fwd.y = 0;

                var rot = _brain.Body.GetRotation();
                if (immediate)
                {
                    _look = Quaternion.LookRotation(fwd, Vector3.up);
                    _brain.Body.TeleportRotation(_look, true);
                    return Quaternion.identity;
                }
                else
                {
                    if (progress == 0f)
                        _look = Quaternion.LookRotation(fwd, Vector3.up);
                    _brain.Body.SetRotation(Quaternion.Inverse(rot));
                    return Quaternion.RotateTowards(rot, _look, angle);
                }
            }
            else
            {
                return _brain.Body.GetRotation();
            }            
        }
    }
}