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

        private IMonaBrain _brain;
        private List<IMonaBody> _children = new List<IMonaBody>();
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

            switch (_objectsToSet)
            {
                case ObjectActivationType.All:
                    SetActivationOnAll();
                    break;
                case ObjectActivationType.RandomOne:
                    SetActivationOnRandom(false);
                    break;
                case ObjectActivationType.RandomAny:
                    SetActivationOnRandom(true);
                    break;
                case ObjectActivationType.AtIndex:
                    SetActiveAtIndex((int)_index);
                    break;
                case ObjectActivationType.WithName:
                    SetActiveWithName(true);
                    break;
                case ObjectActivationType.ContainingString:
                    SetActiveWithName(false);
                    break;
            }

            return Complete(InstructionTileResult.Success);
        }

        private void SetActivationOnAll()
        {
            for (int i = 0; i < _bodyTransform.childCount; i++)
                SetActivationState(_bodyTransform.GetChild(i).gameObject);
        }

        private void SetActivationOnRandom(bool randomOnAll)
        {
            if (randomOnAll)
            {
                for (int i = 0; i < _bodyTransform.childCount; i++)
                {
                    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        SetActiveAtIndex(i);
                }
            }
            else
            {
                int randomIndex = UnityEngine.Random.Range(0, _bodyTransform.childCount);
                SetActiveAtIndex(randomIndex);
            }
        }

        private void SetActiveAtIndex(int index)
        {
            if (index < 0 || index >= _bodyTransform.childCount)
                return;

            SetActivationState(_bodyTransform.GetChild(index).gameObject);
        }

        private void SetActiveWithName(bool useWholeName)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                GameObject go = _bodyTransform.GetChild(i).gameObject;
                bool setActiveState = useWholeName ? go.name == _stringValue : go.name.Contains(_stringValue);

                if (setActiveState)
                    SetActivationState(go);
            }
        }

        private void SetActivationState(GameObject gameObject)
        {
            gameObject.SetActive(_setActive);
            IMonaBody monaBody = gameObject.GetComponent<IMonaBody>();

            if (monaBody == null)
                return;

            monaBody.SetActive(true);

            if (monaBody.ActiveRigidbody != null)
                monaBody.ActiveRigidbody.WakeUp();

            var childBrains = monaBody.Transform.GetComponentsInChildren<IMonaBrainRunner>();
            for (var i = 0; i < childBrains.Length; i++)
                childBrains[i].CacheTransforms();
        }
    }
}