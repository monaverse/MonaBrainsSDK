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
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Assets;

namespace Mona.SDK.Brains.Core.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Mona Brains/Graph", fileName = "BrainGraph")]
    [Serializable]
    public class MonaBrainGraph : ScriptableObject, IMonaBrain
    {
        public event Action<string, IMonaBrain> OnStateChanged;
        public event Action OnMigrate;

        [SerializeField]
        private Guid _guid = Guid.NewGuid();
        public Guid Guid { get => _guid; set => _guid = value; }

        [SerializeField]
        private string _name;
        public string Name { get => _name; set => _name = value; }

        [SerializeField]
        public MonaBrainPropertyType _propertyType;
        public MonaBrainPropertyType PropertyType { get => _propertyType; set => _propertyType = value; }

        [SerializeField]
        private bool _loggingEnabled;
        public bool LoggingEnabled { get => _loggingEnabled; set => _loggingEnabled = value; }

        private GameObject _gameObject;
        public GameObject GameObject => _gameObject;

        private IMonaBrainRunner _runner;
        private int _index;

        private IMonaBody _body;
        private IMonaBody _bodyParent;
        public IMonaBody Body => _body;

        private IMonaBrainVariables _variables;
        public IMonaBrainVariables Variables => _variables;

        [SerializeReference]
        private IMonaBrainVariables _defaultVariables = new MonaBrainVariables();
        public IMonaBrainVariables DefaultVariables => _defaultVariables;

        [SerializeField]
        private int _priority;

        [SerializeReference]
        private IMonaBrainPage _corePage = new MonaBrainPage("Core", true);
        public IMonaBrainPage CorePage => _corePage;

        [SerializeReference]
        private List<IMonaBrainPage> _statePages = new List<IMonaBrainPage>();
        public List<IMonaBrainPage> StatePages => _statePages;

        private IMonaBrainPage _activeStatePage;

        [SerializeField]
        private bool _began;

        [SerializeField]
        protected List<string> _monaTags = new List<string>();
        public List<string> MonaTags => _monaTags;

        [SerializeReference]
        protected List<IMonaAssetProvider> _monaAssets = new List<IMonaAssetProvider>();
        public List<IMonaAssetProvider> MonaAssets => _monaAssets;

        private List<IMonaAssetItem> _assets = new List<IMonaAssetItem>();
        public List<IMonaAssetItem> GetAllMonaAssets()
        {
            _assets.Clear();
            for (var i = 0; i < _monaAssets.Count; i++)
            {
                _assets.AddRange(_monaAssets[i].AllAssets);
            }
            return _assets;
        }

        public IMonaAssetItem GetMonaAsset(string id)
        {
            for(var i = 0;i < _monaAssets.Count; i++)
            {
                var item = _monaAssets[i].GetMonaAsset(id);
                if (item != null)
                    return item;
            }
            return null;
        }

        public bool HasMonaTag(string tag) => MonaTags.Contains(tag);

        public void AddTag(string tag)
        {
            _body.AddTag(tag);
            if (!HasMonaTag(tag))
                MonaTags.Add(tag);
        }

        public void RemoveTag(string tag)
        {
            _body.RemoveTag(tag);
            if (HasMonaTag(tag))
                MonaTags.Remove(tag);
        }

        public bool HasPlayerTag()
        {
            for(var i = 0;i < _body.MonaTags.Count; i++)
            {
                var tag = _body.MonaTags[i];
                var monaTag = _monaTagSource.GetTag(tag);
                if (monaTag.IsPlayerTag) return true;
            }

            for (var i = 0; i < MonaTags.Count; i++)
            {
                var tag = MonaTags[i];
                var monaTag = _monaTagSource.GetTag(tag);
                if (monaTag.IsPlayerTag) return true;
            }

            return false;
        }

        public bool HasPlayerTag(List<string> monaTags)
        {
            for (var i = 0; i < monaTags.Count; i++)
            {
                var tag = monaTags[i];
                var monaTag = _monaTagSource.GetTag(tag);
                if (monaTag.IsPlayerTag) return true;
            }
            return false;
        }


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
            get => _variables.GetString(MonaBrainConstants.RESULT_STATE);
            set
            {
                _variables.Set(MonaBrainConstants.RESULT_STATE, value);
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

        private Action<MonaBodyParentChangedEvent> OnBodyParentChanged;
        private Action<MonaBrainTickEvent> OnMonaBrainTick;
        private Action<MonaTriggerEvent> OnMonaTrigger;
        private Action<MonaValueChangedEvent> OnMonaValueChanged;
        private Action<MonaBroadcastMessageEvent> OnBroadcastMessage;
        private Action<MonaBodyFixedTickEvent> OnInputOnFixedTick;

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

        public void Preload(GameObject gameObject, IMonaBrainRunner runner, int index)
        {
            _index = index;
            CacheReferences(gameObject, runner, _index);
            CacheReservedBrainVariables();
            PreloadPages();
            AddEventDelegates();
            AddHierarchyDelgates();
        }

        private void CacheReservedBrainVariables()
        {
            var speed = _variables.GetVariable(MonaBrainConstants.SPEED_FACTOR, typeof(MonaVariablesFloat));
                ((IMonaVariablesFloatValue)speed).Value = 1f;

            _variables.GetVariable(MonaBrainConstants.RESULT_SENDER, typeof(MonaVariablesBrain));
            _variables.GetVariable(MonaBrainConstants.RESULT_TARGET, typeof(MonaVariablesBody));

            _variables.GetVariable(MonaBrainConstants.RESULT_MOVE_DIRECTION, typeof(MonaVariablesVector2));
            _variables.GetVariable(MonaBrainConstants.RESULT_MOUSE_DIRECTION, typeof(MonaVariablesVector2));

            _variables.GetVariable(MonaBrainConstants.RESULT_HIT_TARGET, typeof(MonaVariablesBody));
            _variables.GetVariable(MonaBrainConstants.RESULT_HIT_POINT, typeof(MonaVariablesVector3));
            _variables.GetVariable(MonaBrainConstants.RESULT_HIT_NORMAL, typeof(MonaVariablesVector3));

            _variables.GetVariable(MonaBrainConstants.RESULT_STATE, typeof(MonaVariablesString));
            _variables.GetVariable(MonaBrainConstants.ON_STARTING, typeof(MonaVariablesBool));
        }

        private void CacheReferences(GameObject gameObject, IMonaBrainRunner runner, int index)
        {
            _gameObject = gameObject;
            _runner = runner;

            _body = gameObject.GetComponent<IMonaBody>();
            _bodyParent = _body.Parent;
            if (_body == null)
                _body = gameObject.AddComponent<MonaBody>();

            if (_variables == null)
            {
                var variables = gameObject.GetComponents<MonaBrainVariablesBehaviour>();
                if (index < variables.Length) _variables = variables[index];
                else _variables = gameObject.AddComponent<MonaBrainVariablesBehaviour>().Variables;

                if (_defaultVariables == null)
                    _defaultVariables = new MonaBrainVariables();

                _variables.VariableList = _defaultVariables.VariableList;
                _variables.SetGameObject(_gameObject, this);
            }
        }

        private void AddEventDelegates()
        {
            OnBodyParentChanged = HandleBodyParentChanged;
            EventBus.Register<MonaBodyParentChangedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_PARENT_CHANGED_EVENT, _body), OnBodyParentChanged);

            OnMonaBrainTick = HandleMonaBrainTick;
            EventBus.Register<MonaBrainTickEvent>(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, this), OnMonaBrainTick);
            EventBus.Register<MonaBrainTickEvent>(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _body), OnMonaBrainTick);

            OnMonaTrigger = HandleMonaTrigger;
            EventBus.Register<MonaTriggerEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, this), OnMonaTrigger);

            OnMonaValueChanged = HandleMonaValueChanged;
            EventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, this), OnMonaValueChanged);

            OnBroadcastMessage = HandleBroadcastMessage;
            EventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            EventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), OnBroadcastMessage);

            OnInputOnFixedTick = HandleInputOnFixedTick;
            EventBus.Register<MonaBodyFixedTickEvent>(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _body), OnInputOnFixedTick);
        }

        private void AddHierarchyDelgates()
        {
            if (_bodyParent != null)
                EventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _bodyParent), OnBroadcastMessage);
        }


        private void RemoveEventDelegates()
        {
            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_PARENT_CHANGED_EVENT, _body), OnBodyParentChanged);

            EventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, this), OnMonaBrainTick);
            EventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _body), OnMonaBrainTick);
            EventBus.Unregister(new EventHook(MonaBrainConstants.TRIGGER_EVENT, this), OnMonaTrigger);
            EventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, this), OnMonaValueChanged);

            EventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            EventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), OnBroadcastMessage);

            EventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_FIXED_TICK_EVENT, _body), OnInputOnFixedTick);

            OnInputOnFixedTick = null;
            OnBroadcastMessage = null;
        }

        private void RemoveHierarchyDelegates()
        {
            {
                if (_bodyParent != null)
                    EventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _bodyParent), OnBroadcastMessage);
            }
        }

        private void PreloadPages()
        {
            CorePage.SetActive(true);
            CorePage.Preload(this);
            for (var i = 0; i < StatePages.Count; i++)
                StatePages[i].Preload(this);
            if(StatePages.Count > 0)
                BrainState = StatePages[0].Name;
        }

        public void Begin()
        {
            _began = true;
            SetActiveStatePage(BrainState);
            //if (LoggingEnabled)
            //    Debug.Log($"{nameof(Begin)} brain on Body {_body.ActiveTransform.name}", _body.ActiveTransform);

            _variables.Set(MonaBrainConstants.ON_STARTING, true, false);
            ExecuteCorePageInstructions(InstructionEventTypes.Start);
            ExecuteStatePageInstructions(InstructionEventTypes.Start);
            _variables.Set(MonaBrainConstants.ON_STARTING, false, false);
        }

        public void Pause()
        {
            //if (LoggingEnabled)
            //    Debug.Log($"{nameof(Pause)} brain on Body {_body.ActiveTransform.name}", _body.ActiveTransform);

            CorePage.Pause();
            for (var i = 0; i < _statePages.Count; i++)
                _statePages[i].Pause();
        }

        public void Resume()
        {
           // if (LoggingEnabled)
            //    Debug.Log($"{nameof(Resume)} brain on Body {_body.ActiveTransform.name}", _body.ActiveTransform);

            CorePage.Resume();
            for (var i = 0; i < _statePages.Count; i++)
                _statePages[i].Resume();

            EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, this), new MonaBrainTickEvent(InstructionEventTypes.Tick));
        }

        private void HandleBodyParentChanged(MonaBodyParentChangedEvent evt)
        {
            RemoveHierarchyDelegates();
            _bodyParent = _body.Parent;
            AddHierarchyDelgates();
        }

        private void HandleMonaBrainTick(MonaBrainTickEvent evt)
        {
            _runner.WaitFrame(_index, ExecuteTickEvent, evt, typeof(MonaBrainTickEvent));
        }

        private void ExecuteTickEvent(IInstructionEvent evt)
        {
            var tickEvt = (MonaBrainTickEvent)evt;
            //if(LoggingEnabled)
            //    Debug.Log($"{nameof(ExecuteTickEvent)} {tickEvt.Type}", _body.ActiveTransform.gameObject);
            ExecuteCorePageInstructions(tickEvt.Type);
            ExecuteStatePageInstructions(tickEvt.Type);
        }

        private void HandleMonaTrigger(MonaTriggerEvent evt)
        {
            if (!_began) return;

            if (evt.Type == MonaTriggerType.OnFieldOfViewChanged)
            {
                _runner.WaitFrame(_index, ExecuteTriggerEvent, evt, typeof(MonaTriggerEvent));
            }
            else
            {
                ExecuteCorePageInstructions(InstructionEventTypes.Trigger, evt);
                ExecuteStatePageInstructions(InstructionEventTypes.Trigger, evt);
            }
        }

        private void ExecuteTriggerEvent(IInstructionEvent evt)
        {
            if (!_began) return;

            ExecuteCorePageInstructions(InstructionEventTypes.Trigger, evt);
            ExecuteStatePageInstructions(InstructionEventTypes.Trigger, evt);
        }

        private void HandleMonaValueChanged(MonaValueChangedEvent evt)
        {
            if (!_began) return;

            if (evt.Name == MonaBrainConstants.RESULT_STATE) return;
            if (evt.Name.StartsWith("__")) return;
            ExecuteCorePageInstructions(InstructionEventTypes.Value);
            ExecuteStatePageInstructions(InstructionEventTypes.Value);
        }

        private void HandleBroadcastMessage(MonaBroadcastMessageEvent evt)
        {
            //Debug.Log($"{nameof(HandleBroadcastMessage)} '{evt.Message}' received by ({Name}) on frame {Time.frameCount}");
            if (!HasMessage(evt.Message))
                _messages.Add(evt);

            _runner.WaitFrame(_index, ExecuteMessage, evt, typeof(MonaBroadcastMessageEvent));
        }

        private void ExecuteMessage(IInstructionEvent evt)
        {
            ExecuteCorePageInstructions(InstructionEventTypes.Message);
            ExecuteStatePageInstructions(InstructionEventTypes.Message);
        }

        private void HandleInputOnFixedTick(MonaBodyFixedTickEvent evt)
        {
            if (!_began) return;

            if (!evt.HasInput) return;
            ExecuteCorePageInstructions(InstructionEventTypes.Input);
            ExecuteStatePageInstructions(InstructionEventTypes.Input);
        }

        private void HandleStatePropertyChanged(string value)
        {
            if (!_began) return;

            SetActiveStatePage(value);

            OnStateChanged?.Invoke(value, this);
            _variables.Set(MonaBrainConstants.ON_STARTING, true, false);
            ExecuteStatePageInstructions(InstructionEventTypes.State);
            _variables.Set(MonaBrainConstants.ON_STARTING, false, false);
        }

        private void SetActiveStatePage(string value)
        {
            _activeStatePage = null;
            for (var i = 0; i < StatePages.Count; i++)
            {
                StatePages[i].SetActive(false);
                if (StatePages[i].Name == BrainState)
                {
                    StatePages[i].SetActive(true);
                    _activeStatePage = StatePages[i];
                }
            }
        }

        private void ExecuteCorePageInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            CorePage.ExecuteInstructions(eventType, evt);
        }

        private void ExecuteStatePageInstructions(InstructionEventTypes eventType, IInstructionEvent evt = null)
        {
            if (_activeStatePage != null)
                _activeStatePage.ExecuteInstructions(eventType, evt);
        }

        public void Unload()
        {
            CorePage.Unload();
            for (var i = 0; i < _statePages.Count; i++)
                _statePages[i].Unload();
            RemoveEventDelegates();
            RemoveHierarchyDelegates();
            _began = false;
        }
    }
}