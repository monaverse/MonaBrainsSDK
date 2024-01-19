using UnityEngine.UIElements;
using UnityEngine;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;

namespace Mona.SDK.Brains.UIElements
{
    public class MonaInstructionTileInstanceViewElement : VisualElement
    {
        private IInstructionTile _tile;
        public MonaInstructionTileInstanceViewElement(IInstructionTile tile)
        {
            //Debug.Log($"{nameof(MonaInstructionTileInstanceViewElement)} {tile}");
            _tile = tile;
            _tile.OnExecute += HandleExecute;
            SetBorderRadius(3);
            SetBackground(Color.grey);
            Add(new Label(_tile.Name));
        }

        private void HandleExecute(InstructionTileResult result, string reason, IInstructionTile tile)
        {
            //Debug.Log($"{nameof(HandleExecute)} {result} {tile}");
            if (result == InstructionTileResult.Success)
            {
                SetBackground(Color.green);
                style.color = Color.black;
                tooltip = "Success";
            }
            else if (result == InstructionTileResult.Running)
            {
                SetBackground(Color.yellow);
                style.color = Color.black;
                tooltip = "Running";
            }
            else if (result == InstructionTileResult.Failure)
            {
                SetBackground(Color.red);
                style.color = Color.white;
                tooltip = reason;
            }
        }

        public void Reset()
        {
            SetBackground(Color.grey);
            style.color = Color.black;
            tooltip = "Not Executed";
        }

        private void SetBorderRadius(float radius)
        {
            style.borderBottomLeftRadius = style.borderBottomRightRadius = style.borderTopLeftRadius = style.borderTopRightRadius = radius;
        }

        private void SetBackground(Color color)
        {
            style.backgroundColor = color;
            style.borderLeftColor = style.borderTopColor = style.borderRightColor = style.borderBottomColor = Color.black;
            style.borderLeftWidth = style.borderTopWidth = style.borderRightWidth = style.borderBottomWidth = 1;
        }
    }
}