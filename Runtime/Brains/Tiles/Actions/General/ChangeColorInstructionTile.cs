﻿using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Tiles.Actions.Movement.Enums;
using UnityEngine;
using System;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Core.Utils;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.General
{

    [Serializable]
    public class ChangeColorInstructionTile : InstructionTile, IChangeColorInstructionTile, IActionInstructionTile, INeedAuthorityInstructionTile,
        IActivateInstructionTile, IPauseableInstructionTile, IProgressInstructionTile
    {
        public const string ID = "ChangeColor";
        public const string NAME = "Change Color";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(ChangeColorInstructionTile);

        public virtual MoveDirectionType DirectionType => MoveDirectionType.Forward;

        [SerializeField] private MonaBrainTargetColorType _target = MonaBrainTargetColorType.ThisBodyOnly;
        [BrainPropertyEnum(true)] public MonaBrainTargetColorType Target { get => _target; set => _target = value; }

        [SerializeField] private string _targetTag;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.Tag)]
        [BrainPropertyMonaTag(true)] public string TargetTag { get => _targetTag; set => _targetTag = value; }

        [SerializeField] private Color _color = Color.white;
        [BrainProperty(true)] public Color Color { get => _color; set => _color = value; }

        [SerializeField] private float _duration = 1f;
        [SerializeField] private string _durationValueName = null;

        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.ThisBodyOnly)]
        [BrainProperty(false)] public float Duration { get => _duration; set => _duration = value; }
        [BrainPropertyValueName("Duration", typeof(IMonaVariablesFloatValue))] public string DurationValueName { get => _durationValueName; set => _durationValueName = value; }

        [SerializeField] private EasingType _easing = EasingType.EaseInOut;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.ThisBodyOnly)]
        [BrainPropertyEnum(false)] public EasingType Easing { get => _easing; set => _easing = value; }

        [SerializeField] private bool _includeAttached = false;
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.Tag)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.MessageSender)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.OnConditionTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.OnSelectTarget)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.MySpawner)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.LastSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.AllSpawnedByMe)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.MyPoolPreviouslySpawned)]
        [BrainPropertyShow(nameof(Target), (int)MonaBrainTargetColorType.MyPoolNextSpawned)]
        [BrainProperty(false)] public bool IncludeAttached { get => _includeAttached; set => _includeAttached = value; }

        private Vector3 _direction;

        private IMonaBrain _brain;
        private string _progressName;


        private Color _start;
        private Color _end;

        private Action<MonaBodyFixedTickEvent> OnFixedTick;
        private Action<MonaInputEvent> OnInput;

        private float _speed
        {
            get => _brain.Variables.GetFloat(MonaBrainConstants.SPEED_FACTOR);
        }

        private MovingStateType _movingState;
        private bool _active;
        private MonaInput _brainInput;

        private Light[] _lights;

        public Vector2 InputMoveDirection
        {
            get => _brainInput.MoveValue;
        }

        public float Progress
        {
            get => _brain.Variables.GetFloat(_progressName);
            set => _brain.Variables.Set(_progressName, value);
        }

        public bool InProgress
        {
            get
            {
                var progress = Progress;
                if (_instruction.CurrentTile != this) return false;
                return progress > 0 && progress <= 1f;
            }
        }

        public ChangeColorInstructionTile() { }
        
        public void Preload(IMonaBrain brainInstance, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brainInstance;
            _instruction = instruction;
            _lights = _brain.Body.Transform.GetComponentsInChildren<Light>();

            var pagePrefix = page.IsCore ? "Core" : ("State" + brainInstance.StatePages.IndexOf(page));
            var instructionIndex = page.Instructions.IndexOf(instruction);

            _progressName = $"__{pagePrefix}_{instructionIndex}_progress";

            _brain.Variables.GetFloat(_progressName);

            UpdateActive();
        }

        private bool ModifyAllAttached
        {
            get
            {
                switch (_target)
                {
                    case MonaBrainTargetColorType.Self:
                        return false;
                    case MonaBrainTargetColorType.GlobalFog:
                        return false;
                    case MonaBrainTargetColorType.GlobalShadows:
                        return false;
                    case MonaBrainTargetColorType.GlobalAmbience:
                        return false;
                    case MonaBrainTargetColorType.CameraBackground:
                        return false;
                    case MonaBrainTargetColorType.Parent:
                        return false;
                    case MonaBrainTargetColorType.Parents:
                        return false;
                    case MonaBrainTargetColorType.Children:
                        return false;
                    case MonaBrainTargetColorType.ThisBodyOnly:
                        return false;
                    default:
                        return _includeAttached;
                }
            }
        }

        public void SetActive(bool active)
        {
            if (_active != active)
            {
                _active = active;
                if (_brain != null)
                    UpdateActive();
            }
        }

        private void UpdateActive()
        {
            if (!_active)
            {
                RemoveDelegates();
                return;
            }

            if (_movingState == MovingStateType.Moving)
            {
                AddDelegates();
            }

            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(UpdateActive)} {_active}");
        }


        public override void Unload(bool destroy = false)
        {
            RemoveDelegates();
            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(Unload)}");
        }

        public void Pause()
        {
            RemoveDelegates();
            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ChangeColorInstructionTile)}.{nameof(Pause)}");
        }

        public bool Resume()
        {
            UpdateActive();
            return _movingState == MovingStateType.Moving;
        }

        private void HandleBodyInput(MonaInputEvent evt)
        {
            _brainInput = evt.Input;
        }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            RemoveDelegates();
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }


        private void AddDelegates()
        {
            OnFixedTick = HandleFixedTick;
            MonaEventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);

            if (DirectionType == MoveDirectionType.UseInput)
            {
                OnInput = HandleBodyInput;
                MonaEventBus.Register<MonaInputEvent>(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
            }
        }

        private void RemoveDelegates()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _brain.Body), OnFixedTick);
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.INPUT_EVENT, _brain.Body), OnInput);
        }

        private List<IMonaBody> _bodiesToControl = new List<IMonaBody>();
        public List<IMonaBody> GetBodiesToControl()
        {
            if (_bodiesToControl.Count == 0)
                _bodiesToControl.Add(_brain.Body);
            return _bodiesToControl;
        }

        public InstructionTileResult Continue()
        {
            Debug.Log($"{nameof(Continue)} take over control and continue executing brain at {Progress}, {_progressName} on ", _brain.Body.ActiveTransform.gameObject);
            _movingState = MovingStateType.Moving;
            _start = _brain.Body.GetColor();
            _end = _color;
            AddDelegates();
            return Do();
        }

        private IMonaBody _body;
        private Light _light;
        private Image _image;
        private Text _text;
        private TMP_Text _textTmp;
        private TextMeshProUGUI _textUI;

        private void SetColor(Color c, IMonaBody body)
        {
            if (_body != body)
            {
                _body = body;
                _light = body.Transform.GetComponent<Light>();
                _image = body.Transform.GetComponent<Image>();
                _text = body.Transform.GetComponent<Text>();
                _textTmp = body.Transform.GetComponent<TMP_Text>();
                _textUI = body.Transform.GetComponent<TMPro.TextMeshProUGUI>();
            }

            if (body.Renderers.Length > 0)
                body.SetColor(_color, true);

            if (_light != null)
                _light.color = c;

            if (_image != null)
                _image.color = c;

            if (_text != null)
                _text.color = c;

            if (_textTmp != null)
                _textTmp.color = c;

            if (_textUI != null)
                _textUI.color = c;

            //else if(_lights.Length > 0)
            //{
            //    for (var i = 0; i < _lights.Length; i++)
            //        _lights[i].color = c;
            //}
        }

        public override InstructionTileResult Do()
        {
            if (!_brain.Body.HasControl()) return InstructionTileResult.WaitingForAuthority;

            if (!string.IsNullOrEmpty(_durationValueName))
                _duration = _brain.Variables.GetFloat(_durationValueName);

            if (_target != MonaBrainTargetColorType.ThisBodyOnly || _duration == 0)
            {
                SetColorOnTarget();
                return Complete(InstructionTileResult.Success);
            }

            if (_movingState == MovingStateType.Stopped)
            {
                Progress = 0;
                _start = _brain.Body.GetColor();
                _end = _color;
                AddDelegates();
            }

            _movingState = MovingStateType.Moving;
            return Complete(InstructionTileResult.Running);
        }

        private void HandleFixedTick(MonaBodyFixedTickEvent evt)
        {
            FixedTick(evt.DeltaTime);
        }

        private void FixedTick(float deltaTime)
        {
            if (!_brain.Body.HasControl())
            {
                if (_movingState == MovingStateType.Moving)
                    LostControl();
                return;
            }

            MoveOverTime(deltaTime);
        }

        private void LostControl()
        {
            Debug.Log($"{nameof(ChangeColorInstructionTile)} {nameof(LostControl)}");
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.LostAuthority, true);
        }

        private float Evaluate(float t)
        {
            switch(_easing)
            {
                case EasingType.EaseInOut:
                    return -((Mathf.Cos(Mathf.PI * t) - 1f) / 2f);
                case EasingType.EaseIn:
                    return 1f - Mathf.Cos((t * Mathf.PI) / 2f);
                case EasingType.EaseOut:
                    return Mathf.Sin((t * Mathf.PI) / 2f);
                default:
                    return t;
            }
        }

        private void MoveOverTime(float deltaTime)
        {
            if (_movingState == MovingStateType.Moving)
            {
                if(Progress >= 1f)
                {
                    SetColor(_end, _brain.Body);
                    StopMoving();
                }
                else
                {
                    SetColor(Color.Lerp(_start, _end, Evaluate(Progress)), _brain.Body);
                }
                Progress += deltaTime / _duration;
            }
        }

        private void StopMoving()
        {
            _movingState = MovingStateType.Stopped;
            Complete(InstructionTileResult.Success, true);
        }

        private void SetColorOnTarget()
        {
            switch (_target)
            {
                case MonaBrainTargetColorType.Tag:
                    SetColorOnTag();
                    break;
                case MonaBrainTargetColorType.Self:
                    SetColorOnWholeEntity(_brain.Body);
                    break;
                case MonaBrainTargetColorType.GlobalFog:
                    RenderSettings.fogColor = _color;
                    break;
                case MonaBrainTargetColorType.GlobalShadows:
                    RenderSettings.subtractiveShadowColor = _color;
                    break;
                case MonaBrainTargetColorType.GlobalAmbience:
                    RenderSettings.ambientSkyColor = _color;
                    break;
                case MonaBrainTargetColorType.Parents:
                    SetColorOnParents(_brain.Body);
                    break;
                case MonaBrainTargetColorType.Children:
                    SetColorOnChildren(_brain.Body);
                    break;
                case MonaBrainTargetColorType.AllSpawnedByMe:
                    SetColorOnAllSpawned();
                    break;
                default:
                    IMonaBody targetBody = GetTarget();

                    if (targetBody == null)
                        break;

                    if (ModifyAllAttached)
                        SetColorOnWholeEntity(targetBody);
                    else
                        SetColor(_color, targetBody);
                    break;
            }
        }

        private IMonaBody GetTarget()
        {
            switch (_target)
            {
                case MonaBrainTargetColorType.Parent:
                    return _brain.Body.Parent;
                case MonaBrainTargetColorType.ThisBodyOnly:
                    return _brain.Body;
                case MonaBrainTargetColorType.MessageSender:
                    var brain = _brain.Variables.GetBrain(MonaBrainConstants.RESULT_SENDER);
                    if (brain != null)
                        return brain.Body;
                    break;
                case MonaBrainTargetColorType.OnConditionTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_TARGET);
                case MonaBrainTargetColorType.OnSelectTarget:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_HIT_TARGET);
                case MonaBrainTargetColorType.MySpawner:
                    return _brain.Body.Spawner;
                case MonaBrainTargetColorType.LastSpawnedByMe:
                    return _brain.Variables.GetBody(MonaBrainConstants.RESULT_LAST_SPAWNED);
                case MonaBrainTargetColorType.MyPoolPreviouslySpawned:
                    return _brain.Body.PoolBodyPrevious;
                case MonaBrainTargetColorType.MyPoolNextSpawned:
                    return _brain.Body.PoolBodyNext;
            }
            return null;
        }

        private void SetColorOnTag()
        {
            var tagBodies = MonaBody.FindByTag(_targetTag);

            if (tagBodies.Count < 1)
                return;

            for (int i = 0; i < tagBodies.Count; i++)
            {
                if (tagBodies[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetColorOnWholeEntity(tagBodies[i]);
                else
                    SetColor(_color, tagBodies[i]);
            }
        }

        private void SetColorOnWholeEntity(IMonaBody body)
        {
            IMonaBody topBody = body;
            while (topBody.Parent != null)
                topBody = topBody.Parent;

            SetColor(_color, topBody);
            SetColorOnChildren(topBody);
        }

        private void SetColorOnParents(IMonaBody body)
        {
            IMonaBody parent = body.Parent;

            if (parent == null)
                return;

            SetColor(_color, parent);
            SetColorOnParents(parent);
        }

        private void SetColorOnChildren(IMonaBody body)
        {
            var children = body.Children();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == null)
                    continue;

                SetColor(_color, children[i]);
                SetColorOnChildren(children[i]);
            }
        }

        private void SetColorOnAllSpawned()
        {
            var spawned = _brain.SpawnedBodies;

            for (int i = 0; i < spawned.Count; i++)
            {
                if (spawned[i] == null)
                    continue;

                if (ModifyAllAttached)
                    SetColorOnWholeEntity(spawned[i]);
                else
                    SetColor(_color, spawned[i]);
            }
        }
    }
}