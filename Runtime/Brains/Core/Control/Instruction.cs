using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core;
using Mona.SDK.Core.Body;
using Mona.SDK.Core.Events;
using Mona.SDK.Core.Input;
using Mona.SDK.Core.Network.Enums;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Mona.SDK.Brains.Core.Control
{
    [Serializable]
    public class Instruction : IInstruction
    {
        private IMonaBrain _brain;
        private IMonaBrainPage _page;

        public event Action<IInstruction> OnReset = delegate { };
        public event Action<int> OnRefresh = delegate { };
        public event Action OnDeselect = delegate { };
        public event Action OnSelect = delegate { };

        private MonaInput _instructionInput;
        public MonaInput InstructionInput { get => _instructionInput; set => _instructionInput = value; }

        private List<IMonaBody> _instructionBodies = new List<IMonaBody>();
        public List<IMonaBody> InstructionBodies { get => _instructionBodies; set => _instructionBodies = value; }

        [SerializeReference]
        private List<IInstructionTile> _instructionTiles = new List<IInstructionTile>();
        public List<IInstructionTile> InstructionTiles => _instructionTiles;

        [SerializeField]
        private InstructionTileResult _result = InstructionTileResult.Success;
        public InstructionTileResult Result { get => _result; set => _result = value; }

        [SerializeField]
        private List<Token> _tokens = new List<Token>();
        public List<Token> Tokens { get => _tokens; set => _tokens = value; }

        public bool IsRunning() => _result == InstructionTileResult.Running;

        private List<IInstructionTile> _needAuthInstructionTiles = new List<IInstructionTile>();

        private bool _hasInputTile;
        private int _firstActionIndex = -1;
        private bool _unloaded;
        private bool _paused;
        private string _progressTile;
        private IInstructionTile _tileWaitingForAuthorization;

        [SerializeField] private bool _muted;
        public bool Muted { get => _muted; set => _muted = value; }

        private bool _hasMessageEventWaitingForAuthorization;
        private List<InstructionEvent> _messageEventsWaitingForAuthorization = new List<InstructionEvent>();

        public IInstructionTile CurrentTile {
            get
            {
                var index = (int)_brain.Variables.GetFloat(_progressTile);
                if (index > -1 && index < InstructionTiles.Count)
                    return InstructionTiles[index];
                else
                    return null;
            }
        }

        public Instruction()
        {
        }

        public void Deselect()
        {
            OnDeselect?.Invoke();
        }

        public void Select()
        {
            OnSelect?.Invoke();
        }

        public void Preload(IMonaBrain brain, IMonaBrainPage page)
        {
            _brain = brain;
            _page = page;
            _firstActionIndex = -1;
            _needAuthInstructionTiles.Clear();

            var pagePrefix = page.IsCore ? "Core" : ("State" + _brain.StatePages.IndexOf(page));
            var instructionIndex = page.Instructions.IndexOf(this);

            _progressTile = $"__{pagePrefix}_{instructionIndex}_tile";
            _brain.Variables.GetFloat(_progressTile);

            PreloadTiles();
            ResetExecutionLinks();
        }

        private void PreloadTiles()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                var tile = InstructionTiles[i];

                if (tile is INeedAuthorityInstructionTile)
                    _needAuthInstructionTiles.Add(tile);

                if (tile is IInstructionTileWithPreload)
                    ((IInstructionTileWithPreload)tile).Preload(_brain);
                else if (tile is IInstructionTileWithPreloadAndPage)
                    ((IInstructionTileWithPreloadAndPage)tile).Preload(_brain, _page);
                else if (tile is IInstructionTileWithPreloadAndPageAndInstruction)
                    ((IInstructionTileWithPreloadAndPageAndInstruction)tile).Preload(_brain, _page, this);

                if (tile is IInputInstructionTile)
                    _hasInputTile = true;

                if (tile is IActionInstructionTile)
                {
                    if (_firstActionIndex == -1)
                        _firstActionIndex = i;
                    PreloadActionTile((IInstructionTile)tile);
                }
            }
        }

        private void ResetExecutionLinks()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                var tile = InstructionTiles[i];
                if (i < InstructionTiles.Count - 1)
                    tile.NextExecutionTile = InstructionTiles[i + 1];
            }
        }

        public void SetActive(bool active)
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                var tile = InstructionTiles[i];
                if(tile is IActivateInstructionTile)
                {
                    ((IActivateInstructionTile)tile).SetActive(active);
                }
            }
            if (active)
                ResetExecutionLinks();
        }

        public bool HasConditional()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i] is IConditionInstructionTile)
                    return true;
            }
            return false;
        }

        public bool HasTickAfter()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i] is ITickAfterInstructionTile)
                    return true;
            }
            return false;
        }

        public bool HasAnimationTiles()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i] is IAnimationInstructionTile)
                    return true;
            }
            return false;
        }

        public bool HasRigidbodyTiles()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i] is IRigidbodyInstructionTile)
                    return true;
            }
            return false;
        }

        public bool HasUsePhysicsTileSetToTrue()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i].GetType() == typeof(Brains.Tiles.Actions.Physics.UsePhysicsInstructionTile))
                {
                    return ((Brains.Tiles.Actions.Physics.UsePhysicsInstructionTile)InstructionTiles[i]).Value;
                }
                    
            }
            return false;
        }

        private bool HasValidTriggerType(InstructionEvent evt)
        {
            for(var i = 0;i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i] is ITriggerInstructionTile && ((ITriggerInstructionTile)InstructionTiles[i]).TriggerTypes.Contains(evt.TriggerType))
                    return true;
            }
            return false;
        }

        private bool HasPlayerTriggeredConditional()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                var tile = InstructionTiles[i];
                if (tile is IPlayerTriggeredConditional && ((IPlayerTriggeredConditional)tile).PlayerTriggered)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasOnMessageTile(string message)
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                if (InstructionTiles[i] is IOnMessageInstructionTile && ((IOnMessageInstructionTile)InstructionTiles[i]).Message == message)
                    return true;
            }
            return false;
        }

        private List<IMonaBody> _waitForAuthBodies = new List<IMonaBody>();
        private Action<MonaStateAuthorityChangedEvent> OnStateAuthorityChanged;
        private bool HasTilesNeedingAuthority() => _needAuthInstructionTiles.Count > 0;
        private bool TakeControlIfHasTilesNeedingAuthority()
        {
            _waitForAuthBodies.Clear();
            var need = false;
            for(var i = 0;i < _needAuthInstructionTiles.Count; i++)
            {
                var tile = _needAuthInstructionTiles[i];
                var body = ((INeedAuthorityInstructionTile)tile).GetBodyToControl();
                if (body != null && !body.HasControl())
                {
                    OnStateAuthorityChanged = HandleStateAuthorityChanged;
                    EventBus.Register<MonaStateAuthorityChangedEvent>(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, body), OnStateAuthorityChanged);
                    _waitForAuthBodies.Add(body);

                    if (_brain.LoggingEnabled)
                        Debug.Log($"{nameof(Instruction)}.{nameof(TakeControlIfHasTilesNeedingAuthority)} {body.ActiveTransform.name}", body.ActiveTransform.gameObject);
                    body.TakeControl();

                    need = true;
                }
            }
            return need;
        }

        private void HandleStateAuthorityChanged(MonaStateAuthorityChangedEvent evt)
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.STATE_AUTHORITY_CHANGED_EVENT, evt.Body), OnStateAuthorityChanged);

            if(_waitForAuthBodies.Count == 0)
            {
                //Debug.Log($"{nameof(Instruction)}.{nameof(HandleStateAuthorityChanged)} extra auth changed event {evt.Body.ActiveTransform.name} {evt.Body.HasControl()}", evt.Body.ActiveTransform.gameObject);
                return;
            }

            _waitForAuthBodies.Remove(evt.Body);
            //if(_brain.LoggingEnabled)
                //Debug.Log($"{nameof(Instruction)}.{nameof(HandleStateAuthorityChanged)} {evt.Body.ActiveTransform.name} {evt.Body.HasControl()}", evt.Body.ActiveTransform.gameObject);
            if(_waitForAuthBodies.Count == 0)
            {
                //Debug.Log($"{nameof(Instruction)}.{nameof(HandleStateAuthorityChanged)} i now have necessary authority, send a tick");
                EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain.Body), new InstructionEvent(InstructionEventTypes.Authority));
            }
            else
            {
                //if (_brain.LoggingEnabled)
                   // Debug.Log($"{nameof(Instruction)}.{nameof(HandleStateAuthorityChanged)} waiting for {_waitForAuthBodies.Count} bodies to complete instruction", evt.Body.ActiveTransform.gameObject);
            }
        }

        private void PreloadActionTile(IInstructionTile tile)
        {
            var callback = new InstructionTileCallback();
            callback.Action = () =>
            {
                //Debug.Log($"Execute Next from Then callback {tile}");
                ExecuteActionTile(tile.NextExecutionTile);
                return _result;
            };
            tile.SetThenCallback(callback);
        }

        public void Execute(InstructionEventTypes eventType, InstructionEvent evt = default)
        {
            //Debug.Log($"{nameof(Execute)} #{_page.Instructions.IndexOf(this)} instruction received event {eventType}", _brain.Body.ActiveTransform.gameObject);
            if (_unloaded) return;
            if (_paused) return;
            if (_muted) return;

            if (_result == InstructionTileResult.WaitingForAuthority)
            {
                if (eventType != InstructionEventTypes.Authority)
                    return;
            }
            else if (IsRunning())
            {
               // if (_brain.LoggingEnabled)
                //    Debug.Log($"{nameof(Execute)} #{_page.Instructions.IndexOf(this)} instruction still running", _brain.Body.ActiveTransform.gameObject);

                /*if (!HasConditional())
                {
                    Debug.Log($"tick while runnin' {_result}");
                    EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new MonaBrainTickEvent(InstructionEventTypes.Tick));
                }*/

                return;
            }

            if (_instructionTiles.Count == 0)
                return;

            var result = ExecuteFirstTile(eventType, evt);
            if (result == InstructionTileResult.Success)
            {
                OnReset?.Invoke(this);
                if (ExecuteRemainingConditionals() == InstructionTileResult.Success)
                    ExecuteActions();
            }
            else if (result == InstructionTileResult.Failure && (!HasConditional() || HasTickAfter()))
            {
                //if(_brain.LoggingEnabled) Debug.Log($"TICK IT needed first file failed #{_page.Instructions.IndexOf(this)}  {_result} {Time.frameCount} {_instructionTiles[0]}", _brain.Body.Transform.gameObject);
                EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Tick, this));
            }

            ClearInputs();
        }

        private void ClearInputs()
        {
            if (_hasInputTile)
            {
                for (var i = 0; i < _instructionTiles.Count; i++)
                {
                    if (_instructionTiles[i] is IInputInstructionTile)
                        ((IInputInstructionTile)_instructionTiles[i]).ClearInput();
                }
            }
        }

        private InstructionTileResult ExecuteFirstTile(InstructionEventTypes eventType, InstructionEvent evt = default)
        {
            if (_unloaded) return InstructionTileResult.Failure;
            if (_paused) return InstructionTileResult.Failure;
            if (_muted) return InstructionTileResult.Failure;

            var tile = InstructionTiles[0];
            //if (tile is IConditionInstructionTile)
            {
                switch (eventType)
                {
                    case InstructionEventTypes.Start:
                    case InstructionEventTypes.State:
                        if (tile is IOnStartInstructionTile || tile is IStartableInstructionTile)
                        {
                            return ExecuteTile(tile);
                        }
                        else if (tile is IActionInstructionTile)
                        {
                            if (_brain.Body.HasControl())
                            {
                                //_result = InstructionTileResult.Running;
                                return ExecuteTile(tile);
                            }
                        }
                        break;
                    case InstructionEventTypes.Message:
                        if (tile is IOnMessageInstructionTile)
                            return ExecuteTile(tile);
                        break;
                    case InstructionEventTypes.Value:
                        if (tile is IOnValueChangedInstructionTile)
                            return ExecuteTile(tile);
                        break;
                    case InstructionEventTypes.Input:
                        if (_hasInputTile)
                            return ExecuteTile(tile);
                        break;
                    case InstructionEventTypes.Trigger:
                        if (HasValidTriggerType(evt))
                            return ExecuteTile(tile);
                        break;
                    case InstructionEventTypes.Tick:
                        if (_brain.Body.HasControl() && (tile is IActionInstructionTile || tile is ITickAfterInstructionTile) && (evt.Instruction == this))
                        {
                            //_result = InstructionTileResult.Running;
                            //Debug.Log($"execute tick");
                            ExecuteActionTile(tile);
                        }
                        break;
                    case InstructionEventTypes.Authority:
                        if(HasTilesNeedingAuthority())
                        {
                            if (_brain.Body.HasControl())
                                ResetExecutionLinks();

                            _result = InstructionTileResult.Running;

                            if (!HasConditional())
                            {
                                var progressTile = GetFirstTileInProgress();
                                if (progressTile != null)
                                {
                                    ContinueTile((IProgressInstructionTile)progressTile);
                                    return InstructionTileResult.Failure;
                                }
                                else
                                {
                                    ExecuteTile(tile);
                                    return InstructionTileResult.Failure;
                                }
                            }
                            else
                            {
                                ProcessMessageEventsIfNeeded();
                                ProcessWaitingInputIfNeeded();

                                if (_tileWaitingForAuthorization != null)
                                {
                                    var tileWaiting = _tileWaitingForAuthorization;
                                    _tileWaitingForAuthorization = null;
                                    ExecuteActionTile(tileWaiting);
                                    return InstructionTileResult.Failure;
                                }
                                else
                                {
                                    return ExecuteTile(tile);
                                }
                            }
                        }
                        break;
                }
            }
            return InstructionTileResult.Failure;
        }

        private void ProcessWaitingInputIfNeeded()
        {
            if (_hasInputWaitingForAuthorization)
            {
                _hasInputWaitingForAuthorization = false;
                for(var i = 0;i < _instructionTiles.Count; i++)
                {
                    var inputInstruction = _instructionTiles[i];
                    if(inputInstruction is IInputInstructionTile)
                    {
                        ((IInputInstructionTile)inputInstruction).ReprocessInput(_inputWaitingForAuthorization);
                        break;
                    }
                }
            }
        }

        private void ProcessMessageEventsIfNeeded()
        {
            if(_messageEventsWaitingForAuthorization.Count > 0)
            {
                for (var i = 0; i < _messageEventsWaitingForAuthorization.Count; i++)
                {
                    Debug.Log($"{nameof(Instruction)} rebroadcast message {_messageEventsWaitingForAuthorization[i].Message}", _brain.Body.Transform.gameObject);
                    EventBus.Trigger<InstructionEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _brain), _messageEventsWaitingForAuthorization[i]);
                }
            }
        }

        private InstructionTileResult ExecuteTile(IInstructionTile tile)
        {
            if (tile.Muted) return InstructionTileResult.Success;
            //Debug.Log($"{nameof(ExecuteTile)} instruction #{_page.Instructions.IndexOf(this)}, tile #{InstructionTiles.IndexOf(tile)} {tile.Name} current result {_result}", _brain.Body.Transform.gameObject);
            if (_brain.Body.HasControl())
                _brain.Variables.Set(_progressTile, (float)InstructionTiles.IndexOf(tile));

            return tile.Do();
        }

        private InstructionTileResult ContinueTile(IProgressInstructionTile tile)
        {
            if (_brain.Body.HasControl())
                _brain.Variables.Set(_progressTile, (float)InstructionTiles.IndexOf(tile));
            return tile.Continue();
        }

        private IProgressInstructionTile GetFirstTileInProgress()
        {
            for (var i = 0; i < InstructionTiles.Count; i++)
            {
                var tile = InstructionTiles[i];
                if (tile is IProgressInstructionTile)
                {
                    var progressTile = (IProgressInstructionTile)tile;
                    if (progressTile.InProgress)
                        return progressTile;
                }
            }
            return null;
        }

        private InstructionTileResult ExecuteRemainingConditionals()
        {
            for(var i = 1;i < InstructionTiles.Count;i++)
            {
                var tile = InstructionTiles[i];
                if (tile is IConditionInstructionTile)
                {
                    if (ExecuteTile(tile) == InstructionTileResult.Failure)
                        return InstructionTileResult.Failure;
                }
            }
            return InstructionTileResult.Success;
        }


        private bool _hasInputWaitingForAuthorization;
        private MonaInput _inputWaitingForAuthorization;

        private void ExecuteActions()
        {
            //if (_brain.LoggingEnabled)
            //    Debug.Log($"{nameof(ExecuteActions)} #{_page.Instructions.IndexOf(this)} start instruction", _brain.Body.ActiveTransform.gameObject);
            _result = InstructionTileResult.Running;
            if (_firstActionIndex == -1) return;
            var tile = InstructionTiles[_firstActionIndex];

            if (HasPlayerTriggeredConditional() && _brain.Player.NetworkSettings.GetNetworkType() == MonaNetworkType.Shared)
            {
                if (TakeControlIfHasTilesNeedingAuthority())
                {
                    //if(_brain.LoggingEnabled)
                    //Debug.Log($"{nameof(Instruction)}.{nameof(ExecuteActions)} i need authority. requesting control {_brain.Body.ActiveTransform.name}", _brain.Body.ActiveTransform.gameObject);

                    CacheTileWaitingForAuthorization();
                    CacheInputIfNeeded();
                    CacheLastMessageEventIfNeeded();

                    _result = InstructionTileResult.WaitingForAuthority;
                    return;
                }
            }

            if (InstructionTiles.Count == 1)
            {
                ExecuteActionTile(null);
            }
            else
            {
                ExecuteActionTile(tile);
            }
        }

        private void CacheTileWaitingForAuthorization()
        {
            if (_firstActionIndex > -1)
                _tileWaitingForAuthorization = _instructionTiles[_firstActionIndex];
        }

        private void CacheInputIfNeeded()
        {
            for (var i = 0; i < _instructionTiles.Count; i++)
            {
                var inputTile = _instructionTiles[i];
                if (inputTile is IInputInstructionTile)
                {
                    _hasInputWaitingForAuthorization = true;
                    _inputWaitingForAuthorization = ((IInputInstructionTile)inputTile).GetInput();
                    break;
                }
            }
        }

        private void CacheLastMessageEventIfNeeded()
        {
            _messageEventsWaitingForAuthorization.Clear();

            for (var i = 0; i < _instructionTiles.Count; i++)
            {
                var messageTile = _instructionTiles[i];
                if (messageTile is IOnMessageInstructionTile)
                {
                    _messageEventsWaitingForAuthorization.Add(((IOnMessageInstructionTile)messageTile).GetLastSuccessfulMessage());
                }
            }
        }

        private void ExecuteActionTile(IInstructionTile tile)
        {
            if (_unloaded) return;
            if (_paused) return;
            if (_muted) return;

            if (tile == null)
            {
                _result = InstructionTileResult.Success;
                _brain.Variables.Set(_progressTile, -1f);
                if (!HasConditional() || HasTickAfter())
                {
                    //if(HasTickAfter()) Debug.Log($"TICK IT success {_result} #{_page.Instructions.IndexOf(this)} ", _brain.Body.Transform.gameObject);
                    EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Tick, this));
                }
            }
            else
            {
                _result = ExecuteTile(tile);
                //Debug.Log($"{nameof(Instruction)} {nameof(ExecuteActionTile)} #{_page.Instructions.IndexOf(this)} tile #{InstructionTiles.IndexOf(tile)} current result {_result}");
                if (_result == InstructionTileResult.Success)
                {
                    //Debug.Log($"{nameof(ExecuteActionTile)} immediately execute next tile {tile.NextExecutionTile} {_result}");
                    ExecuteActionTile(tile.NextExecutionTile);
                }
                else if (_result == InstructionTileResult.Failure && (!HasConditional() || HasTickAfter()))
                {
                    //if (HasTickAfter() && _brain.LoggingEnabled) Debug.Log($"TICK IT failure {_result} #{_page.Instructions.IndexOf(this)} ", _brain.Body.Transform.gameObject);
                    EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _brain), new InstructionEvent(InstructionEventTypes.Tick, this));
                }
            }
        }

        public void AddTile(IInstructionTile tile, int i, IMonaBrainPage page)
        {
            var instance = (IInstructionTile)Activator.CreateInstance(tile.TileType);
            instance.Id = tile.Id;
            instance.Name = tile.Name;
            instance.Category = tile.Category;

            CopyBrainProperties(tile, instance);

            if(instance is IConditionInstructionTile)
            {
                var idx = InstructionTiles.FindLastIndex(x => x is IConditionInstructionTile);
                if(idx > -1)
                {
                    InstructionTiles.Insert(idx+1, instance);
                    Changed(idx+1);
                    return;
                }
                else
                {
                    InstructionTiles.Insert(0, instance);
                    Changed(0);
                    return;
                }
            }

            if (!page.IsCore)
            {
                if (instance is IActionStateEndInstructionTile || instance is IActionEndInstructionTile)
                {
                    if (HasEndTile(page)) return;
                    i = -1;
                }
                else
                {
                    var idx = InstructionTiles.FindLastIndex(x => x is IActionStateEndInstructionTile || x is IActionEndInstructionTile);
                    if (idx > i)
                        i = idx;
                }
            }
            else
            {
                if (instance is IActionEndInstructionTile)
                {
                    if (HasEndTile(page)) return;
                    i = -1;
                }
                else
                {
                    var idx = InstructionTiles.FindLastIndex(x => x is IActionEndInstructionTile);
                    if (idx > i)
                        i = idx;
                }
            }

            if (i == -1)
            {
                InstructionTiles.Add(instance);
                Changed(InstructionTiles.Count - 1);
            }
            else
            {
                if (i < InstructionTiles.Count && InstructionTiles[i] is IConditionInstructionTile && instance is IActionInstructionTile)
                {
                    var actionIndex = InstructionTiles.FindIndex(x => x is IActionInstructionTile);
                    InstructionTiles.Insert(actionIndex > -1 ? actionIndex : i+1, instance);
                    Changed(i);
                }
                else
                {
                    InstructionTiles.Insert(i, instance);
                    Changed(i);
                }
            }
        }

        public bool HasEndTile(IMonaBrainPage page)
        {
            return (!page.IsCore && InstructionTiles.FindLastIndex(x => x is IActionStateEndInstructionTile || x is IActionEndInstructionTile) > -1) ||
                (page.IsCore && InstructionTiles.FindLastIndex(x => x is IActionEndInstructionTile) > -1);
        }

        private void Changed(int i)
        {
            OnRefresh(i);
            if(_brain != null)
                EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brain.Guid), new MonaBrainReloadEvent());
        }

        public void DeleteTile(int i)
        {
            if (i >= 0 && i < InstructionTiles.Count)
            {
                InstructionTiles.RemoveAt(i);
                Changed(Mathf.Min(i, InstructionTiles.Count-1));
            }
        }

        public void ToggleMute()
        {
            _muted = !_muted;
        }

        public void ToggleMuteTile(int i)
        {
            if (i >= 0 && i < InstructionTiles.Count)
            {
                InstructionTiles[i].Muted = !InstructionTiles[i].Muted;
            }
        }

        public void MoveTileRight(int sourceIndex)
        {
            if (sourceIndex < InstructionTiles.Count - 1)
            {
                var targetTile = InstructionTiles[sourceIndex + 1];
                var sourceTile = InstructionTiles[sourceIndex];
                InstructionTiles.RemoveAt(sourceIndex);
                int i = InstructionTiles.IndexOf(targetTile) + 1;
                InstructionTiles.Insert(i, sourceTile);
                Changed(i);
            }
        }

        public void MoveTileLeft(int sourceIndex)
        {
            if (sourceIndex > 0)
            {
                var targetTile = InstructionTiles[sourceIndex - 1];
                var sourceTile = InstructionTiles[sourceIndex];
                InstructionTiles.RemoveAt(sourceIndex);
                var i = InstructionTiles.IndexOf(targetTile);
                InstructionTiles.Insert(i, sourceTile);
                Changed(i);
            }
        }

        public void ReplaceTile(int i, IInstructionTile tile)
        {
            var instance = (IInstructionTile)Activator.CreateInstance(tile.TileType);
            instance.Id = tile.Id;
            instance.Name = tile.Name;
            instance.Category = tile.Category;

            CopyBrainProperties(InstructionTiles[i], instance);
            InstructionTiles[i] = instance;
            Changed(i);
        }

        private void CopyBrainProperties(IInstructionTile source, IInstructionTile target)
        {
            var properties = target.GetType().GetProperties();
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var attributes = property.GetCustomAttributes(typeof(BrainProperty), true);
                if (attributes.Length == 0)
                {
                    attributes = property.GetCustomAttributes(typeof(BrainPropertyValueName), true);
                    if (attributes.Length == 0)
                        continue;
                }
                var sourceProperty = source.GetType().GetProperty(property.Name);
                if (sourceProperty != null)
                {
                    if (!sourceProperty.PropertyType.IsEnum || sourceProperty.PropertyType.GetEnumUnderlyingType().Equals(property.PropertyType.GetEnumUnderlyingType()))  
                        property.SetValue(target, sourceProperty.GetValue(source));
                }
            }
        }

        public void Pause()
        {
            _paused = true;
            for (var i = 0; i < _instructionTiles.Count; i++)
            {
                var tile = _instructionTiles[i];
                if (tile is IPauseableInstructionTile)
                    ((IPauseableInstructionTile)tile).Pause();
            }
        }

        public void Resume()
        {
            _paused = false;
            var resumed = false;
            for (var i = 0; i < _instructionTiles.Count; i++)
            {
                var tile = _instructionTiles[i];
                if (tile is IPauseableInstructionTile)
                {
                    if (((IPauseableInstructionTile)tile).Resume())
                        resumed = true;
                }
            }

            if (!resumed)
            {
                var tileIndex = (int)_brain.Variables.GetFloat(_progressTile);
                //Debug.Log($"{nameof(Instruction)} resume instruction #{_page.Instructions.IndexOf(this)}, tile: {tileIndex}");
                _result = InstructionTileResult.Success;
                if (tileIndex > -1 && InstructionTiles.Count > 0)
                    ExecuteTile(InstructionTiles[tileIndex]);
            }
        }

        public void Unload()
        {
            _unloaded = true;
            for (var i = 0; i < _instructionTiles.Count; i++)
                _instructionTiles[i].Unload();
        }
    }
}