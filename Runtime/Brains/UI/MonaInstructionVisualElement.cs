using UnityEngine;
using UnityEngine.UIElements;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Tiles.ScriptableObjects;
using System;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif

namespace Mona.SDK.Brains.UIElements
{
    public class MonaInstructionVisualElement : VisualElement
    {
        public event Action<IInstruction, int> OnTileIndexClicked = delegate { };

        private IMonaBrain _brain;
        private IInstruction _instruction;
        private IMonaBrainPage _page;

        private ScrollView _scrollView;

#if UNITY_EDITOR
        private ToolbarMenu _replaceTileMenu;
        private VisualElement _toolBar;
        private ToolbarButton _btnDelete;
        private ToolbarButton _btnMoveLeft;
        private ToolbarButton _btnMoveRight;
#endif

        private int _scrollToIndex;
        private int _selectedTile;

        private IManipulator _click;

        public MonaInstructionVisualElement()
        {
            _click = new Clickable(() =>
            {
                HandleDeselect();
                OnTileIndexClicked(_instruction, -1);
            });
            
            style.flexDirection = FlexDirection.Row;
            style.minHeight = 120;
            _scrollView = new ScrollView();
            _scrollView.AddManipulator(_click);
            _scrollView.mode = ScrollViewMode.Horizontal;
            _scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
            _scrollView.style.backgroundColor = new StyleColor(new Color(.1f, .1f, .1f));
            _scrollView.style.height = Length.Percent(100);
            _scrollView.style.width = Length.Percent(100);
            Add(_scrollView);
#if UNITY_EDITOR
            _toolBar = new VisualElement();
            _toolBar.style.flexDirection = FlexDirection.Column;
            _toolBar.style.paddingLeft = _toolBar.style.paddingLeft = 3;
            Add(_toolBar);
            _btnDelete = new ToolbarButton();
            _btnDelete.style.flexGrow = 1;
            _btnDelete.style.borderBottomWidth = 1;
            _btnDelete.style.marginBottom = 1;
            _btnDelete.text = "delete";
            _btnDelete.style.color = Color.black;
            _btnDelete.style.backgroundColor = Color.white;
            StyleButton(_btnDelete);
            _btnDelete.clicked += () =>
            {
                if (_selectedTile > -1)
                    _instruction.DeleteTile(_selectedTile);
            };
            _toolBar.Add(_btnDelete);

            _btnMoveLeft = new ToolbarButton();
            _btnMoveLeft.style.flexGrow = 1;
            _btnMoveLeft.style.borderBottomWidth = 1;
            _btnMoveLeft.style.marginBottom = 1;
            _btnMoveLeft.text = "left";
            _btnMoveLeft.style.color = Color.black;
            _btnMoveLeft.style.backgroundColor = Color.white;
            StyleButton(_btnMoveLeft);
            _btnMoveLeft.clicked += () =>
            {
                if (_selectedTile > -1)
                {
                    _instruction.MoveTileLeft(_selectedTile);
                    Select(_selectedTile - 1, false);
                }
            };
            _toolBar.Add(_btnMoveLeft);

            _btnMoveRight = new ToolbarButton();
            _btnMoveRight.style.flexGrow = 1;
            _btnMoveRight.style.borderBottomWidth = 1;
            _btnMoveRight.style.marginBottom = 1;
            _btnMoveRight.text = "right";
            _btnMoveRight.style.color = Color.black;
            _btnMoveRight.style.backgroundColor = Color.white;
            StyleButton(_btnMoveRight);
            _btnMoveRight.clicked += () =>
            {
                if (_selectedTile > -1)
                {
                    _instruction.MoveTileRight(_selectedTile);
                    Select(_selectedTile + 1, false);
                }
            };
            _toolBar.Add(_btnMoveRight);

            _replaceTileMenu = new ToolbarMenu();
            _replaceTileMenu.style.flexGrow = 1;
            _replaceTileMenu.text = "replace";
            _replaceTileMenu.style.color = Color.black;
            _replaceTileMenu.style.backgroundColor = Color.white;
            _toolBar.Add(_replaceTileMenu);
#endif
        }

#if UNITY_EDITOR
        private void StyleButton(ToolbarButton btn)
        {
            btn.style.unityTextAlign = TextAnchor.MiddleCenter;
            btn.style.unityFontStyleAndWeight = FontStyle.Bold;
            btn.style.fontSize = 12;
        }
#endif

