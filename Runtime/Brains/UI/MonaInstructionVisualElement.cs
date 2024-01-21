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
        private IMonaBrain _brain;
        private IInstruction _instruction;

        private ScrollView _scrollView;

#if UNITY_EDITOR
        private ToolbarMenu _addTileMenu;
#endif

        public MonaInstructionVisualElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.minHeight = 120;
            _scrollView = new ScrollView();
            _scrollView.mode = ScrollViewMode.Horizontal;
            _scrollView.contentContainer.style.flexDirection = FlexDirection.Row;
            _scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            _scrollView.horizontalScrollerVisibility = ScrollerVisibility.Auto;
            _scrollView.style.backgroundColor = new StyleColor(new Color(.1f, .1f, .1f));
            _scrollView.style.height = Length.Percent(100);
            _scrollView.style.width = Length.Percent(100);
            Add(_scrollView);
#if UNITY_EDITOR
            _addTileMenu = new ToolbarMenu();
            _addTileMenu.text = "+ ";
            Add(_addTileMenu);
#endif
        }

        private void AddTile(IInstructionTile tile)
        {
            _instruction.AddTile(tile);
            RefreshInstructionTiles();
            RefreshMenu();
        }

        private void RefreshInstructionTiles()
        {
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
                view.OnDelete += (i) =>
                {
                    _instruction.DeleteTile(i);
                    RefreshInstructionTiles();
                };
                view.OnRight += (i) =>
                {
                    if (_instruction.InstructionTiles[i] is IConditionInstructionTile && _instruction.InstructionTiles[i - 1] is IActionInstructionTile)
                        return;
                    _instruction.MoveTileRight(i);
                    RefreshInstructionTiles();
                };
                view.OnLeft += (i) =>
                {
                    if (_instruction.InstructionTiles[i] is IActionInstructionTile && _instruction.InstructionTiles[i - 1] is IConditionInstructionTile)
                        return;
                    _instruction.MoveTileLeft(i);
                    RefreshInstructionTiles();
                };
                view.OnReplace += (i, tile) =>
                {
                    _instruction.ReplaceTile(i, tile);
                    RefreshInstructionTiles();
                };
                view.SetInstructionTile(_brain, tile, i, _instruction.InstructionTiles.Count);
                _scrollView.Add(view);
            }
        }

        public void SetInstruction(IMonaBrain brain, IInstruction instruction)
        {
            _brain = brain;
            _brain.OnMigrate -= HandleMigrate;
            _brain.OnMigrate += HandleMigrate;

            _instruction = instruction;
            RefreshInstructionTiles();
            RefreshMenu();
        }

        public void ClearInstruction()
        {
            //Debug.Log($"{nameof(MonaInstructionVisualElement)} ClearInstruction");
            _brain.OnMigrate -= HandleMigrate;
        }

        private void HandleMigrate()
        {
            MigrateTiles();
            RefreshMenu();
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
            if (_instruction.InstructionTiles.Count == 0)
                return tile is IStartableInstructionTile;
            else
                return true;
        }

        private void RefreshMenu() 
        {
            if(_brain.TileSet == null)
            {
                Debug.LogError($"Please assign a tile set to this brain");
                return;
            }
#if UNITY_EDITOR
            _addTileMenu.menu.ClearItems();
            for (var i = 0; i < _brain.TileSet.ConditionTiles.Count; i++)
            {
                var def = _brain.TileSet.ConditionTiles[i];
                CopyToTile(def);
                if(AllowTile(def.Tile))
                    _addTileMenu.menu.AppendAction($"{def.Category}/{def.Name}", (action) => AddTile(def.Tile));
            }
            for (var i = 0; i < _brain.TileSet.ActionTiles.Count; i++)
            {
                var def = _brain.TileSet.ActionTiles[i];
                CopyToTile(def);
                if (AllowTile(def.Tile))
                    _addTileMenu.menu.AppendAction($"{def.Category}/{def.Name}", (action) => AddTile(def.Tile));
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