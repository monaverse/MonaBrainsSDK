using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CopyBodyValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "CopyBodyValue";
        public const string NAME = "Copy Body Value";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CopyBodyValueInstructionTile);

        private TargetBodyType _body = TargetBodyType.Self;
        [BrainPropertyEnum(true)] public TargetBodyType Body { get => _body; set => _body = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(Body), (int)TargetBodyType.Tag)]
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private MonaBodyValueType _source = MonaBodyValueType.Position;
        [BrainPropertyEnum(true)] public MonaBodyValueType Source { get => _source; set => _source = value; }

        [SerializeField] private VectorThreeAxis _axis = VectorThreeAxis.Y;
        [BrainPropertyShow(nameof(AxisDisplay), (int)AxisDisplayType.Show)]
        [BrainPropertyEnum(true)]
        public VectorThreeAxis Axis { get => _axis; set => _axis = value; }

        [SerializeField] private TargetVariableType _targetType;
        [BrainPropertyEnum(false)]
        [SerializeField] public TargetVariableType TargetType { get => _targetType; set => _targetType = value; }

        [SerializeField] string _targetValue;
        [BrainPropertyShow(nameof(TargetType), (int)TargetVariableType.Vector3)]
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        [SerializeField] private string _targetNumber;
        [BrainPropertyShow(nameof(TargetType), (int)TargetVariableType.Number)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string TargetNumber { get => _targetNumber; set => _targetNumber = value; }

        [SerializeField] private string _targetString;
        [BrainPropertyShow(nameof(TargetType), (int)TargetVariableType.String)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string TargetString { get => _targetString; set => _targetString = value; }

        private StringCopyType _copyType;
        [BrainPropertyShow(nameof(TargetType), (int)TargetVariableType.String)]
        [BrainPropertyEnum(false)]
        public StringCopyType CopyType { get => _copyType; set => _copyType = value; }        

        public enum TargetVariableType
        {
            Vector3 = 0,
            Number = 10,
            String = 20
        }

        public enum StringCopyType
        {
            Vector3 = 0,
            SingleAxis = 10
        }

        public enum AxisDisplayType
        {
            Show = 0,
            Hide = 10
        }

        public enum TargetBodyType
        {
            Tag = 0,
            Self = 10,
            Parent = 20,
            MessageSender = 40,
            OnConditionTarget = 50,
            OnHitTarget = 60,
            MySpawner = 70,
            LastSpawnedByMe = 80
        }

        private IMonaBrain _brain;

        public AxisDisplayType AxisDisplay
        {
            get
            {
                if (TargetType == TargetVariableType.Number && _source != MonaBodyValueType.Velocity)
                    return AxisDisplayType.Show;
                else if (TargetType == TargetVariableType.String && CopyType == StringCopyType.SingleAxis)
                    return AxisDisplayType.Show;

                return AxisDisplayType.Hide;
            }
        }

        public CopyBodyValueInstructionTile() { }

        public void Preload(IMonaBrain brain) => _brain = brain;

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            IMonaBody body = GetBody();

            if (body == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            switch (_source)
            {
                case MonaBodyValueType.StartPosition:
                    SetVariable(body.InitialPosition); break;
                case MonaBodyValueType.Rotation:
                    SetVariable(body.GetRotation().eulerAngles); break;
                case MonaBodyValueType.StartRotation:
                    SetVariable(body.InitialRotation.eulerAngles); break;
                case MonaBodyValueType.Scale:
                    SetVariable(body.GetScale()); break;
                case MonaBodyValueType.StartScale:
                    SetVariable(body.InitialScale); break;
                case MonaBodyValueType.Velocity:
                    SetVelocity(); break;
                case MonaBodyValueType.Forward:
                    SetVariable(body.ActiveTransform.forward); break;
                default:
                    SetVariable(body.GetPosition()); break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private IMonaBody GetBody()
        {
            switch (_body)
            {
                case TargetBodyType.Tag:
                    var tagBodies = MonaBody.FindByTag(_tag);
                    return tagBodies.Count > 0 ? tagBodies[0] : null;
                case TargetBodyType.Self:
                    return _brain.Body;
                case TargetBodyType.Parent:
                    return _brain.Body.Parent;
                case TargetBodyType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    return brain != null ? brain.Body : null;
                case TargetBodyType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case TargetBodyType.OnHitTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case TargetBodyType.MySpawner:
                    return _brain.Body.Spawner;
                case TargetBodyType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                default:
                    return null;
            }
        }
        

        private void SetVariable(Vector3 result)
        {
            switch (_targetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetValue, result);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, GetAxisValue(result));
                    break;
                case TargetVariableType.String:
                    if (_copyType == StringCopyType.Vector3)
                        _brain.Variables.Set(_targetString, result.ToString());
                    else
                        _brain.Variables.Set(_targetString, GetAxisValue(result).ToString());
                    break;
            }
        }

        private float GetAxisValue(Vector3 result)
        {
            switch (_axis)
            {
                case VectorThreeAxis.X:
                    return result.x;
                case VectorThreeAxis.Y:
                    return result.y;
                default:
                    return result.z;
            }
        }

        private void SetVelocity()
        {
            Vector3 velocity = _brain.Body.GetVelocity();
            float velocityMagnitude = velocity.magnitude;

            switch (_targetType)
            {
                case TargetVariableType.Vector3:
                    SetVariable(velocity);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, velocityMagnitude);
                    break;
                case TargetVariableType.String:
                    if (_copyType == StringCopyType.Vector3)
                        _brain.Variables.Set(_targetString, velocity.ToString());
                    else
                        _brain.Variables.Set(_targetString, velocityMagnitude.ToString());
                    break;
            }
        }
    }
}