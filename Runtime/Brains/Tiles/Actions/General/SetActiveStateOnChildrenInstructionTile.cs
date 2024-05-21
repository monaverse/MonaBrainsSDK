using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.Core.State.Structs;
using Mona.SDK.Core.State.Structs;
using Unity.Profiling;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class SetActiveStateOnChildrenInstructionTile : InstructionTile, IInstructionTileWithPreload, IActionInstructionTile
    {
        public const string ID = "SetActiveStateOnChildren";
        public const string NAME = "Children On/Off";
        public const string CATEGORY = "Activation";
        public override Type TileType => typeof(SetActiveStateOnChildrenInstructionTile);

        [SerializeField] private bool _setActive = true;
        [SerializeField] private string _setActiveName;
        [BrainProperty(true)] public bool SetActive { get => _setActive; set => _setActive = value; }
        [BrainPropertyValueName("SetActive", typeof(IMonaVariablesBoolValue))] public string SetActiveName { get => _setActiveName; set => _setActiveName = value; }

        [SerializeField] private ObjectActivationType _objectsToSet;
        [BrainPropertyEnum(true)] public ObjectActivationType ObjectsToSet { get => _objectsToSet; set => _objectsToSet = value; }

        [SerializeField] private float _index;
        [SerializeField] private string _indexName;
        [BrainPropertyShow(nameof(ObjectsToSet), (int)ObjectActivationType.AtIndex)]
        [BrainProperty(true)] public float Index { get => _index; set => _index = value; }
        [BrainPropertyValueName("Index", typeof(IMonaVariablesFloatValue))] public string IndexName { get => _indexName; set => _indexName = value; }

        [SerializeField] private string _stringValue;
        [SerializeField] private string _stringValueName;
        [BrainPropertyShow(nameof(ObjectsToSet), (int)ObjectActivationType.WithName)]
        [BrainPropertyShow(nameof(ObjectsToSet), (int)ObjectActivationType.ContainingString)]
        [BrainProperty(true)] public string StringValue { get => _stringValue; set => _stringValue = value; }
        [BrainPropertyValueName("StringValue", typeof(IMonaVariablesStringValue))] public string StringValueName { get => _stringValueName; set => _stringValueName = value; }

        [SerializeField] private bool _childrenOfChildren = false;
        [SerializeField] private string _childrenOfChildrenName;
        [BrainProperty(false)] public bool ChildrenOfChildren { get => _childrenOfChildren; set => _childrenOfChildren = value; }
        [BrainPropertyValueName("ChildrenOfChildren", typeof(IMonaVariablesBoolValue))] public string ChildrenOfChildrenName { get => _childrenOfChildrenName; set => _childrenOfChildrenName = value; }

        static readonly ProfilerMarker _profilerDo = new ProfilerMarker($"MonaBrains.{nameof(SetActiveStateOnChildrenInstructionTile)}.{nameof(Do)}");

        private IMonaBrain _brain;
        private Transform _bodyTransform;

        public enum ObjectActivationType
        {
            All = 0,
            RandomOne = 10,
            RandomAny = 20,
            AtIndex = 30,
            WithName = 40,
            ContainingString = 50
        }

        public SetActiveStateOnChildrenInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            _profilerDo.Begin();

            if (_brain == null)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            _bodyTransform = _brain.Body.Transform;

            if (_bodyTransform == null || _bodyTransform.childCount < 1)
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_setActiveName))
                _setActive = _brain.Variables.GetBool(_setActiveName);

            if (!string.IsNullOrEmpty(_indexName))
                _index = _brain.Variables.GetFloat(_indexName);

            if (!string.IsNullOrEmpty(_stringValueName))
                _stringValue = _brain.Variables.GetString(_stringValueName);

            if (!string.IsNullOrEmpty(_childrenOfChildrenName))
                _childrenOfChildren = _brain.Variables.GetBool(_childrenOfChildrenName);

            Transform parent = _bodyTransform;

            switch (_objectsToSet)
            {
                case ObjectActivationType.All:
                    SetActivationOnAll(parent);
                    break;
                case ObjectActivationType.RandomOne:
                    SetActivationOnRandom(parent, false);
                    break;
                case ObjectActivationType.RandomAny:
                    SetActivationOnRandom(parent, true);
                    break;
                case ObjectActivationType.AtIndex:
                    SetActiveAtIndex(parent, (int)_index);
                    break;
                case ObjectActivationType.WithName:
                    SetActiveWithName(parent, true);
                    break;
                case ObjectActivationType.ContainingString:
                    SetActiveWithName(parent, false);
                    break;
            }

            _profilerDo.End();
            return Complete(InstructionTileResult.Success);
        }

        private void SetActivationOnAll(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                SetActivationState(parent.GetChild(i).gameObject);

                if (_childrenOfChildren)
                    SetActivationOnAll(parent.GetChild(i));
            }
        }

        private void SetActivationOnRandom(Transform parent, bool randomOnAll)
        {
            if (randomOnAll)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                    {
                        SetActiveAtIndex(parent, i);

                        if (_childrenOfChildren)
                            SetActivationOnRandom(parent.GetChild(i), randomOnAll);
                    }
                }
            }
            else
            {
                int randomIndex = UnityEngine.Random.Range(0, parent.childCount);
                SetActiveAtIndex(parent, randomIndex);

                if (_childrenOfChildren)
                    SetActivationOnRandom(parent.GetChild(randomIndex), randomOnAll);
            }
        }

        private void SetActiveAtIndex(Transform parent, int index)
        {
            if (index < 0 || index >= parent.childCount)
                return;

            SetActivationState(parent.GetChild(index).gameObject);
        }

        private void SetActiveWithName(Transform parent, bool useWholeName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                GameObject go = parent.GetChild(i).gameObject;
                bool setActiveState = useWholeName ? go.name == _stringValue : go.name.Contains(_stringValue);

                if (setActiveState)
                    SetActivationState(go);

                if (_childrenOfChildren)
                    SetActiveWithName(parent.GetChild(i), useWholeName);
            }
        }

        private void SetActivationState(GameObject gameObject)
        {
            IMonaBody monaBody = gameObject.GetComponent<IMonaBody>();

            if (monaBody == null)
            {
                if(gameObject.activeSelf != _setActive)
                    gameObject.SetActive(_setActive);
                return;
            }

            //Debug.Log($"{nameof(SetActivationState)} {_brain.Name} {_setActive} {gameObject.activeInHierarchy} {gameObject.activeSelf}", gameObject);
            //if(monaBody.GetActive() != _setActive)
                monaBody.SetActive(_setActive);

            if (!_setActive)
                return;

            //if (monaBody.ActiveRigidbody != null)
            //    monaBody.ActiveRigidbody.WakeUp();

            if (_setActive)
            {
                var childBrains = monaBody.Children();
                for (var i = 0; i < childBrains.Count; i++)
                {
                    var runner = childBrains[i].Transform.GetComponent<IMonaBrainRunner>();
                    if(runner != null)
                        runner.CacheTransforms();
                }
            }
        }
    }
}