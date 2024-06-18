using UnityEngine;
using System;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Tiles.Conditions.Interfaces;
using Mona.SDK.Core.State.Structs;
using UnityEngine.XR;

namespace Mona.SDK.Brains.Tiles.Conditions
{
    [Serializable]
    public class IsDeviceInstructionTile : InstructionTile, IInstructionTileWithPreload, IConditionInstructionTile, IStartableInstructionTile, IOnStartInstructionTile, ITickAfterInstructionTile
    {
        public const string ID = "IsDevice";
        public const string NAME = "Is Device";
        public const string CATEGORY = "Platform";
        public override Type TileType => typeof(IsDeviceInstructionTile);

        [SerializeField] private PlatformType _device = PlatformType.SpecificOS;
        [BrainPropertyEnum(true)] public PlatformType Device { get => _device; set => _device = value; }

        [SerializeField] private OperatingSystemType _operatingSystem = OperatingSystemType.WebGL;
        [BrainPropertyShow(nameof(Device), (int)PlatformType.SpecificOS)]
        [BrainPropertyEnum(true)] public OperatingSystemType OperatingSystem { get => _operatingSystem; set => _operatingSystem = value; }

        [SerializeField] private bool _allowInEditor = true;
        [SerializeField] private string _allowInEditorName;
        [BrainPropertyShow(nameof(Device), (int)PlatformType.Console)]
        [BrainPropertyShow(nameof(Device), (int)PlatformType.Desktop)]
        [BrainPropertyShow(nameof(Device), (int)PlatformType.Handheld)]
        [BrainPropertyShow(nameof(Device), (int)PlatformType.Headset)]
        [BrainPropertyShow(nameof(Device), (int)PlatformType.SpecificOS)]
        [BrainProperty(true)] public bool AllowInEditor { get => _allowInEditor; set => _allowInEditor = value; }
        [BrainPropertyValueName("AllowInEditor", typeof(IMonaVariablesBoolValue))] public string AllowInEditorName { get => _allowInEditorName; set => _allowInEditorName = value; }

        private IMonaBrain _brain;
        private DeviceType _runtimeDevice;
        private RuntimePlatform _runtimePlatform;
        private bool _tileIsTrue;

        public enum PlatformType
        {
            Console = 0,
            Desktop = 10,
            GameEditor = 20,
            Handheld = 30,
            Headset = 40,
            SpecificOS = 50
        }

        public enum OperatingSystemType
        {
            Any = 0,
            Android = 10,
            Linux = 20,
            MacOS = 30,
            iOS = 40,
            iPadOS = 50,
            Playstation = 60,
            MetaQuest = 70,
            NintendoSwitch = 80,
            tvOS = 90,
            VisionOS = 100,
            WebGL = 120,
            Windows = 130,
            Xbox = 140
        }

        public IsDeviceInstructionTile() { }

        public void Preload(IMonaBrain brainInstance)
        {
            _brain = brainInstance;
            _runtimeDevice = SystemInfo.deviceType;
            _runtimePlatform = Application.platform;

            _tileIsTrue = IsSpecifiedDevice();
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_allowInEditorName))
                _allowInEditor = _brain.Variables.GetBool(_allowInEditorName);

            #if UNITY_EDITOR
            if (_allowInEditor) return Complete(InstructionTileResult.Success);
            #endif

            return _tileIsTrue ?
                Complete(InstructionTileResult.Success) :
                Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);
        }

        private bool IsSpecifiedDevice()
        {

            #if UNITY_EDITOR
            if (_device == PlatformType.GameEditor) return true;
            #endif

            switch (_device)
            {
                case PlatformType.Console:
                    return _runtimeDevice == DeviceType.Console;
                case PlatformType.Desktop:
                    return _runtimeDevice == DeviceType.Desktop;
#if UNITY_2023_2_OR_NEWER
                case PlatformType.Headset:
                    return XRSettings.isDeviceActive;
#else
                case PlatformType.Headset:
                    return _runtimePlatform == RuntimePlatform.VisionOS || XRSettings.isDeviceActive;
#endif
                case PlatformType.Handheld:
                    return _runtimeDevice == DeviceType.Handheld;
                case PlatformType.SpecificOS:
                    return IsSpecifiedOS();
            }

            return false;
        }

        private bool IsSpecifiedOS()
        {
            switch (_operatingSystem)
            {
                case OperatingSystemType.Any:
                    return true;
                case OperatingSystemType.Android:
                    return _runtimePlatform == RuntimePlatform.Android;
                case OperatingSystemType.Linux:
                    return _runtimePlatform == RuntimePlatform.LinuxPlayer || _runtimePlatform == RuntimePlatform.LinuxServer;
                case OperatingSystemType.MacOS:
                    return _runtimePlatform == RuntimePlatform.OSXPlayer || _runtimePlatform == RuntimePlatform.OSXServer;
                case OperatingSystemType.iOS:
                case OperatingSystemType.iPadOS:
                    return _runtimePlatform == RuntimePlatform.IPhonePlayer;
                case OperatingSystemType.Playstation:
                    return _runtimePlatform == RuntimePlatform.PS5 || _runtimePlatform == RuntimePlatform.PS4;
                case OperatingSystemType.MetaQuest:
                    return _runtimePlatform == RuntimePlatform.Android && XRSettings.isDeviceActive;
                case OperatingSystemType.NintendoSwitch:
                    return _runtimePlatform == RuntimePlatform.Switch;
                case OperatingSystemType.tvOS:
                    return _runtimePlatform == RuntimePlatform.tvOS;
#if UNITY_2023_2_OR_NEWER
                case OperatingSystemType.VisionOS:
                    return XRSettings.isDeviceActive;
#else
                case OperatingSystemType.VisionOS:
                    return _runtimePlatform == RuntimePlatform.VisionOS;
#endif
                case OperatingSystemType.WebGL:
                    return _runtimePlatform == RuntimePlatform.WebGLPlayer;
                case OperatingSystemType.Windows:
                    return _runtimePlatform == RuntimePlatform.WindowsPlayer || _runtimePlatform == RuntimePlatform.WindowsPlayer || _runtimePlatform == RuntimePlatform.WindowsServer;
                case OperatingSystemType.Xbox:
                    return _runtimePlatform == RuntimePlatform.XboxOne || _runtimePlatform == RuntimePlatform.GameCoreXboxOne || _runtimePlatform == RuntimePlatform.GameCoreXboxSeries;
                
            }

            return false;
        }
    }
}