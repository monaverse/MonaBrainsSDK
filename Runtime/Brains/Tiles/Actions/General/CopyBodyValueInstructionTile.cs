using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using System.Text;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CopyBodyValueInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "CopyBodyValue";
        public const string NAME = "Copy Body Value";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CopyBodyValueInstructionTile);

        [SerializeField] private TargetBodyType _body = TargetBodyType.Self;
        [BrainPropertyEnum(true)] public TargetBodyType Body { get => _body; set => _body = value; }

        [SerializeField] private string _tag;
        [BrainPropertyShow(nameof(Body), (int)TargetBodyType.Tag)]
        [BrainPropertyMonaTag(true)] public string Tag { get => _tag; set => _tag = value; }

        [SerializeField] private MonaBodyValueType _source = MonaBodyValueType.Position;
        [BrainPropertyEnum(true)] public MonaBodyValueType Source { get => _source; set => _source = value; }

        [SerializeField] private VectorThreeAxis _axis = VectorThreeAxis.Y;
        [BrainPropertyShow(nameof(AxisDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyEnum(true)]
        public VectorThreeAxis Axis { get => _axis; set => _axis = value; }

        [SerializeField] private TargetVariableType _targetType;
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Velocity)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.StartScale)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.StartRotation)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.StartPosition)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Scale)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Rotation)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Position)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.Forward)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.ChildIndex)]
        [BrainPropertyShow(nameof(Source), (int)MonaBodyValueType.SiblingIndex)]
        [BrainPropertyEnum(false)] public TargetVariableType TargetType { get => _targetType; set => _targetType = value; }

        [SerializeField] string _targetValue;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Vector3)]
        [BrainPropertyValue(typeof(IMonaVariablesVector3Value), true)] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        [SerializeField] private string _targetNumber;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.Number)]
        [BrainPropertyValue(typeof(IMonaVariablesFloatValue), true)] public string TargetNumber { get => _targetNumber; set => _targetNumber = value; }

        [SerializeField] private string _targetString;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.String)]
        [BrainPropertyValue(typeof(IMonaVariablesStringValue), true)] public string TargetString { get => _targetString; set => _targetString = value; }

        [SerializeField] private StringCopyType _copyType;
        [BrainPropertyShow(nameof(TrueTargetType), (int)TargetVariableType.String)]
        [BrainPropertyEnum(false)]
        public StringCopyType CopyType { get => _copyType; set => _copyType = value; }        

        public TargetVariableType TrueTargetType
        {
            get
            {
                switch (Source)
                {
                    case MonaBodyValueType.ChildCount:
                        return TargetVariableType.Number;
                    case MonaBodyValueType.PlayerId:
                        return TargetVariableType.Number;
                    case MonaBodyValueType.ClientId:
                        return TargetVariableType.Number;
                    case MonaBodyValueType.PlayerName:
                        return TargetVariableType.String;
                    case MonaBodyValueType.ReadMe:
                        return TargetVariableType.String;
                }

                return _targetType;
            }
        }

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

        public enum UIDisplayType
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
            LastSpawnedByMe = 80,
            MyPoolPreviouslySpawned = 100,
            MyPoolNextSpawned = 110
        }

        private IMonaBrain _brain;

        public UIDisplayType AxisDisplay
        {
            get
            {
                if (_source != MonaBodyValueType.ChildIndex && _source != MonaBodyValueType.ChildCount && _source != MonaBodyValueType.SiblingIndex)
                {
                    if (TargetType == TargetVariableType.Number && _source != MonaBodyValueType.Velocity)
                        return UIDisplayType.Show;
                    else if (TargetType == TargetVariableType.String && CopyType == StringCopyType.SingleAxis)
                        return UIDisplayType.Show;
                }

                return UIDisplayType.Hide;
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
                return Complete(InstructionTileResult.Success);

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
                case MonaBodyValueType.ChildIndex:
                    SetVariable((float)body.ChildIndex); break;
                case MonaBodyValueType.SiblingIndex:
                    SetVariable((float)body.ActiveTransform.GetSiblingIndex()); break;
                case MonaBodyValueType.ChildCount:
                    SetVariable((float)body.Transform.childCount); break;
                case MonaBodyValueType.PlayerId:
                    SetVariable((float)body.PlayerId); break;
                case MonaBodyValueType.ClientId:
                    SetVariable((float)body.ClientId); break;
                case MonaBodyValueType.PlayerName:
                    SetVariable(body.PlayerName); break;
                case MonaBodyValueType.ReadMe:
                    var runner = body.Transform.GetComponent<IMonaBrainRunner>();
                    if (runner != null)
                    {
                        var readme = new StringBuilder();
                        for (var i = 0; i < runner.BrainInstances.Count; i++)
                        {
                            if (i > 0 && !string.IsNullOrEmpty(runner.BrainInstances[i].ReadMe.Replace(" ", ""))) readme.Append("\n");
                            readme.Append(runner.BrainInstances[i].ReadMe);
                        }
                        SetVariable(readme.ToString());
                    }
                    break;
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

        private void SetVariable(float result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.Vector3:
                    _brain.Variables.Set(_targetValue, result);
                    break;
                case TargetVariableType.Number:
                    _brain.Variables.Set(_targetNumber, result);
                    break;
                case TargetVariableType.String:
                    _brain.Variables.Set(_targetString, result.ToString());
                    break;
            }
        }

        private void SetVariable(Vector3 result)
        {
            switch (TrueTargetType)
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

        private void SetVariable(string result)
        {
            switch (TrueTargetType)
            {
                case TargetVariableType.String:
                    _brain.Variables.Set(_targetString, result);
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