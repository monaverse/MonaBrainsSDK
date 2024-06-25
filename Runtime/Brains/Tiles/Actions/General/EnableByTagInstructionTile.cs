using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Core.Body;
using Mona.SDK.Brains.ThirdParty.Redcode.Awaiting;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class EnableByTagInstructionTile : InstructionTile, IChangeTagInstructionTile, IActionInstructionTile
    {
        public const string ID = "EnableByTag";
        public const string NAME = "Enable By Tag";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(EnableByTagInstructionTile);

        [SerializeField]
        private string _tag;
        [BrainPropertyMonaTag]
        public string Tag { get => _tag; set => _tag = value; }

        public EnableByTagInstructionTile() { }

        public override void SetThenCallback(IInstructionTile tile, Func<InstructionTileCallback, InstructionTileResult> thenCallback)
        {
            if (_thenCallback.ActionCallback == null)
            {
                _instructionCallback.Tile = tile;
                _instructionCallback.ActionCallback = thenCallback;
                _thenCallback.Tile = this;
                _thenCallback.ActionCallback = ExecuteActionCallback;
            }
        }

        private InstructionTileCallback _instructionCallback = new InstructionTileCallback();
        private InstructionTileResult ExecuteActionCallback(InstructionTileCallback callback)
        {
            if (_instructionCallback.ActionCallback != null) return _instructionCallback.ActionCallback.Invoke(_thenCallback);
            return InstructionTileResult.Success;
        }

        public override InstructionTileResult Do()
        {
            var bodies = MonaBody.FindByTag(_tag);
            for (var i = 0; i < bodies.Count; i++)
            {
                bodies[i].SetActive(true);
                //Debug.Log($"{nameof(EnableByTagInstructionTile)} body: {bodies[i].Transform.name}");
            }

            WaitForEnable(bodies);
            return Complete(InstructionTileResult.Running);
        }

        private bool HasNotStarted(List<IMonaBody> bodies)
        {
            var hasNotStarted = false;
            for (var i = 0; i < bodies.Count; i++)
            {
                if (bodies[i].GetActive() && !bodies[i].Started)
                    hasNotStarted = true;
            }
            return hasNotStarted;
        }

        private async void WaitForEnable(List<IMonaBody> bodies)
        {
            var ready = false;
            var t = 0f;
            while (!ready)
            {
                await new WaitForSeconds(.1f);

                if(t > 1f)
                {
                    t += .1f;
                    Debug.Log($"{nameof(EnableByTagInstructionTile)} wait timed out");
                    break;
                }

                if (!HasNotStarted(bodies))
                    break;
            }
            await new WaitForSeconds(.1f);
            Complete(InstructionTileResult.Success, true);
        }


    }
}