﻿using Mona.SDK.Core.Body;
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

        public void Preload(GameObject gameObject)
        {
            CacheReferences(gameObject);
            PreloadPages();
            AddEventDelegates();
        }

        private void CacheReferences(GameObject gameObject)
        {
            _gameObject = gameObject;

            _body = gameObject.GetComponent<IMonaBody>();
            if (_body == null)
                _body = gameObject.AddComponent<MonaBody>();

            if (_state == null)
            {
                _state = gameObject.AddComponent<MonaBrainValues>().State;
                _state.Values = _defaultState.Values;
            }
        }

        private void AddEventDelegates()
        {
            OnBroadcastMessage = HandleBroadcastMessage;
            EventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);

            OnInputTick = HandleInputTick;
            EventBus.Register<MonaHasInputTickEvent>(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, this), OnInputTick);
        }

        private void RemoveEventDelegates()
        {
            EventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            EventBus.Unregister(new EventHook(MonaBrainConstants.INPUT_TICK_EVENT, this), OnInputTick);

            OnInputTick = null;
            OnBroadcastMessage = null;
        }

        private void PreloadPages()
        {
            CorePage.Preload(this);
            for (var i = 0; i < StatePages.Count; i++)
                StatePages[i].Preload(this);
        }

        public void Begin()
        {
            _state.Set(MonaBrainConstants.ON_STARTING, true, false);
            ExecuteCorePageInstructions(InstructionEventTypes.Start);
            ExecuteStatePageInstructions(InstructionEventTypes.Start);
            _state.Set(MonaBrainConstants.ON_STARTING, false, false);
        }

        private void HandleBroadcastMessage(MonaBroadcastMessageEvent evt)
        {
            Debug.Log($"{nameof(HandleBroadcastMessage)} '{evt.Message}' received by ({Name}) on frame {Time.frameCount}");
            if (!HasMessage(evt.Message))
                _messages.Add(evt);

            ExecuteCorePageInstructions(InstructionEventTypes.Message);
            ExecuteStatePageInstructions(InstructionEventTypes.Message);

            _messages.Remove(evt);
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

        private void ExecuteCorePageInstructions(InstructionEventTypes eventType)
        {
            CorePage.ExecuteInstructions(eventType);
        }

        private void ExecuteStatePageInstructions(InstructionEventTypes eventType)
        {
            for (var i = 0; i < StatePages.Count; i++)
            {
                if (StatePages[i].Name == BrainState)
                {
                    StatePages[i].ExecuteInstructions(eventType);
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