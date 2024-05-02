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
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Core.Assets.Interfaces;
using Mona.SDK.Core.Utils;

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
            _labelMore.style.color = Color.white;
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
            _label.style.color = Color.white;
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
            if (expand && _valuesExtended.childCount > 0)
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
            if (extend)
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

            _tile.OnMuteChanged -= HandleMuted;
            _tile.OnMuteChanged += HandleMuted;

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

        private void HandleMuted()
        {
            SetStyle(_tile);
        }

        private void Changed()
        {
            MonaEventBus.Trigger(new EventHook(MonaBrainConstants.BRAIN_RELOAD_EVENT, _brain.Guid), new MonaBrainReloadEvent());
        }

        private void BuildValueEditor()
        {
            //TODO REFACTOR

            _propertyValues.Clear();
            _valuesExtended.Clear();
            var container = _values;
            var properties = new List<PropertyInfo>(_tile.GetType().GetProperties());
            var count = 0;

            var fieldDictionary = new Dictionary<string, VisualElement>();
            var targetFieldDictionary = new Dictionary<string, VisualElement>();
            var targetFieldVisible = new Dictionary<string, bool>();

            var buttonDictionary = new Dictionary<string, Button>();
            var showAttributes = new Dictionary<string, List<BrainPropertyShow>>();
            for (var i = 0; i < properties.Count; i++)
            {
                var property = properties[i];
                var isVisible = (BrainPropertyShow[])property.GetCustomAttributes(typeof(BrainPropertyShow), true);

                for (var j = 0; j < isVisible.Length; j++)
                {
                    if (isVisible[j] != null)
                    {
                        if (!showAttributes.ContainsKey(property.Name))
                            showAttributes.Add(property.Name, new List<BrainPropertyShow>());

                        showAttributes[property.Name].Add(isVisible[j]);
                    }
                }
            }

            Action CheckVisibleChange = () =>
            {
                foreach (var pair in showAttributes)
                {
                    // fieldDictionary[pair.Key].style.display = DisplayStyle.Flex; 
                    for (var i = 0; i < pair.Value.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (pair.Value.FindAll(x => x is BrainPropertyShowLabel).Count < pair.Value.Count)
                            {
                                fieldDictionary[pair.Key].style.display = DisplayStyle.None;

                                if (targetFieldVisible.ContainsKey(pair.Key))
                                    targetFieldDictionary[pair.Key].style.display = DisplayStyle.None;

                                if (buttonDictionary.ContainsKey(pair.Key) && buttonDictionary[pair.Key] != null)
                                    buttonDictionary[pair.Key].style.display = DisplayStyle.None;
                            }

                            var field = fieldDictionary[pair.Key];
                            var prop = (new List<PropertyInfo>(field.GetType().GetProperties())).Find(x => x.Name == "labelElement");
                            if (prop != null)
                            {
                                var label = (Label)prop.GetValue(field);
                                label.text = pair.Key;
                            }
                        }


                        if (pair.Value[i] is BrainPropertyShowLabel)
                        {
                            var labelAttribute = (BrainPropertyShowLabel)pair.Value[i];
                            var otherProperty = _tile.GetType().GetProperty(pair.Value[i].Name);
                            if (((int)otherProperty.GetValue(_tile)) == pair.Value[i].Value)
                            {
                                var field2 = fieldDictionary[pair.Key];
                                var prop2 = (new List<PropertyInfo>(field2.GetType().GetProperties())).Find(x => x.Name == "labelElement");
                                if (prop2 != null)
                                {
                                    var label = (Label)prop2.GetValue(field2);
                                    label.text = labelAttribute.Label;
                                }
                            }
                            if (pair.Value.FindAll(x => x is BrainPropertyShowLabel).Count == pair.Value.Count)
                            {
                                if (targetFieldVisible.ContainsKey(pair.Key))
                                {
                                    if (targetFieldVisible[pair.Key])
                                    {
                                        fieldDictionary[pair.Key].style.display = DisplayStyle.Flex;
                                        targetFieldDictionary[pair.Key].style.display = DisplayStyle.None;
                                    }
                                    else
                                    {
                                        fieldDictionary[pair.Key].style.display = DisplayStyle.None;
                                        targetFieldDictionary[pair.Key].style.display = DisplayStyle.Flex;
                                    }
                                }
                                else
                                    fieldDictionary[pair.Key].style.display = DisplayStyle.Flex;

                                if (buttonDictionary.ContainsKey(pair.Key) && buttonDictionary[pair.Key] != null)
                                    buttonDictionary[pair.Key].style.display = DisplayStyle.Flex;
                            }
                        }
                        else
                        {
                            var otherProperty = _tile.GetType().GetProperty(pair.Value[i].Name);
                            if(otherProperty == null)
                            {
                                Debug.LogError($"{nameof(CheckVisibleChange)} cannot find property {pair.Value[i].Name}");
                            }
                            if (
                                (!pair.Value[i].UseBoolValue && ((int)otherProperty.GetValue(_tile)) == pair.Value[i].Value)
                                || (pair.Value[i].UseBoolValue && ((bool)otherProperty.GetValue(_tile)) == pair.Value[i].BoolValue)
                                )
                            {
                                if (targetFieldVisible.ContainsKey(pair.Key))
                                {
                                    if (targetFieldVisible[pair.Key])
                                    {
                                        fieldDictionary[pair.Key].style.display = DisplayStyle.Flex;
                                        targetFieldDictionary[pair.Key].style.display = DisplayStyle.None;
                                    }
                                    else
                                    {
                                        fieldDictionary[pair.Key].style.display = DisplayStyle.None;
                                        targetFieldDictionary[pair.Key].style.display = DisplayStyle.Flex;
                                    }
                                }
                                else
                                    fieldDictionary[pair.Key].style.display = DisplayStyle.Flex;

                                if (buttonDictionary.ContainsKey(pair.Key) && buttonDictionary[pair.Key] != null)
                                    buttonDictionary[pair.Key].style.display = DisplayStyle.Flex;
                            }

                        }
                    }
                }
            };

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
                    List<string> values;
                    if(isAsset.UseProviders)
                        values = _brain.GetAllMonaAssetProviders().FindAll(x => {
                            if (x.AllAssets.Count > 0)
                                return isAsset.Type.IsAssignableFrom(x.GetMonaAssetByIndex(0).GetType());
                            return false;
                        }).ConvertAll<string>(x => x.Name);
                    else
                        values = _brain.GetAllMonaAssets().FindAll(x => isAsset.Type.IsAssignableFrom(x.GetType())).ConvertAll<string>(x => x.PrefabId);

                    var field = new DropdownField(values, 0);
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterCallback<FocusInEvent>((evt) =>
                    {
                        if (isAsset.UseProviders)
                            values = _brain.GetAllMonaAssetProviders().FindAll(x => {
                                    if (x.AllAssets.Count > 0)
                                        return isAsset.Type.IsAssignableFrom(x.GetMonaAssetByIndex(0).GetType());
                                    return false;
                            }).ConvertAll<string>(x => x.Name);
                        else
                            values = _brain.GetAllMonaAssets().FindAll(x => isAsset.Type.IsAssignableFrom(x.GetType())).ConvertAll<string>(x => x.PrefabId);

                        field.choices = values;
                    });
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        field.value = (string)evt.newValue;
                        property.SetValue(_tile, field.value);
                        Changed();
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);
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
                        CheckVisibleChange();
                    });
                    field.RegisterCallback<KeyDownEvent>(evt =>
                    {
                        evt.StopPropagation();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
                }
                else if (property.PropertyType == typeof(Vector3))
                {
                    fieldContainer.style.minWidth = 180;
                    var field = new Vector3Field();
                    field.style.flexDirection = FlexDirection.Column;
                    field.style.minWidth = 160;
                    field.labelElement.style.color = _textColor;
                    field.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
                    field.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (Vector3)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        property.SetValue(_tile, (Vector3)evt.newValue);
                        Changed();
                        CheckVisibleChange();
                    });

                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    var fields = field.Query<FloatField>().Build();
                    fields.ForEach<FloatField>(x =>
                    {
                        //x.style.minWidth = 30;
                        return x;
                    });

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    fieldContainer.Add(field);
                    fieldDictionary.Add(property.Name, field);

                    buttonDictionary.Add(property.Name, AddTargetFieldIfExists(fieldContainer, field, properties, property, targetFieldDictionary, targetFieldVisible));
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
                        CheckVisibleChange();
                    });
                    container.Add(field);
                    fieldDictionary.Add(property.Name, field);
                }
