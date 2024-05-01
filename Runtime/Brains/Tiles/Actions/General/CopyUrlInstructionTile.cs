using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Control;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;
using Mona.SDK.Core.State.Structs;
using System.Collections.Generic;

namespace Mona.SDK.Brains.Tiles.Actions.General
{
    [Serializable]
    public class CopyUrlInstructionTile : InstructionTile, IInstructionTileWithPreloadAndPageAndInstruction, IActionInstructionTile
    {
        public const string ID = "CopyUrl";
        public const string NAME = "Copy Url";
        public const string CATEGORY = "General";
        public override Type TileType => typeof(CopyUrlInstructionTile);

        [SerializeField] private MonaBrainCopyUrlType _source = MonaBrainCopyUrlType.UrlVariable;
        [BrainProperty(true)] public MonaBrainCopyUrlType Source { get => _source; set => _source = value; }

        [SerializeField] string _urlVariableName;
        [BrainPropertyShow(nameof(Source), (int)MonaBrainCopyUrlType.UrlVariable)]
        [BrainProperty(true)] public string UrlVariable { get => _urlVariableName; set => _urlVariableName = value; }

        [SerializeField] string _targetValue;
        [BrainPropertyValue(typeof(IMonaVariablesStringValue))] public string TargetValue { get => _targetValue; set => _targetValue = value; }

        private IMonaBrain _brain;

        public CopyUrlInstructionTile() { }

        public void Preload(IMonaBrain brain, IMonaBrainPage page, IInstruction instruction)
        {
            _brain = brain;
            _instruction = instruction;
        }

        public override InstructionTileResult Do()
        {
            switch(_source)
            {
                case MonaBrainCopyUrlType.UrlVariable:
                    //var url = "http://monaverse.com/test?vrm_url=" + System.Uri.EscapeUriString("https://storage.cryptoavatars.io/VRMsByContractAddress/0xc1def47cf1e15ee8c2a92f4e0e968372880d18d1/100Avatars_082_GoodTomato.vrm");
                    var url = Application.absoluteURL;

                    var urlParams = ParseUrl(url);
                    if(urlParams.ContainsKey(_urlVariableName))
                        _brain.Variables.Set(_targetValue, urlParams[_urlVariableName]); break;
                default: break;
            }
            return Complete(InstructionTileResult.Success);
        }

        private Dictionary<string, string> ParseUrl(string url)
        {
            var query = url.Split('?');
            string[] parameters = new string[0];
            if (query.Length > 1)
                parameters = query[1].Split('&');

            var urlParams = new Dictionary<string, string>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var rawParam = parameters[i].Split('=');
                if (!urlParams.ContainsKey(rawParam[0]))
                    urlParams.Add(rawParam[0], System.Uri.UnescapeDataString(rawParam[1]));
            }
            return urlParams;
        }

    }
}