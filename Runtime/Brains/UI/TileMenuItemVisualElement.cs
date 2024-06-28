using System;
using Mona.SDK.Brains.Core.Tiles;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mona.SDK.Brains.UIElements
{
    public class TileMenuItem
    {
        public string Label;
        public Action Action;
        public bool IsCategory;
        public string Category;
        public bool IsHeader;
        public bool IsCondition;
        public bool ShowByDefault;
        public IInstructionTile Tile;
        public override string ToString()
        {
            return Label;
        }

    }
    public class TileMenuItemVisualElement : VisualElement
    {
        private TileMenuItem _item;
        public TileMenuItem Item => _item;
        private Label _label;

        private Color _brightPink = Color.HSVToRGB(351f / 360f, .79f, .98f);
        private Color _lightRed = Color.HSVToRGB(347f / 360f, .80f, .66f);
        private Color _textColor = Color.white;

        private TileDragAndDropManipulator _manipulator;
        private VisualElement _root;
        private MonaBrainGraphVisualElement _search;

        public VisualElement Root => _root;
        public MonaBrainGraphVisualElement Search => _search;

        public TileMenuItemVisualElement(VisualElement root, MonaBrainGraphVisualElement search, bool draggable = true)
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1;
            _label = new Label();
            _label.style.flexWrap = Wrap.Wrap;
            Add(_label);

            _root = root;
            _search = search;

            if (draggable)
                _manipulator = new TileDragAndDropManipulator(this, root, search);
        }

        private void SetRadius(float r) => _label.style.borderBottomLeftRadius = _label.style.borderBottomRightRadius = _label.style.borderTopLeftRadius = _label.style.borderTopRightRadius = r;

        public void SetItem(TileMenuItem item)
        {
            _item = item;
            _label.style.flexGrow = 1;
            _label.style.unityFontStyleAndWeight = FontStyle.Normal;
            _label.style.marginBottom = _label.style.marginLeft = _label.style.marginRight = 2;
            _label.style.borderLeftWidth = 5;
            _label.style.borderLeftColor = Color.white;
            _label.style.paddingLeft = 4;
            _label.text = _item.Label;
            _label.style.unityTextAlign = TextAnchor.MiddleLeft;
            SetRadius(3);

            if (_item.IsCondition)
            {
                _label.style.backgroundColor = _lightRed;
                _label.style.color = _textColor;
            }
            else
            {
                _label.style.backgroundColor = _brightPink;
                _label.style.color = _textColor;
            }

            if (_item.IsCategory)
            {
                _label.style.borderLeftWidth = 0;
                _label.style.unityTextAlign = TextAnchor.MiddleLeft;
                _label.text = _item.Label.ToUpper();
                SetRadius(0);
                if (_item.IsHeader)
                {
                    Debug.Log($"is header: {_item.Label}");
                    _label.style.backgroundColor = Color.HSVToRGB(.9f, 0f, .1f);
                    _label.style.color = Color.HSVToRGB(1f, 0f, 1f);
                    _label.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    Debug.Log($"is category: {_item.Label}");
                    if (_item.IsCondition)
                    {
                        _label.style.backgroundColor = Color.HSVToRGB(.9f, 0f, .1f);
                        _label.style.color = _textColor;
                    }
                    else
                    {
                        _label.style.backgroundColor = Color.HSVToRGB(.9f, 0f, .1f);
                        _label.style.color = _textColor;
                    }

                    _label.style.color = Color.HSVToRGB(1f, 0f, 1f);
                    _label.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
            }
        }
    }
}