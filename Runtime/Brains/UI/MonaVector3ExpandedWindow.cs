using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Mona.SDK.Core.State.Structs;
using UnityEngine.UIElements;
using Mona.SDK.Core.EasyUI;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Brain;

namespace Mona.SDK.Brain.UIElements
{
#if UNITY_EDITOR
    public class MonaVector3ExpandedWindow : EditorWindow
    {
        private IMonaBrain _brain;
        private IInstructionTile _tile;

        private DropdownField _targetField;

        protected Action callback;

        public static void Open(IMonaBrain brain, IInstructionTile tile, Action newCallback)
        {
            var window = GetWindow<MonaVector3ExpandedWindow>("Vector3 Editor");
            window.SetupTile(brain, tile);
            window.callback = newCallback;
        }

        public void SetupTile(IMonaBrain brain, IInstructionTile tile)
        {
            _brain = brain;
            _tile = tile;
            Refresh();
        }

        private void Refresh()
        {
            
        }

        private void CreateGUI()
        {

        }
    }
#endif
}

