using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using UnityEngine.UI;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class HideInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "Hide";
        public const string NAME = "Hide";
        public const string CATEGORY = "Visuals";
        public override Type TileType => typeof(HideInstructionTile);

        private IMonaBrain _brain;
        private TMPro.TextMeshProUGUI[] _uiTexts;
        private TMPro.TextMeshPro[] _texts;
        private Text[] _basicTexts;

        public HideInstructionTile() { }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _uiTexts = _brain.Body.Transform.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            _texts = _brain.Body.Transform.GetComponentsInChildren<TMPro.TextMeshPro>();
            _basicTexts = _brain.Body.Transform.GetComponentsInChildren<Text>();
        }

        public override InstructionTileResult Do()
        {
            _brain.Body.SetVisible(false);

            if(_uiTexts.Length > 0)
            {
                for(var i = 0;i < _uiTexts.Length; i++)
                {
                    _uiTexts[i].enabled = false;
                }
            }

            if (_texts.Length > 0)
            {
                for (var i = 0; i < _texts.Length; i++)
                {
                    _texts[i].enabled = false;
                }
            }

            if (_basicTexts.Length > 0)
            {
                for (var i = 0; i < _basicTexts.Length; i++)
                {
                    _basicTexts[i].enabled = false;
                }
            }
            //Debug.Log($"{nameof(ChangeStateInstructionTile)} state: {_changeState}");
            return Complete(InstructionTileResult.Success);
        }
    }
}