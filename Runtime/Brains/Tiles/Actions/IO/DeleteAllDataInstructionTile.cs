using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Utils.Interfaces;
using Mona.SDK.Brains.Core.Utils.Enums;

namespace Mona.SDK.Brains.Tiles.Actions.IO
{
    [Serializable]
    public class DeleteAllDataInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload
    {
        public const string ID = "DeleteAllData";
        public const string NAME = "Delete All Data";
        public const string CATEGORY = "File Storage";
        public override Type TileType => typeof(DeleteAllDataInstructionTile);

        [SerializeField] private StorageTargetType _storageTarget = StorageTargetType.LocalAndCloud;
        [BrainPropertyEnum(true)] public StorageTargetType StorageTarget { get => _storageTarget; set => _storageTarget = value; }

        public DeleteAllDataInstructionTile() { }

        private IMonaBrain _brain;
        private MonaGlobalBrainRunner _globalBrainRunner;
        private IBrainStorage _localStorage;
        private IBrainStorage _cloudStorage;

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _globalBrainRunner = MonaGlobalBrainRunner.Instance;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _globalBrainRunner == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (_storageTarget != StorageTargetType.Cloud && _localStorage == null)
            {
                _localStorage = _globalBrainRunner.LocalStorage;

                if (_localStorage == null)
                    return Complete(InstructionTileResult.Success);
            }

            if (_storageTarget != StorageTargetType.Local && _cloudStorage == null)
            {
                _cloudStorage = _globalBrainRunner.ClousStorage;

                if (_cloudStorage == null)
                    return Complete(InstructionTileResult.Success);
            }

            if (_storageTarget != StorageTargetType.Cloud) _localStorage.DeleteAllData(out bool _);
            if (_storageTarget != StorageTargetType.Local) _cloudStorage.DeleteAllData(out bool _);
            return Complete(InstructionTileResult.Success);
        }
    }
}