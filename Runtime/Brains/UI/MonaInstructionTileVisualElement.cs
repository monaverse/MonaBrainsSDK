using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Tiles;
using System;
using System.Reflection;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaInstructionTileVisualElement : VisualElement
    {
        public event Action<int> OnDelete;
        public event Action<int> OnLeft;
        public event Action<int> OnRight;
        public event Action OnHeight;

        private IMonaBrain _brain;
        private IInstructionTile _tile;
        private int _index;

        private VisualElement _toolBar;
        private VisualElement _valuesContainer;
        private VisualElement _values;
        private VisualElement _propertyValues;
        private ScrollView _valuesExtended;

        private Label _label;
        private ToolbarMenu _toolbarMenu;
        private bool _extended;

        private ListView _instructionListView;

        public MonaInstructionTileVisualElement()
        {
            style.flexDirection = FlexDirection.Column;

            _toolBar = new VisualElement();
            _toolBar.style.flexDirection = FlexDirection.Row;
            _toolBar.style.height = 20;
            _toolBar.style.paddingRight = 0;

            _label = new Label();
            _label.style.flexGrow = 1;
            _label.style.fontSize = 12;
            _label.style.minHeight = 12;
            _label.style.alignItems = Align.Center;
            _label.style.unityTextAlign = TextAnchor.MiddleCenter;
            _label.style.unityFontStyleAndWeight = FontStyle.Bold;
            _label.style.marginRight = 5;
            _toolBar.Add(_label);

            _toolbarMenu = new ToolbarMenu();
            _toolbarMenu.text = null;
            _toolbarMenu.style.width = 12;
            _toolbarMenu.style.paddingLeft = 1;
            _toolbarMenu.style.paddingRight = 1;
            _toolBar.Add(_toolbarMenu);

            Add(_toolBar);

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

        private void HandleLeft(int i)
        {
            OnLeft?.Invoke(i);
        }

        private void HandleRight(int i)
        {
            OnRight?.Invoke(i);
        }

        private void HandleDelete(int i)
        {
            OnDelete?.Invoke(i);
        }

        private void HandleMore()
        {            
            _extended = !_extended;

            _toolbarMenu.menu.RemoveItemAt(0);
            if(_extended)
                _toolbarMenu.menu.InsertAction(0, MonaBrainConstants.MENU_HIDE, (evt) => HandleMore());
            else
                _toolbarMenu.menu.InsertAction(0, MonaBrainConstants.MENU_SHOW, (evt) => HandleMore());
            _valuesExtended.style.display = _extended ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetInstructionTile(IMonaBrain brain, IInstructionTile tile, int tileIndex, int instructionTileCount)
        {
            _brain = brain;
            _tile = tile;
            _index = tileIndex;

            if (_tile == null) return;

            SetStyle(_tile);
            _label.text = _tile.Name;

            BuildValueEditor();
            BuildMenu(tileIndex, instructionTileCount);
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

                var isTag = (BrainPropertyMonaTag)property.GetCustomAttribute(typeof(BrainPropertyMonaTag), true);

                if (isTag != null)
                {
                    var field = new DropdownField(_brain.MonaTagSource.Tags, 0);
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) =>
                    {
                        field.value = (string)evt.newValue;
                        property.SetValue(_tile, field.value);
                    });
                    fieldContainer.Add(field);
                }
                else if (property.PropertyType == typeof(string))
                {
                    var field = new TextField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (string)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (string)evt.newValue));
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(float))
                {
                    var field = new FloatField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (float)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (float)evt.newValue));
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(int))
                {
                    var field = new IntegerField();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (int)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (int)evt.newValue));
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var field = new Toggle();
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (bool)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (bool)evt.newValue));
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(Vector2))
                {
                    var field = new Vector2Field();
                    field.style.width = 120;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (Vector2)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (Vector2)evt.newValue));
                    fieldContainer.Add(field);
                    
                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType == typeof(Vector3))
                {
                    var field = new Vector3Field();
                    field.style.width = 130;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (Vector3)property.GetValue(_tile);
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (Vector3)evt.newValue));
                    fieldContainer.Add(field);

                    AddTargetFieldIfExists(fieldContainer, field, properties, property);
                }
                else if (property.PropertyType.IsEnum)
                {
                    var type = property.PropertyType;
                    var field = new EnumField((Enum)type.GetEnumValues().GetValue(0));
                    field.style.width = 100;
                    field.style.flexDirection = FlexDirection.Column;
                    field.labelElement.style.color = Color.black;
                    field.label = property.Name;
                    field.value = (Enum)Enum.Parse(type, property.GetValue(_tile).ToString());
                    field.RegisterValueChangedCallback((evt) => property.SetValue(_tile, (Enum)evt.newValue));
                    container.Add(field);
                }
            }
        }

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
                var target = new TextField();
                var value = (string)targetProperty.GetValue(_tile);
                Debug.Log($"{nameof(AddTargetFieldIfExists)} {targetProperty.Name} {value}");
                target.style.width = 80;
                target.style.flexDirection = FlexDirection.Column;
                target.labelElement.style.color = Color.black;
                target.label = property.Name;
                target.value = value;
                target.style.display = !string.IsNullOrEmpty(value) ? DisplayStyle.Flex : DisplayStyle.None;
                target.RegisterValueChangedCallback((evt) =>
                {
                    targetProperty.SetValue(_tile, evt.newValue);
                });
                fieldContainer.Add(target);

                field.style.width = 80;
                field.style.display = !string.IsNullOrEmpty(value) ? DisplayStyle.None : DisplayStyle.Flex;

                var btn = new Button();
                btn.style.width = 20;
                btn.style.height = 20;
                btn.style.color = Color.black;
                btn.style.backgroundColor = !string.IsNullOrEmpty(value) ? Color.red : Color.gray;
                btn.style.color = !string.IsNullOrEmpty(value) ? Color.white : Color.black;
                btn.text = "*";
                btn.clicked += () =>
                {
                    var value2 = (string)targetProperty.GetValue(_tile);
                    Debug.Log($"Btn Clicked {targetProperty.Name} {value2}");
                    if (!string.IsNullOrEmpty(value2))
                    {
                        targetProperty.SetValue(_tile, null);
                        field.style.display = DisplayStyle.Flex;
                        target.style.display = DisplayStyle.None;
                        btn.style.backgroundColor = Color.gray;
                        btn.style.color = Color.black;
                    }
                    else
                    {
                        targetProperty.SetValue(_tile, property.Name);
                        target.value = property.Name;
                        field.style.display = DisplayStyle.None;
                        target.style.display = DisplayStyle.Flex;
                        btn.style.backgroundColor = Color.red;
                        btn.style.color = Color.white;
                    }
                };
                fieldContainer.Add(btn);
            }
        }

        private void BuildMenu(int tileIndex, int instructionTileCount)
        {
            _toolbarMenu.menu.ClearItems();

            if(_valuesExtended.childCount > 0)
                _toolbarMenu.menu.AppendAction(MonaBrainConstants.MENU_SHOW, (evt) => HandleMore());
            if(tileIndex > 0)
                _toolbarMenu.menu.AppendAction(MonaBrainConstants.MENU_MOVE_LEFT, (evt) => HandleLeft(tileIndex));
            if(tileIndex < instructionTileCount - 1)
                _toolbarMenu.menu.AppendAction(MonaBrainConstants.MENU_MOVE_RIGHT, (evt) => HandleRight(tileIndex));

            _toolbarMenu.menu.AppendSeparator();

            _toolbarMenu.menu.AppendAction(MonaBrainConstants.MENU_DELETE_TILE, (evt) => HandleDelete(tileIndex));
        }

        private void SetStyle(IInstructionTile tile)
        {
            style.color = Color.black;
            style.minWidth = 100;
            style.minHeight = 120;
            style.borderBottomLeftRadius = style.borderBottomRightRadius = style.borderTopLeftRadius = style.borderTopRightRadius = 5;
            style.borderBottomWidth = style.borderTopWidth = style.borderLeftWidth = style.borderRightWidth = 2;
            style.borderBottomColor = style.borderTopColor = style.borderLeftColor = style.borderRightColor = Color.black;
            style.paddingRight = style.paddingLeft = 5;
            style.paddingTop = style.paddingBottom = 5;

            if (tile is IActionInstructionTile)
            {
                style.backgroundColor = new Color(149f / 255f, 209f / 255f, 139f / 255f);
            }
            else if (tile is IConditionInstructionTile)
            {
                style.backgroundColor = new Color(108f / 255f, 173f / 255f, 201f / 255f);
                if (tile is IStartableInstructionTile && _index == 0)
                    style.borderBottomLeftRadius = style.borderTopLeftRadius = 30;
            }

        }

    }
}