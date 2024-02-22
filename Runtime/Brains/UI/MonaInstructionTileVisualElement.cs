using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using Mona.SDK.Brains.Core.Events;
using Unity.VisualScripting;
using Mona.SDK.Brains.Core.Control;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Mona.SDK.Brains.UIElements
{
    public class MonaInstructionTileVisualElement : VisualElement
    {
        public event Action<int, bool> OnClicked;

        public event Action OnHeight;

        private IMonaBrain _brain;
        private IMonaBrainPage _page;
        private IInstructionTile _tile;
        private int _index;

        private VisualElement _toolBar;
        private VisualElement _valuesContainer;
        private VisualElement _values;
        private VisualElement _propertyValues;
        private ScrollView _valuesExtended;

        private Label _label;
        private Label _labelMore;
        private Label _labelType;
#if UNITY_EDITOR
        private ToolbarMenu _toolbarMenu;
#endif
        private bool _extended;

        private ListView _instructionListView;
        private IManipulator _click;
        private IManipulator _clickLabel;

        private Sprite _expandIcon;
        private Sprite _collapseIcon;

        public MonaInstructionTileVisualElement()
        {
#if UNITY_EDITOR
            _expandIcon = (Sprite)AssetDatabase.LoadAssetAtPath("Packages/com.monaverse.brainssdk/Runtime/Resources/tile_expand.png", typeof(Sprite));
            _collapseIcon = (Sprite)AssetDatabase.LoadAssetAtPath("Packages/com.monaverse.brainssdk/Runtime/Resources/tile_collapse.png", typeof(Sprite));
#endif


            style.flexDirection = FlexDirection.Column;

            _toolBar = new VisualElement();
            _toolBar.style.flexDirection = FlexDirection.Row;
            _toolBar.style.height = 20;
            _toolBar.style.paddingRight = 0;
            _toolBar.style.alignItems = Align.FlexEnd;
            _toolBar.style.flexGrow = 1;

            _labelType = new Label();
            _labelType.style.fontSize = 10;
            _labelType.style.color = Color.white;
            _labelType.style.unityTextAlign = TextAnchor.MiddleCenter;
            _labelType.style.paddingLeft = 16;
            _labelType.style.flexGrow = 1;
            _toolBar.Add(_labelType);

#if UNITY_EDITOR
            _labelMore = new Label();
            _labelMore.style.backgroundImage = new StyleBackground(_expandIcon);
            _labelMore.style.width = _labelMore.style.height = 16;
            _labelMore.style.color = _textColor;
            _labelMore.style.unityFontStyleAndWeight = FontStyle.Bold;
#endif
            Add(_toolBar);

            _label = new Label();
            _label.style.flexGrow = 1;
            _label.style.fontSize = 14;
            _label.style.minHeight = 12;
            _label.style.alignItems = Align.Center;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.style.marginRight = 5;
            _label.style.color = _textColor;
            _label.style.paddingBottom = 3;

            Add(_label);

            _valuesContainer = new VisualElement();
            _valuesContainer.style.width = Length.Percent(100);
            _valuesContainer.style.height = Length.Percent(100);
            _valuesContainer.style.flexDirection = FlexDirection.Row;
            Add(_valuesContainer);

            _values = new VisualElement();
            _values.style.height = Length.Percent(100);
            _values.style.flexDirection = FlexDirection.Column;
            _values.style.alignItems = Align.Center;
            _values.style.marginRight = 10;
            _valuesContainer.Add(_values);

            _propertyValues = new VisualElement();
            _propertyValues.style.width = Length.Percent(100);
            _propertyValues.style.height = Length.Percent(100);
            _propertyValues.style.flexDirection = FlexDirection.Column;
            _propertyValues.style.alignItems = Align.Center;
            _values.Add(_propertyValues);

            _valuesExtended = new ScrollView();
            _valuesExtended.style.minWidth = 150;
            _valuesExtended.style.height = Length.Percent(100);
            _valuesExtended.style.flexGrow = 1;
            _valuesExtended.style.flexDirection = FlexDirection.Column;
            _valuesExtended.style.alignItems = Align.Center;
            _valuesExtended.verticalScrollerVisibility = ScrollerVisibility.Auto;
            _valuesExtended.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            _valuesExtended.style.display = DisplayStyle.None;
            _valuesExtended.style.paddingLeft = _valuesExtended.style.paddingRight = 10;
            _valuesExtended.style.borderLeftWidth = 1;
            _valuesExtended.style.borderLeftColor = Color.black;
            _valuesContainer.Add(_valuesExtended);

        }

        public void Select(bool expand)
        {
            if(expand && _valuesExtended.childCount > 0)
                Extend(true);
            SetBorder(2, Color.white);
        }

        public void Deselect()
        {
            Extend(false);
            SetBorder(2, Color.black);
        }

        private void SetBorder(int i, Color c)
        {
            style.borderBottomWidth = style.borderTopWidth = style.borderLeftWidth = style.borderRightWidth = i;
            style.borderBottomColor = style.borderTopColor = style.borderLeftColor = style.borderRightColor = c;
        }

        private void HandleClick(int i, bool expand)
        {
            OnClicked?.Invoke(i, expand);
        }

        private void Extend(bool extend)
        {            
            _extended = extend;
            _valuesExtended.style.display = _extended ? DisplayStyle.Flex : DisplayStyle.None;
            if(extend)
                _labelMore.style.backgroundImage = new StyleBackground(_collapseIcon);
            else
                _labelMore.style.backgroundImage = new StyleBackground(_expandIcon);
        }

        public void SetInstructionTile(IMonaBrain brain, IMonaBrainPage page, IInstructionTile tile, int tileIndex, int instructionTileCount)
        {
            _brain = brain;
            _page = page;
            _tile = tile;
            _index = tileIndex;

            if (_click != null)
                this.RemoveManipulator(_click);
            if (_clickLabel != null)
                _labelMore.RemoveManipulator(_clickLabel);

            _click = new Clickable(evt => HandleClick(tileIndex, false));
            this.AddManipulator(_click);

            _clickLabel = new Clickable(evt => HandleClick(tileIndex, !_extended));
            _labelMore.AddManipulator(_clickLabel);

            if (_tile == null) return;

            SetStyle(_tile);
            BuildValueEditor();

            _label.text = _tile.Name;
            if (_valuesExtended.childCount > 0 && _labelMore.parent == null)
            {
                _toolBar.Add(_labelMore);
                _labelType.style.paddingLeft = 16;
            }
            else if (_labelMore.parent != null)
            {
                _toolBar.Remove(_labelMore);
                _labelType.style.paddingLeft = 0;
            }
            else
            {
                _labelType.style.paddingLeft = 0;
            }
        }

        private void Changed()
        {
            EventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brain.Guid), new MonaBrainReloadEvent());
        }

        private void BuildValueEditor()
        {
            _propertyValues.Clear();
            _valuesExtended.Clear();
            var container = _values;
            var properties = new List<PropertyInfo>(_tile.GetType().GetProperties());
            var count = 0;
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var attribute = (BrainProperty)property.GetCustomAttribute(typeof(BrainProperty), true);
                if (attribute == null) continue;
                
                var show = attribute.ShowOnTile;
                container = show ? _propertyValues : _valuesExtended;
                count++;

                var fieldContainer = new VisualElement();
                fieldContainer.style.flexDirection = FlexDirection.Row;
                fieldContainer.style.alignItems = Align.FlexEnd;
                container.Add(fieldContainer);

                var isAsset = (BrainPropertyMonaAsset)property.GetCustomAttribute(typeof(BrainPropertyMonaAsset), true);
                var isTag = (BrainPropertyMonaTag)property.GetCustomAttribute(typeof(BrainPropertyMonaTag), true);
                var isValue = (BrainPropertyValue)property.GetCustomAttribute(typeof(BrainPropertyValue), true);

                if (isAsset != null)
                {
                    var values = _brain.GetAllMonaAssets().FindAll(x => isAsset.Type.IsAssignableFrom(x.GetType())).ConvertAll<string>(x => x.PrefabId);
                    var field = new DropdownField(values, 0);
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterCallback<FocusInEvent>((evt) =>
                    {
                        values = _brain.GetAllMonaAssets().FindAll(x => isAsset.Type.IsAssignableFrom(x.GetType())).ConvertAll<string>(x => x.PrefabId);
                        field.choices = values;
                    });
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        field.value = (string)evt.newValue;
                        property.SetValue(_tile, field.value);
                        Changed();
                    });
                    fieldContainer.Add(field);
                }
                else if (isValue != null)
                {
                    var values = _brain.DefaultVariables.VariableList.FindAll(x => isValue.Type.IsAssignableFrom(_brain.DefaultVariables.GetVariable(x.Name).GetType())).ConvertAll<string>(x => x.Name);
                    var field = new DropdownField(values, 0);
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterCallback<FocusInEvent>((evt) =>
                    {
                        values = _brain.DefaultVariables.VariableList.FindAll(x => isValue.Type.IsAssignableFrom(_brain.DefaultVariables.GetVariable(x.Name).GetType())).ConvertAll<string>(x => x.Name);
                        field.choices = values;
                    });
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        field.value = (string)evt.newValue;
                        property.SetValue(_tile, field.value);
                        Changed();
                    });
                    fieldContainer.Add(field);
                }
                else if (isTag != null)
                {
                    var field = new DropdownField(_brain.MonaTagSource.Tags, 0);
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        field.value = (string)evt.newValue;
                        property.SetValue(_tile, field.value);
                        Changed();
                    });
                    fieldContainer.Add(field);
                }
                else if (property.PropertyType == typeof(string))
                {
                    var field = new TextField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => {
                        property.SetValue(_tile, (string)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(float))
                {
                    var field = new FloatField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (float)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => {
                        property.SetValue(_tile, (float)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(int))
                {
                    var field = new IntegerField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (int)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (int)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var field = new Toggle();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (bool)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (bool)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(Vector2))
                {
                    var field = new Vector2Field();
                    field.style.width = 120;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (Vector2)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (Vector2)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(Vector3))
                {
                    var field = new Vector3Field();
                    field.style.width = 130;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (Vector3)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (Vector3)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType.IsEnum)
                {
                    var type = property.PropertyType;
                    var field = new EnumField((Enum)type.GetEnumValues().GetValue(0));
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (Enum)Enum.Parse(type, property.GetValue(_tile).ToString());
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (Enum)evt.newValue);
                        Changed();
                    });
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
#if UNITY_EDITOR
                else if (property.PropertyType == typeof(Color))
                {
                    var type = property.PropertyType;
                    var field = new ColorField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (Color)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (Color)evt.newValue);
                        Changed();
                    });
                    container.Add(field);
                }