        public void AddTile(IInstructionTile tile, int i, IMonaBrainPage page)
        {
            _instruction.AddTile(tile, i, page);
        }
        
        public void ClearBorders()
        {
            for (var i = 0; i < _instruction.InstructionTiles.Count; i++)
                GetTileVisualElement(i).Deselect();
        }

        private void RefreshInstructionTiles(int scrollIndex)
        {
            _scrollToIndex = scrollIndex;
            for (var i = _instruction.InstructionTiles.Count - 1; i >= 0; i--)
            {
                if (_instruction.InstructionTiles[i] == null)
                    _instruction.InstructionTiles.RemoveAt(i);
            }

            _scrollView.Clear();
            for (var i = 0; i <_instruction.InstructionTiles.Count; i++)
            {
                var tile = _instruction.InstructionTiles[i];
                var view = new MonaInstructionTileVisualElement();
                view.OnClicked += (c, expand) =>
                {
                    Select(c, expand);
                };
                view.SetInstructionTile(_brain, _page, tile, i, _instruction.InstructionTiles.Count);
                _scrollView.Add(view);
                _scrollView.schedule.Execute(() =>
                {
                    if (_scrollView.contentContainer.childCount > _scrollToIndex && _scrollView.contentContainer.childCount >= _scrollToIndex)
                        _scrollView.ScrollTo(_scrollView.contentContainer.ElementAt(_scrollToIndex));
                }).ExecuteLater(100);
            }
        }

        private void Select(int c, bool expand)
        {
            ClearBorders();
            OnTileIndexClicked(_instruction, c);
            ShowMenu(c);
            GetTileVisualElement(c).Select(expand);
            Debug.Log($"Select");
            _scrollView.schedule.Execute(() =>
            {
                if (_scrollView.contentContainer.childCount > c)
                    _scrollView.ScrollTo(_scrollView.contentContainer.ElementAt(c));
            }).ExecuteLater(100);
        }

        private void ShowMenu(int i)
        {
            _selectedTile = i;
            RefreshMenu();
#if UNITY_EDITOR
            if (_toolBar.parent == null)
                Add(_toolBar);

            if (_instruction.HasEndTile(_page))
                _btnMoveRight.SetEnabled(i != _instruction.InstructionTiles.Count - 2);
            else
                _btnMoveRight.SetEnabled(i != _instruction.InstructionTiles.Count - 1);

            if (_instruction.HasEndTile(_page) && i == _instruction.InstructionTiles.Count - 1)
            {
                _btnMoveLeft.SetEnabled(false);
                _btnMoveRight.SetEnabled(false);
                return;
            }
            else
                _btnMoveLeft.SetEnabled(i != 0);

            if (_instruction.InstructionTiles[_selectedTile] is IActionInstructionTile && _selectedTile > 0 && _instruction.InstructionTiles[_selectedTile-1] is IConditionInstructionTile)
            {
                _btnMoveLeft.SetEnabled(false);
            }
            else if (_instruction.InstructionTiles[_selectedTile] is IConditionInstructionTile && _selectedTile < _instruction.InstructionTiles.Count-1 && _instruction.InstructionTiles[_selectedTile + 1] is IActionInstructionTile)
            {
                _btnMoveRight.SetEnabled(false);
            }


#endif

        }

        private void HideMenu()
        {
            _selectedTile = -1;
#if UNITY_EDITOR
            if (_toolBar.parent != null)
                Remove(_toolBar);
#endif
        }

        public void SetInstruction(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _brain.OnMigrate -= HandleMigrate;
            _brain.OnMigrate += HandleMigrate;

            _page = page;

            _instruction = instruction;
            _instruction.OnRefresh -= RefreshInstructionTiles;
            _instruction.OnRefresh += RefreshInstructionTiles;

            _instruction.OnDeselect -= HandleDeselect;
            _instruction.OnDeselect += HandleDeselect;

            RefreshInstructionTiles(0);
            RefreshMenu();
            HideMenu();
            AddKeyListeners();
        }

