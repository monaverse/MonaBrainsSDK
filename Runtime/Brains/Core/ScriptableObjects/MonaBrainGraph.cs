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
using Mona.SDK.Core.Events;
using Mona.SDK.Core;
using Mona.SDK.Brains.Core.Brain.Interfaces;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Assets;
using Mona.SDK.Brains.Core.Animation;
using Mona.SDK.Core.Utils;

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

        [SerializeField]
        private bool _legacyMonaPlatforms;
        public bool LegacyMonaPlatforms { get => _legacyMonaPlatforms; set => _legacyMonaPlatforms = value; }

        [SerializeField]
        private string _sourceUrl;
        public string SourceUrl { get => _sourceUrl; set => _sourceUrl = value; }

        private GameObject _gameObject;
        public GameObject GameObject => _gameObject;

        private IMonaBrainRunner _runner;
        public IMonaBrainRunner Runner => _runner;

        private int _index;

        private IMonaBody _body;
        private IMonaBody _bodyParent;
        private List<IMonaBody> _spawnedBodies =  new List<IMonaBody>();
        public IMonaBody Body => _body;
        public List<IMonaBody> SpawnedBodies { get => _spawnedBodies; set => _spawnedBodies = value; }

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

        private List<IMonaAssetProviderBehaviour> _childAssets;
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
            for (var i = 0; i < _monaAssets.Count; i++)
            {
                var item = _monaAssets[i].GetMonaAsset(id);
                if (item != null)
                    return item;
            }
            return null;
        }

        public List<IMonaAssetProvider> GetAllMonaAssetProviders()
        {
            return _monaAssets;
        }

        public IMonaAssetProvider GetMonaAssetProvider(string id)
        {
            return _monaAssets.Find(x => x.Name == id);
        }

        public bool HasAnimationTiles()
        {
            if (_corePage.HasAnimationTiles()) return true;
            for (var i = 0; i < _statePages.Count; i++)
            {
                if (_statePages[i].HasAnimationTiles()) return true;
            }
            return PropertyType != MonaBrainPropertyType.Default;
        }

        public bool HasRigidbodyTiles()
        {
            if (_corePage.HasRigidbodyTiles()) return true;
            for (var i = 0; i < _statePages.Count; i++)
            {
                if (_statePages[i].HasRigidbodyTiles()) return true;
            }
            return false;
        }

        public bool HasUsePhysicsTileSetToTrue()
        {
            if (_corePage.HasUsePhysicsTileSetToTrue()) return true;
            for (var i = 0; i < _statePages.Count; i++)
            {
                if (_statePages[i].HasUsePhysicsTileSetToTrue()) return true;
            }
            return false;
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
            if (_monaTagSource == null)
            {
                if (_body != null)
                    Debug.Log($"Please Attach Mona Tags to this brain. ", _body.Transform.gameObject);
                else
                    Debug.Log($"Please Attach Mona Tags to this brain. {this.Name}");
                return false;
            }

            for (var i = 0; i < _body.MonaTags.Count; i++)
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
            if (_monaTagSource == null)
            {
                if (_body != null)
                    Debug.Log($"Please Attach Mona Tags to this brain. ", _body.Transform.gameObject);
                else
                    Debug.Log($"Please Attach Mona Tags to this brain. {this.Name}");
                return false;
            }

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
        private Transform _root;
        public Transform Root => _root;

        private List<InstructionEvent> _messages = new List<InstructionEvent>();

        private Action<InstructionEvent> OnMonaBrainTick;
        private Action<InstructionEvent> OnMonaTrigger;
        private Action<InstructionEvent> OnBroadcastMessage;

        private Action<MonaBodyHasInputEvent> OnMonaBodyHasInput;
        private Action<MonaBodyParentChangedEvent> OnBodyParentChanged;
        private Action<MonaValueChangedEvent> OnMonaValueChanged;
        private Action<MonaBodyAnimationControllerChangeEvent> OnAnimationControllerChange;

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
                if (_messages[i].Message.Equals(message))
                    return true;
            }
            return false;
        }

        public InstructionEvent GetMessage(string message)
        {
            for (var i = 0; i < _messages.Count; i++)
            {
                if (_messages[i].Message == message)
                    return _messages[i];
            }
            return new InstructionEvent();
        }

        public void Preload(GameObject gameObject, IMonaBrainRunner runner, int index)
        {
            _index = index;
            CacheReferences(gameObject, runner, _index);
            CacheChildMonaAssets();
            AddMonaAssetsToNetwork();
            CacheReservedBrainVariables();
            BuildRoot();
            SetupAnimation(null);
            PreloadPages();
            AddEventDelegates();
            AddHierarchyDelgates();
        }

        private void CacheChildMonaAssets()
        {
            if (_childAssets == null)
                _childAssets = new List<IMonaAssetProviderBehaviour>(_body.Transform.GetComponentsInChildren<IMonaAssetProviderBehaviour>(true));

            for(var i = 0;i < _childAssets.Count; i++)
            {
                if(_childAssets[i].MonaAssetProvider != null && !_monaAssets.Contains(_childAssets[i].MonaAssetProvider))
                    _monaAssets.Add(_childAssets[i].MonaAssetProvider);
            }
        }

        private void CacheReservedBrainVariables()
        {
            var speed = _variables.GetVariable(MonaBrainConstants.SPEED_FACTOR, typeof(MonaVariablesFloat));
            ((IMonaVariablesFloatValue)speed).Value = 1f;

            _variables.GetVariable(MonaBrainConstants.RESULT_SENDER, typeof(MonaVariablesBrain));
            _variables.GetVariable(MonaBrainConstants.RESULT_TARGET, typeof(MonaVariablesBody));
            _variables.GetVariable(MonaBrainConstants.RESULT_LAST_SPAWNED, typeof(MonaVariablesBody));

            _variables.GetVariable(MonaBrainConstants.RESULT_MOVE_DIRECTION, typeof(MonaVariablesVector2));
            _variables.GetVariable(MonaBrainConstants.RESULT_MOUSE_DIRECTION, typeof(MonaVariablesVector2));

            _variables.GetVariable(MonaBrainConstants.RESULT_HIT_TARGET, typeof(MonaVariablesBody));
            _variables.GetVariable(MonaBrainConstants.RESULT_HIT_POINT, typeof(MonaVariablesVector3));
            _variables.GetVariable(MonaBrainConstants.RESULT_HIT_NORMAL, typeof(MonaVariablesVector3));

            _variables.GetVariable(MonaBrainConstants.RESULT_STATE, typeof(MonaVariablesString));
            _variables.GetVariable(MonaBrainConstants.ON_STARTING, typeof(MonaVariablesBool));

            if (HasAnimationTiles())
            {
                _variables.GetVariable(MonaBrainConstants.TRIGGER, typeof(MonaVariablesString));
                _variables.GetVariable(MonaBrainConstants.TRIGGER_1, typeof(MonaVariablesString));
                _variables.GetVariable(MonaBrainConstants.ANIMATION_SPEED, typeof(MonaVariablesFloat));
            }
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

                _variables.SaveResetDefaults();

                _variables.SetGameObject(_gameObject, this);
            }

            if (HasRigidbodyTiles())
            {
                if (_body.ActiveRigidbody == null)
                {
                    Rigidbody parentRB = _body.Transform.GetComponentInParent<Rigidbody>();
                    IMonaBody parentMB = parentRB != null ? parentRB.GetComponent<IMonaBody>() : null;

                    if (parentRB == null || (parentRB != null && parentMB == null))
                        _body.AddRigidbody();
                }

                if (((_body.ActiveRigidbody != null && !_body.ActiveRigidbody.isKinematic) || HasUsePhysicsTileSetToTrue()) && !_body.HasCollider())
                    _body.AddCollider();
            }

            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_SPAWNED_EVENT), new MonaBrainSpawnedEvent(this));
        }

        private void AddMonaAssetsToNetwork()
        {
            //Debug.Log($"{nameof(AddMonaAssetsToNetwork)}", _body.Transform.gameObject);
            for (var i = 0; i < _monaAssets.Count; i++)
                MonaEventBus.Trigger<MonaAssetProviderAddedEvent>(new EventHook(MonaCoreConstants.MONA_ASSET_PROVIDER_ADDED), new MonaAssetProviderAddedEvent(_monaAssets[i]));
        }

        private void BuildRoot()
        {
            if (!HasAnimationTiles()) return;
            if (_root != null) return;

            var found = false;
            if (_body.Transform.Find("Root") != null)
            {
                var child = _body.Transform.Find("Root");
                if (Vector3.Distance(child.position, _body.GetPosition()) < Mathf.Epsilon)
                {
                    _root = child;
                    found = true;
                }
            }
            else if (_body.Transform.childCount > 0)
            {
                for (var i = 0; i < _body.Transform.childCount; i++)
                {
                    var child = _body.Transform.GetChild(i);
                    if(child.GetComponent<Animator>() != null)
                    {
                        _root = child;
                        _root.name = "Root";
                        found = true;
                        break;
                    }
                }
                if(!found)
                {
                    var child = _body.Transform.GetChild(0);
                    if (Vector3.Distance(child.position, _body.GetPosition()) < Mathf.Epsilon)
                    {
                        _root = child;
                        _root.name = "Root";
                        found = true;
                    }
                }
            }

            if (!found)
            {
                if (_root == null)
                {
                    _root = (new GameObject("Root")).transform;
                    _root.transform.position = _body.GetPosition();
                    _root.transform.rotation = _body.GetRotation();

                    var children = new List<Transform>();
                    for (var i = 0; i < _body.Transform.childCount; i++)
                        children.Add(_body.Transform.GetChild(i));

                    for (var i = 0; i < children.Count; i++)
                        children[i].SetParent(_root, true);

                    _root.transform.SetParent(_body.Transform, true);
                }
            }
        }

        private void HandleAnimationControllerChange(MonaBodyAnimationControllerChangeEvent evt)
        {
            //Debug.Log($"{nameof(HandleAnimationControllerChange)}", _body.ActiveTransform.gameObject);
            SetupAnimation(evt.Animator);
        }

        private void SetupAnimation(Animator animator)
        {
            if (!HasAnimationTiles()) return;

            IMonaAnimationController oldMonaAnimationController = _root.GetComponent<IMonaAnimationController>();
            IMonaAnimationController newMonaAnimationController = null;
            AnimatorOverrideController oldController = null;
            bool reuseController = true;
            bool destroyed = false;

            switch (PropertyType)
            {
                case MonaBrainPropertyType.Default:
                    if (oldMonaAnimationController != null)
                    {
                        oldController = oldMonaAnimationController.Controller;
                        reuseController = oldMonaAnimationController.ReuseController;
                    }

                    if (oldMonaAnimationController != null && !(oldMonaAnimationController is MonaDefaultAnimationController))
                    {
                        Debug.Log($"{nameof(SetupAnimation)} destroy previous controller", _body.Transform.gameObject);
                        DestroyImmediate((MonoBehaviour)oldMonaAnimationController);
                        destroyed = true;
                    }
                    else if (oldMonaAnimationController != null)
                        newMonaAnimationController = oldMonaAnimationController;

                    if (destroyed || newMonaAnimationController == null)
                    {
                        newMonaAnimationController = _root.AddComponent<MonaDefaultAnimationController>();
                    }
                    if (oldController != null)
                        newMonaAnimationController.OldController = oldController;

                    newMonaAnimationController.ReuseController = reuseController;
                    newMonaAnimationController.SetBrain(this, animator);
                    break;
                default:
                    var parts = new List<IMonaBodyPart>(_root.GetComponentsInChildren<IMonaBodyPart>(true));
                    if (parts.Find(x => x.HasMonaTag(HumanBodyBones.Hips.ToString())) != null)
                    {
                        if(oldMonaAnimationController != null)
                        {
                            oldController = oldMonaAnimationController.Controller;
                            reuseController = oldMonaAnimationController.ReuseController;
                        }
                        Debug.Log($"{nameof(SetupAnimation)} add human controller", _body.ActiveTransform.gameObject);
                        if (oldMonaAnimationController != null && !(oldMonaAnimationController is MonaGroundedCreatureAnimationController))
                        {
                            Debug.Log($"{nameof(SetupAnimation)} destroy previous controller", _body.Transform.gameObject);
                            DestroyImmediate((MonoBehaviour)oldMonaAnimationController);
                            destroyed = true;
                        }
                        else if (oldMonaAnimationController != null)
                            newMonaAnimationController = oldMonaAnimationController;

                        if (destroyed || newMonaAnimationController == null)
                        {
                            newMonaAnimationController = _root.AddComponent<MonaGroundedCreatureAnimationController>();
                        }
                        if (oldController != null)
                            newMonaAnimationController.OldController = oldController;

                        newMonaAnimationController.ReuseController = reuseController;
                        newMonaAnimationController.SetBrain(this, animator);
                    }
                    else
                    {
                        if(oldMonaAnimationController != null)
                        {
                            oldController = oldMonaAnimationController.Controller;
                            reuseController = oldMonaAnimationController.ReuseController;
                        }
                        //Debug.Log($"{nameof(SetupAnimation)} add default controller", _body.ActiveTransform.gameObject
                        if (oldMonaAnimationController != null && !(oldMonaAnimationController is MonaDefaultCreatureAnimationController))
                        {
                            Debug.Log($"{nameof(SetupAnimation)} destroy previous controller", _body.Transform.gameObject);
                            DestroyImmediate((MonoBehaviour)oldMonaAnimationController);
                            destroyed = true;
                        }
                        else if (oldMonaAnimationController != null)
                            newMonaAnimationController = oldMonaAnimationController;

                        if (destroyed || newMonaAnimationController == null)
                        {
                            newMonaAnimationController = _root.AddComponent<MonaDefaultCreatureAnimationController>();
                        }
                        if (oldController != null)
                            newMonaAnimationController.OldController = oldController;

                        newMonaAnimationController.ReuseController = reuseController;
                        newMonaAnimationController.SetBrain(this, animator);
                    }
                    break;
            }

            var parent = _body;
            while(parent != null)
            {
                MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGED_EVENT, parent), new MonaBodyAnimationControllerChangedEvent());
                parent = parent.Parent;
            }

        }

        private void AddEventDelegates()
        {
            OnBodyParentChanged = HandleBodyParentChanged;
            MonaEventBus.Register<MonaBodyParentChangedEvent>(new EventHook(MonaCoreConstants.MONA_BODY_PARENT_CHANGED_EVENT, _body), OnBodyParentChanged);

            OnMonaBrainTick = HandleMonaBrainTick;
            MonaEventBus.Register<InstructionEvent>(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, this), OnMonaBrainTick);
            MonaEventBus.Register<InstructionEvent>(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _body), OnMonaBrainTick);

            OnMonaTrigger = HandleMonaTrigger;
            MonaEventBus.Register<InstructionEvent>(new EventHook(MonaBrainConstants.TRIGGER_EVENT, this), OnMonaTrigger);

            OnMonaValueChanged = HandleMonaValueChanged;
            MonaEventBus.Register<MonaValueChangedEvent>(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, this), OnMonaValueChanged);

            OnBroadcastMessage = HandleBroadcastMessage;
            MonaEventBus.Register<InstructionEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            MonaEventBus.Register<InstructionEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), OnBroadcastMessage);

            OnMonaBodyHasInput = HandleHasInput;
            MonaEventBus.Register<MonaBodyHasInputEvent>(new EventHook(MonaCoreConstants.MONA_BODY_HAS_INPUT_EVENT, _body), OnMonaBodyHasInput);

            OnAnimationControllerChange = HandleAnimationControllerChange;
            MonaEventBus.Register<MonaBodyAnimationControllerChangeEvent>(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGE_EVENT, _body), OnAnimationControllerChange);

        }

        private void AddHierarchyDelgates()
        {
            //if (_bodyParent != null && _body is IMonaBodyPart)
              //  MonaEventBus.Register<MonaBroadcastMessageEvent>(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _bodyParent), OnBroadcastMessage);
        }


        private void RemoveEventDelegates()
        {
            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_PARENT_CHANGED_EVENT, _body), OnBodyParentChanged);

            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, this), OnMonaBrainTick);
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, _body), OnMonaBrainTick);

            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.TRIGGER_EVENT, this), OnMonaTrigger);

            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.VALUE_CHANGED_EVENT, this), OnMonaValueChanged);

            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, this), OnBroadcastMessage);
            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _body), OnBroadcastMessage);

            MonaEventBus.Unregister(new EventHook(MonaCoreConstants.MONA_BODY_HAS_INPUT_EVENT, _body), OnMonaBodyHasInput);

            MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BODY_ANIMATION_CONTROLLER_CHANGE_EVENT, _body), OnAnimationControllerChange);
        }

        private void RemoveHierarchyDelegates()
        {
            //if (_bodyParent != null && _body is IMonaBodyPart)
            //    MonaEventBus.Unregister(new EventHook(MonaBrainConstants.BROADCAST_MESSAGE_EVENT, _bodyParent), OnBroadcastMessage);
        }

        private void PreloadPages()
        {
            CorePage.SetActive(true);
            CorePage.Preload(this);
            for (var i = 0; i < StatePages.Count; i++)
                StatePages[i].Preload(this);
            if (StatePages.Count > 0)
                BrainState = StatePages[0].Name;
            _activeStatePage = null;
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

            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_TICK_EVENT, this), new InstructionEvent(InstructionEventTypes.Tick));
        }

        private void HandleBodyParentChanged(MonaBodyParentChangedEvent evt)
        {
            RemoveHierarchyDelegates();
            _bodyParent = _body.Parent;
            AddHierarchyDelgates();
        }

        private Action<InstructionEvent> OnExecuteTickEvent;
        private Action<InstructionEvent> OnExecuteTriggerEvent;
        private Action<InstructionEvent> OnExecuteValueEvent;
        private Action<InstructionEvent> OnExecuteMessageEvent;

        private void HandleMonaBrainTick(InstructionEvent evt)
        {
            if(OnExecuteTickEvent == null) OnExecuteTickEvent = ExecuteTickEvent;
            _runner.WaitFrame(_index, OnExecuteTickEvent, evt, LoggingEnabled);
        }

        private void ExecuteTickEvent(InstructionEvent evt)
        {
            //if(LoggingEnabled)
            //    Debug.Log($"{nameof(ExecuteTickEvent)} {evt.Type}", _body.ActiveTransform.gameObject);
            ExecuteCorePageInstructions(evt.Type, evt);
            ExecuteStatePageInstructions(evt.Type, evt);
        }

        private void HandleMonaTrigger(InstructionEvent evt)
        {
            if (!_began) return;

            if (evt.TriggerType == MonaTriggerType.OnFieldOfViewChanged)
            {
                if (OnExecuteTriggerEvent == null) OnExecuteTriggerEvent = ExecuteTriggerEvent;
                _runner.WaitFrame(_index, OnExecuteTriggerEvent, evt, LoggingEnabled);
            }
            else
            {
                ExecuteCorePageInstructions(InstructionEventTypes.Trigger, evt);
                ExecuteStatePageInstructions(InstructionEventTypes.Trigger, evt);
            }
        }

        private void ExecuteTriggerEvent(InstructionEvent evt)
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

            var nevt = new InstructionEvent(evt.Name, evt.Value);
            if (OnExecuteValueEvent == null) OnExecuteValueEvent = ExecuteValueEvent;
            _runner.WaitFrame(_index, OnExecuteValueEvent, nevt, LoggingEnabled);
        }

        private void ExecuteValueEvent(InstructionEvent evt)
        {
            if (!_began) return;

            ExecuteCorePageInstructions(InstructionEventTypes.Value, evt);
            ExecuteStatePageInstructions(InstructionEventTypes.Value, evt);
        }

        private void HandleBroadcastMessage(InstructionEvent evt)
        {
            //Debug.Log($"{nameof(HandleBroadcastMessage)} '{evt.Message}' received by ({Name}) on frame {Time.frameCount}");
            if (OnExecuteMessageEvent == null) OnExecuteMessageEvent = ExecuteMessage;
            _runner.WaitFrame(_index, OnExecuteMessageEvent, evt, LoggingEnabled);
        }

        private void ExecuteMessage(InstructionEvent evt)
        {
            var message = evt.Message;
            if (!HasMessage(message))
                _messages.Add(evt);

            ExecuteCorePageInstructions(InstructionEventTypes.Message);
            ExecuteStatePageInstructions(InstructionEventTypes.Message);

            _runner.TriggerMessage(message);

            //Debug.Log($"{nameof(ExecuteMessage)} message: {message} count: {_messages.Count}", _body.Transform.gameObject);
            if (HasMessage(message))
            {
                //Debug.Log($"{nameof(ExecuteMessage)} remove message: {message}");
                _messages.Remove(evt);
            }
        }

        private void HandleHasInput(MonaBodyHasInputEvent evt)
        {
            if (!_began) return;

            ExecuteCorePageInstructions(InstructionEventTypes.Input);
            ExecuteStatePageInstructions(InstructionEventTypes.Input);
        }

        private void HandleStatePropertyChanged(string value)
        {
            if (!_began) return;

            if (_activeStatePage == null || value != _activeStatePage.Name)
            {
                SetActiveStatePage(value);

                OnStateChanged?.Invoke(value, this);
                _variables.Set(MonaBrainConstants.ON_STARTING, true, false);
                ExecuteStatePageInstructions(InstructionEventTypes.State);
                _variables.Set(MonaBrainConstants.ON_STARTING, false, false);
            }
        }

        private void SetActiveStatePage(string value)
        {
            _activeStatePage = null;
            for (var i = 0; i < StatePages.Count; i++)
            {

                for(var m = _messages.Count-1; m >= 0; m--)
                {
                    if (StatePages[i].HasOnMessageTile(_messages[m].Message))
                        _messages.RemoveAt(m);
                }

                if (StatePages[i].Name == value || (string.IsNullOrEmpty(value) && i == 0))
                {
                    StatePages[i].SetActive(true);
                    _activeStatePage = StatePages[i];
                }
                else
                {
                    StatePages[i].SetActive(false);
                }
            }
        }

        private void ExecuteCorePageInstructions(InstructionEventTypes eventType, InstructionEvent evt = default)
        {
            CorePage.ExecuteInstructions(eventType, evt);
        }

        private void ExecuteStatePageInstructions(InstructionEventTypes eventType, InstructionEvent evt = default)
        {
            if (_activeStatePage == null && _statePages.Count > 0)
                SetActiveStatePage(null);

            if (_activeStatePage != null)
                _activeStatePage.ExecuteInstructions(eventType, evt);
        }

        public void Unload(bool destroy = false)
        {
            _messages.Clear();

            CorePage.Unload(destroy);
            for (var i = 0; i < _statePages.Count; i++)
                _statePages[i].Unload(destroy);
            RemoveEventDelegates();
            RemoveHierarchyDelegates();
            _began = false;

            //if(_root != null && (MonoBehaviour)_root.GetComponent<IMonaAnimationController>() != null)
            //    Destroy((MonoBehaviour)_root.GetComponent<IMonaAnimationController>());

            RemoveMonaAssetsFromNetwork();
            ResetBrainVariables();
        }

        private void ResetBrainVariables()
        {
            for(var i = 0;i < _variables.VariableList.Count; i++)
            {
                _variables.VariableList[i].Reset();
            }
        }

        private void RemoveMonaAssetsFromNetwork()
        {
            //Debug.Log($"{nameof(RemoveMonaAssetsFromNetwork)}", _body.Transform.gameObject);
            for (var i = 0; i < _monaAssets.Count; i++)
                MonaEventBus.Trigger<MonaAssetProviderRemovedEvent>(new EventHook(MonaCoreConstants.MONA_ASSET_PROVIDER_REMOVED), new MonaAssetProviderRemovedEvent(_monaAssets[i]));
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public static MonaBrainGraph CreateFromJson(string json)
        {
            var brain = ScriptableObject.CreateInstance<MonaBrainGraph>();
            JsonUtility.FromJsonOverwrite(json, brain);
            return brain;
        }

    }
}