#endif
            }
            CheckVisibleChange();
        }

        private Color _darkRed = Color.HSVToRGB(347f / 360f, .66f, .1f);
        private Button AddTargetFieldIfExists(VisualElement fieldContainer, VisualElement field, List<PropertyInfo> properties, PropertyInfo property, Dictionary<string, VisualElement> targetFieldDictionary, Dictionary<string, bool> targetFieldVisible)
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
                var btn = CreateTargetPropertyButton(targetProperty, property, fieldContainer, field, targetFieldDictionary, targetFieldVisible);
                return btn;
            }
            return null;
        }

        private Button CreateTargetPropertyButton(PropertyInfo targetProperty, PropertyInfo property, VisualElement fieldContainer, VisualElement field, Dictionary<string, VisualElement> targetFieldDictionary, Dictionary<string, bool> targetFieldVisible)
        {
            var isValue = (BrainPropertyValueName)targetProperty.GetCustomAttribute(typeof(BrainPropertyValueName), true);

            var btn = CreateBindButton();
            var fields = new List<DropdownField>();
            var isVector3 = isValue.Type.IsAssignableFrom(typeof(IMonaVariablesVector3Value));
            var isVector2 = isValue.Type.IsAssignableFrom(typeof(IMonaVariablesVector2Value));
            var hasDefaultValue = false;

            if (isVector3 || isVector2)
            {
                var xyzContainer = new VisualElement();
                targetFieldDictionary[isValue.PropertyName] = xyzContainer;

                if (isVector3)
                    fields.Add(CreateDropdownField($"{property.Name}", targetProperty, xyzContainer, 0));
                else
                    fields.Add(CreateDropdownField($"{property.Name}", targetProperty, xyzContainer, 0));

                xyzContainer.style.flexDirection = FlexDirection.Column;

                var xyzSubContainer = new VisualElement();
                xyzSubContainer.style.flexGrow = 1;
                xyzSubContainer.style.flexDirection = FlexDirection.Row;
                xyzContainer.Add(xyzSubContainer);

                fields.Add(CreateDropdownField($"X", targetProperty, xyzSubContainer, 1));
                fields.Add(CreateDropdownField($"Y", targetProperty, xyzSubContainer, 2));

                if (isVector3)
                    fields.Add(CreateDropdownField($"Z", targetProperty, xyzSubContainer, 3));

                fieldContainer.Add(xyzContainer);
                
                var selectedValues = (string[])targetProperty.GetValue(_tile);
                var hasBound = false;

                btn.clicked += () =>
                {
                    List<string> values = new List<string>();
                    var selectedValues = (string[])targetProperty.GetValue(_tile);
                    
                    for (var i = 0; i < selectedValues.Length; i++)
                    {
                        if (i == 0)
                            values = GetVariableList(isValue.Type);
                        else
                            values = GetVariableList(typeof(IMonaVariablesFloatValue));

                        values.Insert(0, "_");

                        fields[i].choices = values;

                        var selectedValue = selectedValues[i];

                        if (string.IsNullOrEmpty(selectedValue))
                        {
                            var targetPropertyValue = (string[])targetProperty.GetValue(_tile);

                            if (i == 0)
                            {
                                var defaultVariable = values.Find(x => string.Compare(x, property.Name, true) == 0);
                                if (defaultVariable != null)
                                    fields[i].value = defaultVariable;
                                else if (values.Count > 0)
                                    fields[i].value = values[0];
                            }

                            if(fields[i].value == "_")
                                targetPropertyValue[i] = null;
                            else
                                targetPropertyValue[i] = fields[i].value;
                            targetProperty.SetValue(_tile, targetPropertyValue);
                        }
                        else
                        {
                            hasBound = true;
                        }
                    }

                    if (values.Count == 0 || hasBound)
                    {
                        hasBound = false;

                        if (isVector3)
                            targetProperty.SetValue(_tile, new string[4]);
                        else
                            targetProperty.SetValue(_tile, new string[3]);

                        field.style.display = DisplayStyle.Flex;

                        xyzContainer.style.display = DisplayStyle.None;
                        targetFieldVisible[isValue.PropertyName] = true;

                        btn.style.backgroundColor = Color.gray;
                        btn.style.color = Color.black;
                    }
                    else
                    {
                        xyzContainer.style.display = DisplayStyle.Flex;
                        targetFieldVisible[isValue.PropertyName] = false;

                        field.style.display = DisplayStyle.None;

                        btn.style.backgroundColor = _darkRed;
                        btn.style.color = Color.white;

                        hasBound = true;
                    }
                };

                for(var i = 0;i < selectedValues.Length; i++)
                {
                    if (!string.IsNullOrEmpty(selectedValues[i]))
                        hasDefaultValue = true;
                }

                if (hasDefaultValue)
                {
                    xyzContainer.style.display = DisplayStyle.Flex;
                    targetFieldVisible[isValue.PropertyName] = false;
                }
                else
                {
                    xyzContainer.style.display = DisplayStyle.None;
                    targetFieldVisible[isValue.PropertyName] = true;
                }
            }
            else
            {
                var target = CreateDropdownField(property.Name, targetProperty, fieldContainer);
                targetFieldDictionary[isValue.PropertyName] = target;

                var selectedValue = (string)targetProperty.GetValue(_tile);

                btn.clicked += () =>
                {
                    var values = GetVariableList(isValue.Type);
                    var selectedValue = (string)targetProperty.GetValue(_tile);

                    target.choices = values;
                    target.style.display = DisplayStyle.None;
                    
                    if (string.IsNullOrEmpty(selectedValue))
                    {
                        var targetPropertyValue = (string)targetProperty.GetValue(_tile);

                        var defaultVariable = values.Find(x => string.Compare(x, property.Name, true) == 0);
                        if (defaultVariable != null)
                            target.value = defaultVariable;
                        else if (values.Count > 1)
                            target.value = values[1];

                        targetPropertyValue = target.value;
                        targetProperty.SetValue(_tile, targetPropertyValue);
                    }
                    
                    if (values.Count == 0 || !string.IsNullOrEmpty(selectedValue))
                    {
                        targetProperty.SetValue(_tile, null);
                        field.style.display = DisplayStyle.Flex;

                        target.style.display = DisplayStyle.None;
                        targetFieldVisible[isValue.PropertyName] = true;

                        btn.style.backgroundColor = Color.gray;
                        btn.style.color = Color.black;
                    }
                    else
                    {
                        target.style.display = DisplayStyle.Flex;
                        targetFieldVisible[isValue.PropertyName] = false;

                        field.style.display = DisplayStyle.None;
                        btn.style.backgroundColor = _darkRed;
                        btn.style.color = Color.white;
                    }
                };

                if (!string.IsNullOrEmpty(selectedValue))
                    hasDefaultValue = true;

                if (hasDefaultValue)
                {
                    target.style.display = DisplayStyle.Flex;
                    targetFieldVisible[isValue.PropertyName] = false;
                }
                else
                {
                    targetFieldVisible[isValue.PropertyName] = true;
                }
            }

            field.style.width = 80;
            field.style.display = hasDefaultValue ? DisplayStyle.None : DisplayStyle.Flex;
            fieldContainer.Add(btn);

            return btn;
        }

        private DropdownField CreateDropdownField(string name, PropertyInfo targetProperty, VisualElement fieldContainer)
        {
            var value = targetProperty.GetValue(_tile);
            var isValue = (BrainPropertyValueName)targetProperty.GetCustomAttribute(typeof(BrainPropertyValueName), true);
            var type = isValue.Type;

            var values = GetVariableList(isValue.Type);
            var defaultValue = "";

            var defaultVariable = values.Find(x => string.Compare(x, (string)value, true) == 0);
            var target = new DropdownField(values, 0);

            if(defaultVariable != null)
            {
                target.value = defaultVariable;
                targetProperty.SetValue(_tile, target.value);
            }

            target.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue == "_")
                    target.value = null;
                else
                    target.value = (string)evt.newValue;
                targetProperty.SetValue(_tile, target.value);
                Changed();
            });

            target.style.width = 80;
            target.style.flexDirection = FlexDirection.Column;
            target.style.display = string.IsNullOrEmpty(defaultValue) ? DisplayStyle.None : DisplayStyle.Flex;
            target.labelElement.style.color = _textColor;
            target.label = name;

            fieldContainer.Add(target);

            return target;
        }

        private DropdownField CreateDropdownField(string name, PropertyInfo targetProperty, VisualElement fieldContainer, int index)
        {
            var value = (string[])targetProperty.GetValue(_tile);
            var isValue = (BrainPropertyValueName)targetProperty.GetCustomAttribute(typeof(BrainPropertyValueName), true);
            var type = isValue.Type;

            var values = GetVariableList(typeof(IMonaVariablesFloatValue));

            if (index == 0)
                values = GetVariableList(isValue.Type);

            values.Insert(0, "_");

            if (value.Length == 0)
                value = new string[4];

            var defaultVariable = values.Find(x => string.Compare(x, value[index], true) == 0);
            var target = new DropdownField(values, 0);

            if (defaultVariable != null)
            {
                target.value = defaultVariable;
                var currentValue = (string[])targetProperty.GetValue(_tile);
                currentValue[index] = target.value;
                targetProperty.SetValue(_tile, currentValue);
            }

            target.RegisterValueChangedCallback((evt) =>
            {
                if (evt.newValue == "_")
                    target.value = null;
                else
                    target.value = (string)evt.newValue;
                var currentValue = (string[])targetProperty.GetValue(_tile);
                currentValue[index] = target.value;
                targetProperty.SetValue(_tile, currentValue);
                Changed();
            });

            if (index == 0)
            {
                target.style.flexGrow = 1;
                target.label = name;
                target.style.flexDirection = FlexDirection.Column;
            }
            else
            {
                target.style.width = 60;
                var label = new Label();
                label.text = name;
                label.style.color = _textColor;
                fieldContainer.Add(label);
            }

            target.labelElement.style.color = _textColor;

            fieldContainer.Add(target);

            return target;
        }

        private List<string> GetVariableList(Type type)
        {
            return _brain.DefaultVariables.VariableList.FindAll(x => type.IsAssignableFrom(_brain.DefaultVariables.GetVariable(x.Name).GetType())).ConvertAll<string>(x => x.Name);
        }

        private Button CreateBindButton()
        {
            var btn = new Button();
            btn.style.width = 20;
            btn.style.height = 20;
            btn.style.color = Color.black;
            btn.text = "*";
            return btn;
        }

        private void CopyToTile(IInstructionTileDefinition def)
        {
            def.Tile.Id = def.Id;
            def.Tile.Name = def.Name;
            def.Tile.Category = def.Category;
        }

        private Color _brightPink = Color.HSVToRGB(351f / 360f, .79f, .98f);
        private Color _lightRed = Color.HSVToRGB(347f / 360f, .80f, .66f);
        private Color _textColor = Color.HSVToRGB(351f / 360f, .06f, .98f);

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
                _labelType.text = "ACTION" + (_tile.Muted ? " - [Muted]" : "");
                style.backgroundColor = _brightPink;
                if (tile is IActionStateEndInstructionTile && !_page.IsCore)
                    style.borderBottomRightRadius = style.borderTopRightRadius = 30;
                if (tile is IActionEndInstructionTile)
                    style.borderBottomRightRadius = style.borderTopRightRadius = 30;
            }
            else if (tile is IConditionInstructionTile)
            {
                if (tile is IInputInstructionTile)
                    _labelType.text = "INPUT" + (_tile.Muted ? " - [Muted]" : "");
                else
                    _labelType.text = "IF" + (_tile.Muted ? " - [Muted]" : "");
                style.backgroundColor = _lightRed;
                if (tile is IStartableInstructionTile && _index == 0)
                    style.borderBottomLeftRadius = style.borderTopLeftRadius = 30;
            }

        }

    }
}