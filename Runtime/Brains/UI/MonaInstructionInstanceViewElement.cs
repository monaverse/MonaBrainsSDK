using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaInstructionInstanceViewElement : VisualElement
    {
        private IInstruction _instruction;
        private bool _markForClear;

        public MonaInstructionInstanceViewElement(IInstruction instruction, int idx)
        {
            var elems = new List<MonaInstructionTileInstanceViewElement>();

            _instruction = instruction;
            _instruction.OnReset += (instruction) =>
            {
               for (var i = 1; i < elems.Count; i++)
                   elems[i].Reset();
            };

            style.flexDirection = FlexDirection.Row;
            style.flexWrap = Wrap.Wrap;
            SetPadding(5);
            SetBackgroundColor(new Color(.1f, .1f, .1f));
            SetBorder(Color.black);

            Add(new Label($"{idx}:"));
            for (var i = 0; i < _instruction.InstructionTiles.Count; i++)
            {
                var elemIdx = i;
                var tile = _instruction.InstructionTiles[i];                                
                tile.OnExecute += (result, reason, tile) =>
                {
                    if (result == InstructionTileResult.Failure)
                    {
                        for (var j = elemIdx + 1; j < elems.Count; j++)
                            elems[j].Reset();
                    }
                };
                var elem = new MonaInstructionTileInstanceViewElement(tile);
                elems.Add(elem);
                Add(elem);

                if (i < _instruction.InstructionTiles.Count - 1)
                    Add(new Label(">"));
            }
        }

        private void SetPadding(float padding)
        {
            style.paddingLeft = style.paddingTop = style.paddingRight = style.paddingBottom = padding;
        }

        private void SetBackgroundColor(Color color)
        {
            style.backgroundColor = color;
        }

        private void SetBorder(Color color)
        {
            style.borderLeftColor = style.borderTopColor = style.borderRightColor = style.borderBottomColor = color;
            style.borderLeftWidth = style.borderTopWidth = style.borderRightWidth = style.borderBottomWidth = 1;
        }
    }
}