        private bool _listeners;
        private void AddKeyListeners()
        {
            if (_listeners) return;
            _listeners = true;
#if UNITY_EDITOR
            this.panel.visualTree.RegisterCallback<KeyDownEvent>((evt) =>
            {
                if (evt.keyCode == KeyCode.Delete)
                {
                    if (_selectedTile > -1)
                    {
                        _instruction.DeleteTile(_selectedTile);
                        if(_selectedTile <= _instruction.InstructionTiles.Count-1)
                            Select(_selectedTile, false);
                    }
                }
                else if(evt.keyCode == KeyCode.RightArrow)
                {
                    if (_selectedTile > -1 && _btnMoveRight.enabledSelf)
                    {
                        _instruction.MoveTileRight(_selectedTile);
                        Select(_selectedTile + 1, false);
                    }
                }
                else if (evt.keyCode == KeyCode.LeftArrow)
                {
                    if (_selectedTile > -1 && _btnMoveLeft.enabledSelf)
                    {
                        _instruction.MoveTileLeft(_selectedTile);
                        Select(_selectedTile - 1, false);
                    }
                }
            }, TrickleDown.TrickleDown);
#endif
        }

        public void ClearInstruction()
        {
            //Debug.Log($"{nameof(MonaInstructionVisualElement)} ClearInstruction");
            _instruction.OnRefresh -= RefreshInstructionTiles;
            _brain.OnMigrate -= HandleMigrate;
            _instruction.OnDeselect -= HandleDeselect;
        }

        private void HandleDeselect()
        {
            Debug.Log($"Deselect");
            HideMenu();
            for (var i = 0; i < _instruction.InstructionTiles.Count; i++)
                GetTileVisualElement(i).Deselect();
        }

        private MonaInstructionTileVisualElement GetTileVisualElement(int i)
        {
            if (i <= 0) i = 0;
            if (i > _scrollView.contentContainer.childCount - 1) i = _scrollView.contentContainer.childCount - 1;
            return ((MonaInstructionTileVisualElement)_scrollView.contentContainer.ElementAt(i));
        }

        private void HandleMigrate()
        {
            MigrateTiles();
            //RefreshMenu();
        }

        public void MigrateTiles()
        {
            Debug.Log($"{nameof(MigrateTiles)}");
            for(var i = 0;i < _instruction.InstructionTiles.Count; i++)
            {
                var tile = _instruction.InstructionTiles[i];
                var newTile = _brain.TileSet.Find(tile.Id);
                if (newTile != null)
                {
                    _instruction.ReplaceTile(i, newTile.Tile);
                    //Debug.Log($"replaced {i} {newTile.Id}");
                }
            }
        }

        private bool AllowTile(IInstructionTile tile)
        {
            if (_selectedTile < 0) return false;
            if (_selectedTile > _instruction.InstructionTiles.Count-1) return false;
            if (_instruction.InstructionTiles[_selectedTile] is IActionInstructionTile)
                return tile is IActionInstructionTile;
            else
                return tile is IConditionInstructionTile;
        }

        private void RefreshMenu() 
        {
            if(_brain.TileSet == null)
            {
                Debug.LogError($"Please assign a tile set to this brain");
                return;
            }
#if UNITY_EDITOR
            _replaceTileMenu.menu.ClearItems();
            _replaceTileMenu.menu.AppendAction("REPLACE TILE", null, DropdownMenuAction.Status.Disabled);
            for (var i = 0; i < _brain.TileSet.ConditionTiles.Count; i++)
            {
                var def = _brain.TileSet.ConditionTiles[i];
                if (def == null) continue;
                CopyToTile(def);
                if(AllowTile(def.Tile))
                    _replaceTileMenu.menu.AppendAction($"{def.Category}/{def.Name}", (action) => _instruction.ReplaceTile(_selectedTile, def.Tile));
            }
            for (var i = 0; i < _brain.TileSet.ActionTiles.Count; i++)
            {
                var def = _brain.TileSet.ActionTiles[i];
                if (def == null) continue;
                CopyToTile(def);
                if (AllowTile(def.Tile))
                    _replaceTileMenu.menu.AppendAction($"{def.Category}/{def.Name}", (action) => _instruction.ReplaceTile(_selectedTile, def.Tile));
            }
#endif
        }

        private void CopyToTile(IInstructionTileDefinition def)
        {
            def.Tile.Id = def.Id;
            def.Tile.Name = def.Name;
            def.Tile.Category = def.Category;
        }
    }
}