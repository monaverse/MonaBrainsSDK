using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using UnityEngine.UI;
using TMPro;

namespace Mona.SDK.Brains.Tiles.Actions.UI
{
    [Serializable]
    public class UISetTextInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "UISetText";
        public const string NAME = "Set UI Text";
        public const string CATEGORY = "User Interface";
        public override Type TileType => typeof(UISetTextInstructionTile);

        [SerializeField] private string _newText;
        [SerializeField] private string _newTextString;
        [BrainProperty(true)] public string NewText { get => _newText; set => _newText = value; }
        [BrainPropertyValueName("NewText", typeof(IMonaVariablesValue))] public string NewTextString { get => _newTextString; set => _newTextString = value; }

        public UISetTextInstructionTile() { }

        private IMonaBrain _brain;
        private TMP_Text _tmp_text;
        private Text _unityUI_text;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || !HasTextElement())
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_newTextString))
                _newText = _brain.Variables.GetValueAsString(_newTextString);

            if (_tmp_text != null)
                _tmp_text.text = _newText;
            else
                _unityUI_text.text = _newText;

            return Complete(InstructionTileResult.Success);
        }

        private bool HasTextElement()
        {
            if (_tmp_text != null || _unityUI_text != null)
                return true;

            Transform tf = _brain.Body.Transform;

            if (!tf)
                return false;

            _tmp_text = tf.GetComponent<TMP_Text>();

            if (_tmp_text == null)
                _tmp_text = tf.GetComponentInChildren<TMP_Text>(true);

            if (_tmp_text != null)
                return true;

            _unityUI_text = tf.GetComponent<Text>();

            if (_unityUI_text == null)
                _unityUI_text = tf.GetComponentInChildren<Text>(true);

            if (_unityUI_text != null)
                return true;

            return false;
        }
    }
}