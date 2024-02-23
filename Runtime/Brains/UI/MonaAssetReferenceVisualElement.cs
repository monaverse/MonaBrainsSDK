using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.Assets;
using Mona.SDK.Core.Assets.Interfaces;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaAssetReferenceVisualElement : VisualElement
    {
        private IMonaBrain _brain;
        private int _listIndex;

        private DropdownField _monaAssetField;

        private List<MonaAssetsDefinition> _assetSource;

        public MonaAssetReferenceVisualElement()
        {
            style.flexDirection = FlexDirection.Row;

            _monaAssetField = new DropdownField();
            _monaAssetField.style.flexGrow = 1;
            _monaAssetField.choices = GetMonaAssetProviders();
            _monaAssetField.RegisterValueChangedCallback((evt) =>
            {
                AssignAsset(evt.newValue);
            });
            Add(_monaAssetField);

            var button = new Button();
            button.text = "Find";
            button.style.width = 50;
            button.clicked += () =>
            {
                for (var i = 0; i < _assetSource.Count; i++)
                {
                    if (_assetSource[i].MonaAsset.Name == _brain.MonaAssets[_listIndex].Name)
                    {
#if UNITY_EDITOR
                        Selection.activeObject = (MonaAssetsDefinition)_assetSource[i];
#endif
                        return;
                    }
                }
            };
            Add(button);
        }

        private void AssignAsset(string name)
        {
            var item = _assetSource.Find(x => x.MonaAsset.Name == name);
            if (item != null)
                _brain.MonaAssets[_listIndex] = item.MonaAsset;
        }

        public void SetValue(IMonaBrain brain, int idx)
        {
            _brain = brain;
            _listIndex = idx;

            if (_brain.MonaAssets[_listIndex] != null)
            {
                _monaAssetField.value = _brain.MonaAssets[_listIndex].Name;
                AssignAsset(_brain.MonaAssets[_listIndex].Name);
            }
        }

        private List<string> GetMonaAssetProviders()
        {
            if (_assetSource == null)
            {
                _assetSource = new List<MonaAssetsDefinition>();
#if UNITY_EDITOR
                string[] guids = AssetDatabase.FindAssets("t:MonaAssetsDefinition", null);
                _assetSource.Clear();
                foreach (string guid in guids)
                    _assetSource.Add(((MonaAssetsDefinition)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guid), typeof(MonaAssetsDefinition))));
#endif
            }
            var list = _assetSource.ConvertAll<string>(x => x.MonaAsset.Name);
            list.Insert(0, "Select Asset");
            return list;
        }
    }
}