#endif
            }
        }

        private Color _darkRed = Color.HSVToRGB(347f / 360f, .66f, .1f);
        private void AddTargetFieldIfExists(VisualElement fieldContainer, VisualElement field, List<PropertyInfo> properties, PropertyInfo property)
        {
            var targetProperty = properties.Find(x =>
            {
                var attributes = x.GetCustomAttributes(typeof(BrainPropertyValueName), true);
                if (attributes.Length > 0)
                {
                    var brainPropertyValueName = (BrainPropertyValueName)attributes[0];
                    if (brainPropertyValueName.PropertyName == property.Name)
                        return true;
                }
                return false;
            });

            if (targetProperty != null)
            {
                var value = (string)targetProperty.GetValue(_tile);
                var isValue = (BrainPropertyValueName)targetProperty.GetCustomAttribute(typeof(BrainPropertyValueName), true);
                var values = _brain.DefaultVariables.VariableList.FindAll(x => isValue.Type.IsAssignableFrom(_brain.DefaultVariables.GetVariable(x.Name).GetType())).ConvertAll<string>(x => x.Name);   
                var target = new DropdownField(values, 0);

                target.style.width = 80;
                target.style.flexDirection = FlexDirection.Column;
                target.style.display = string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;
                target.labelElement.style.color = _textColor;
                target.label = property.Name;

                var defaultValue = (string)targetProperty.GetValue(_tile);
                var defaultVariable = values.Find(x => string.Compare(x, defaultValue, true) == 0);
                if (defaultVariable != null)
                {
                    target.value = defaultVariable;
                    targetProperty.SetValue(_tile, target.value);
                }

                target.RegisterValueChangedCallback((evt) =>
                {
                    target.value = (string)evt.newValue;
                    targetProperty.SetValue(_tile, target.value);
                    Changed();
                });
                fieldContainer.Add(target);

                field.style.width = 80;
                field.style.display = !string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;

                var btn = new Button();
                btn.style.width = 20;
                btn.style.height = 20;
                btn.style.color = Color.black;
                btn.style.backgroundColor = !string.IsNullOrEmpty(value) ? _darkRed : Color.gray;
                btn.style.color = !string.IsNullOrEmpty(value) ? Color.white : Color.black;
                btn.text = "*";
                btn.clicked += () =>
                {
                    values = _brain.DefaultVariables.VariableList.FindAll(x => isValue.Type.IsAssignableFrom(_brain.DefaultVariables.GetVariable(x.Name).GetType())).ConvertAll<string>(x => x.Name);
                    target.choices = values;
                    var value2 = (string)targetProperty.GetValue(_tile);
                    Debug.Log($"Btn Clicked {targetProperty.Name} {value2}");
                    if (!string.IsNullOrEmpty(value2) || values.Count == 0)
                    {
                        targetProperty.SetValue(_tile, null);
                        field.style.display = DisplayStyle.Flex;
                        target.style.display = DisplayStyle.None;
                        btn.style.backgroundColor = Color.gray;
                        btn.style.color = Color.black;
                    }
                    else
                    {
                        var defaultVariable = values.Find(x => string.Compare(x, property.Name, true) == 0);
                        if (defaultVariable != null)
                        {
                            targetProperty.SetValue(_tile, defaultVariable);
                            target.value = defaultVariable;
                        }
                        else if(values.Count > 0)
                        {
                            targetProperty.SetValue(_tile, values[0]);
                        }
                        field.style.display = DisplayStyle.None;
                        target.style.display = DisplayStyle.Flex;
                        btn.style.backgroundColor = _darkRed;
                        btn.style.color = Color.white;
                    }
                };
                fieldContainer.Add(btn);
            }
        }

        private void CopyToTile(IInstructionTileDefinition def)
        {
            def.Tile.Id = def.Id;
            def.Tile.Name = def.Name;
            def.Tile.Category = def.Category;
        }

        private Color _brightPink = Color.HSVToRGB(351f / 360f, .79f, .98f);
        private Color _lightRed = Color.HSVToRGB(347f / 360f, .80f, .66f);
        private Color _textColor = Color.white;

        private void SetStyle(IInstructionTile tile)
        {
            style.color = Color.black;
            style.minWidth = 100;
            style.minHeight = 120;
            style.borderBottomLeftRadius = style.borderBottomRightRadius = style.borderTopLeftRadius = style.borderTopRightRadius = 5;
            SetBorder(2, Color.black);
            style.paddingRight = style.paddingLeft = 5;
            style.paddingTop = style.paddingBottom = 5;

            if (tile is IActionInstructionTile)
            {
                _labelType.text = "ACTION";
                style.backgroundColor = _brightPink;
                if (tile is IActionStateEndInstructionTile && !_page.IsCore)
                    style.borderBottomRightRadius = style.borderTopRightRadius = 30;
                if (tile is IActionEndInstructionTile)
                    style.borderBottomRightRadius = style.borderTopRightRadius = 30;
            }
            else if (tile is IConditionInstructionTile)
            {
                if (tile is IInputInstructionTile)
                    _labelType.text = "INPUT";
                else
                    _labelType.text = "IF";
                style.backgroundColor = _lightRed;
                if (tile is IStartableInstructionTile && _index == 0)
                    style.borderBottomLeftRadius = style.borderTopLeftRadius = 30;
            }

        }

    }
}