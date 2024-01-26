using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.State;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Mona.SDK.Brains.Core.Events;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Events;
using Mona.SDK.Core;

namespace Mona.SDK.Brains.Core.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Graph", fileName = "BrainGraph")]
    [Serializable]
    public class MonaBrainGraph : ScriptableObject, IMonaBrain
    {
        public event Action<string, IMonaBrain> OnStateChanged;
        public event Action OnMigrate;

        [SerializeField]
        private string _name;
        public string Name { get => _name; set => _name = value; }

        [SerializeField]
        public MonaBrainPropertyType _propertyType;
        public MonaBrainPropertyType PropertyType { get => _propertyType; set => _propertyType = value; }

        private GameObject _gameObject;
        public GameObject GameObject => _gameObject;

        private IMonaBrainRunner _runner;

        private IMonaBody _body;
        public IMonaBody Body => _body;

        private IMonaBrainState _state;
        public IMonaBrainState State => _state;

        [SerializeReference]
        private IMonaBrainState _defaultState = new MonaBrainState();
        public IMonaBrainState DefaultState => _defaultState;

        [SerializeField]
        private int _priority;

        [SerializeReference]
        private IMonaBrainPage _corePage = new MonaBrainPage();
        public IMonaBrainPage CorePage => _corePage;

        [SerializeReference]
        private List<IMonaBrainPage> _statePages = new List<IMonaBrainPage>();
        public List<IMonaBrainPage> StatePages => _statePages;

        [SerializeField]
        protected List<string> _monaTags = new List<string>();
        public List<string> MonaTags => _monaTags;

        public bool HasMonaTag(string tag) => MonaTags.Contains(tag);

        [SerializeField]
        private MonaTags _monaTagSource;
        public IMonaTags MonaTagSource { get => _monaTagSource; set => _monaTagSource = (MonaTags)value; }

        [SerializeField]
        private InstructionTileSet _tileSet;
        public IInstructionTileSet TileSet { 
            get => _tileSet; 
            set {
                if (_tileSet != (InstructionTileSet)value)
                {
                    _tileSet = (InstructionTileSet)value;
                    OnMigrate?.Invoke();
                }
            }
        }

        public string BrainState
        {
            get => _state.GetString(MonaBrainConstants.RESULT_STATE);
            set
            {
                _state.Set(MonaBrainConstants.RESULT_STATE, value);
                HandleStatePropertyChanged(value);
            }
        }

        private bool _hasInput;

        public string LocalId => _body.LocalId;
        public int Priority => _priority;

        public bool HasInput {
            get => _hasInput;
            set => _hasInput = value;
        }

        private IMonaBrainPlayer _player;
        public IMonaBrainPlayer Player => _player;

        private List<MonaBroadcastMessageEvent> _messages = new List<MonaBroadcastMessageEvent>();

        private Action<MonaTickEvent> OnMonaTick;
        private Action<MonaTriggerEvent> OnMonaTrigger;
        private Action<MonaValueChangedEvent> OnMonaValueChanged;
        private Action<MonaBroadcastMessageEvent> OnBroadcastMessage;
        private Action<MonaHasInputTickEvent> OnInputTick;

        private bool _coreOnStarting;
        private bool _stateOnStarting;

        public void SetMonaBrainPlayer(IMonaBrainPlayer player)
        {
            _player = player;
        }

        public bool HasAnyMessage => _messages.Count > 0;
        public bool HasMessage(string message)
        {
            for (var i = 0; i < _messages.Count; i++)
            {
                if (_messages[i].Message == message)
                    return true;
            }
            return false;
        }

        public MonaBroadcastMessageEvent GetMessage(string message)
        {
            for (var i = 0; i < _messages.Count; i++)
            {
                if (_messages[i].Message == message)
                    return _messages[i];
            }
            return new MonaBroadcastMessageEvent();
        }

        public void Preload(GameObject gameObject, IMonaBrainRunner runner)
        {
            CacheReferences(gameObject, runner);
            PreloadPages();
            AddEventDelegates();
        }

        private void CacheReferences(GameObject gameObject, IMonaBrainRunner runner)
        {
            _gameObject = gameObject;
            _runner = runner;

            _body = gameObject.GetComponent<IMonaBody>();
            if (_body == null)
                _body = gameObject.AddComponent<MonaBody>();

            if (_state == null)
            {
                _state = gameObject.AddComponent<MonaBrainValues>().State;
                if (_defaultState == null)
                    _defaultState = new MonaBrainState();
                _state.Values = _defaultState.Values;
                _state.SetGameObject(_gameObject, this);
            }
        }

        private void AddEventDelegates()
        {
            OnMonaTick = HandleMonaTick;
            EventBus.Register<MonaTickEvent>(new EventHook(MonaBrainConstants.TICK_EVENT, this), OnMonaTick);

            OnMonaTrigger = HandleMonaTrigger;
            EventBus.Register<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, this), OnMonaTrigger);

            OnMonaValueChanged = HandleMonaValueChanged;
            EventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, this), OnMonaValueChanged);

            OnBroadcastMessage = HandleBroadcastMessage;
            EventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            EventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), OnBroadcastMessage);

            OnInputTick = HandleInputTick;
            EventBus.Register<MonaHasInputTickEvent>(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, this), OnInputTick);
        }

        private void RemoveEventDelegates()
        {
            EventBus.Unregister(new EventHook(MonaBrainConstants.TICK_EVENT, this), OnMonaTick);
            EventBus.Unregister(new EventHook(MonaBrainConstants.TRIGGER_EVENT, this), OnMonaTrigger);
            EventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, this), OnMonaValueChanged);

            EventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            EventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), OnBroadcastMessage);

            EventBus.Unregister(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, this), OnInputTick);

            OnInputTick = null;
            OnBroadcastMessage = null;
        }

        private void PreloadPages()
        {
            CorePage.Preload(this);
            for (var i = 0; i < StatePages.Count; i++)
                StatePages[i].Preload(this);
            if(StatePages.Count > 0)
                BrainState = StatePages[0].Name;
        }

        public void Begin()
        {
            _state.Set(MonaBrainConstants.ON_STARTING, true, false);
            ExecuteCorePageInstructions(InstructionEventTypes.Start);
            ExecuteStatePageInstructions(InstructionEventTypes.Start);
            _state.Set(MonaBrainConstants.ON_STARTING, false, false);
        }

        private void HandleMonaTick(MonaTickEvent evt)
        {
            _runner.WaitFrame(ExecuteTickEvent, evt, typeof(MonaTickEvent));
        }

        private void ExecuteTickEvent(IInstructionEvent evt)
        { 
            ExecuteCorePageInstructions(InstructionEventTypes.Tick);
            ExecuteStatePageInstructions(InstructionEventTypes.Tick);
        }

        private void HandleMonaTrigger(MonaTriggerEvent evt)
        {
            if (evt.Type == MonaTriggerType.OnFieldOfViewChanged)
            {
                _runner.WaitFrame(ExecuteTriggerEvent, evt, typeof(MonaTriggerEvent));
            }
            else
            {
                ExecuteCorePageInstructions(InstructionEventTypes.Trigger, evt);
                ExecuteStatePageInstructions(InstructionEventTypes.Trigger, evt);
            }
        }

        private void ExecuteTriggerEvent(IInstructionEvent evt)
        {
            ExecuteCorePageInstructions(InstructionEventTypes.Trigger, evt);
            ExecuteStatePageInstructions(InstructionEventTypes.Trigger, evt);
        }

        private void HandleMonaValueChanged(MonaValueChangedEvent evt)
        {
            if (evt.Name == MonaBrainConstants.RESULT_STATE) return;
            ExecuteCorePageInstructions(InstructionEventTypes.Value);
            ExecuteStatePageInstructions(InstructionEventTypes.Value);
        }

        private void HandleBroadcastMessage(MonaBroadcastMessageEvent evt)
        {
            //Debug.Log($"{nameof(HandleBroadcastMessage)} '{evt.Message}' received by ({Name}) on frame {Time.frameCount}");
            if (!HasMessage(evt.Message))
                _messages.Add(evt);

            _runner.WaitFrame(ExecuteMessage, evt, typeof(MonaBroadcastMessageEvent));
        }

        private void ExecuteMessage(IInstructionEvent evt)
        { 
            ExecuteCorePageInstructions(InstructionEventTypes.Message);
            ExecuteStatePageInstructions(InstructionEventTypes.Message);

            _messages.Remove((MonaBroadcastMessageEvent)evt);
        }

        private void HandleInputTick(MonaHasInputTickEvent evt)
        {
            ExecuteCorePageInstructions(InstructionEventTypes.Input);
            ExecuteStatePageInstructions(InstructionEventTypes.Input);
        }

        private void HandleStatePropertyChanged(string value)
        {
            OnStateChanged?.Invoke(value, this);
            _state.Set(MonaBrainConstants.ON_STARTING, true, false);
            ExecuteStatePageInstructions(InstructionEventTypes.Start);
            _state.Set(MonaBrainConstants.ON_STARTING, false, false);
        }

        private void ExecuteCorePageInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            if (!_body.HasControl()) return;
            CorePage.ExecuteInstructions(eventType, evt);
        }

        private void ExecuteStatePageInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            if (!_body.HasControl()) return;
            for (var i = 0; i < StatePages.Count; i++)
            {
                if (StatePages[i].Name == BrainState)
                {
                    StatePages[i].ExecuteInstructions(eventType, evt);
                    break;
                }
            }
        }

        public void Unload()
        {
            RemoveEventDelegates();
        }
    }
}