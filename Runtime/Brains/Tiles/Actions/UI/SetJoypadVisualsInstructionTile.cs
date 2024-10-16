using Mona.SDK.Brains.Core.Enums;
using Mona.SDK.Brains.Core.Tiles;
using Mona.SDK.Brains.Core;
using UnityEngine;
using System;
using Mona.SDK.Brains.Core.State;
using Mona.SDK.Brains.Tiles.Actions.General.Interfaces;
using Mona.SDK.Brains.Core.Brain;
using Mona.SDK.Core.State.Structs;
using Mona.SDK.Brains.EasyUI.ScreenInput;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;
using Mona.SDK.Brains.EasyUI.ScreenInput.Structs;
using Mona.SDK.Core.Assets.Interfaces;

namespace Mona.SDK.Brains.Tiles.Actions.UI
{
    [Serializable]
    public class SetJoypadVisualsInstructionTile : InstructionTile, IActionInstructionTile, IInstructionTileWithPreload, ITickAfterInstructionTile
    {
        public const string ID = "SetJoypadVisuals";
        public const string NAME = "Set Joypad Visuals";
        public const string CATEGORY = "User Interface";
        public override Type TileType => typeof(SetJoypadVisualsInstructionTile);

        [SerializeField] private ScreenJoypadDefaultScreenSide _screenSide = ScreenJoypadDefaultScreenSide.Left;
        [BrainPropertyEnum(true)] public ScreenJoypadDefaultScreenSide ScreenSide { get => _screenSide; set => _screenSide = value; }

        [SerializeField] private ScreenJoypadElementBaseType _elementType = ScreenJoypadElementBaseType.Background;
        [BrainPropertyEnum(true)] public ScreenJoypadElementBaseType ElementType { get => _elementType; set => _elementType = value; }

        [SerializeField] private string _spriteAsset = null;
        [BrainPropertyMonaAsset(typeof(IMonaTextureAssetItem))] public string SpriteAsset { get => _spriteAsset; set => _spriteAsset = value; }

        [SerializeField] private Color _elementColor = Color.white;
        [BrainProperty(false)] public Color ElementColor { get => _elementColor; set => _elementColor = value; }

        [SerializeField] private ScreenJoypadElementDisplayType _displayType = ScreenJoypadElementDisplayType.AlwaysEnabled;
        [BrainPropertyEnum(false)] public ScreenJoypadElementDisplayType DisplayType { get => _displayType; set => _displayType = value; }

        [SerializeField] private ScreenJoypadElementSize _sizeType = ScreenJoypadElementSize.Percentage;
        [BrainPropertyEnum(false)] public ScreenJoypadElementSize SizeType { get => _sizeType; set => _sizeType = value; }

        [SerializeField] private float _sizePercentageGeneral = 1f;
        [SerializeField] private string _sizePercentageGeneralName;
        [BrainPropertyShow(nameof(SizeGeneralPercentageDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyShowLabel(nameof(SizeGeneralPercentageDisplay), (int)UIDisplayType.Show, "Size")]
        [BrainProperty(false)] public float SizePercentageGeneral { get => _sizePercentageGeneral; set => _sizePercentageGeneral = value; }
        [BrainPropertyValueName("SizePercentageGeneral", typeof(IMonaVariablesFloatValue))] public string SizePercentageGeneralName { get => _sizePercentageGeneralName; set => _sizePercentageGeneralName = value; }

        [SerializeField] private float _sizePixelsGeneral = 256f;
        [SerializeField] private string _sizePixelsGeneralName;
        [BrainPropertyShow(nameof(SizeGeneralPixelDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyShowLabel(nameof(SizeGeneralPixelDisplay), (int)UIDisplayType.Show, "Size")]
        [BrainProperty(false)] public float SizePixelsGeneral { get => _sizePixelsGeneral; set => _sizePixelsGeneral = value; }
        [BrainPropertyValueName("SizePixelsGeneral", typeof(IMonaVariablesFloatValue))] public string SizePixelsGeneralName { get => _sizePixelsGeneralName; set => _sizePixelsGeneralName = value; }

        [SerializeField] private float _sizePercentageHandle = 0.5f;
        [SerializeField] private string _sizePercentageHandleName;
        [BrainPropertyShow(nameof(SizeHandlePercentageDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyShowLabel(nameof(SizeHandlePercentageDisplay), (int)UIDisplayType.Show, "Size")]
        [BrainProperty(false)] public float SizePercentageHandle { get => _sizePercentageHandle; set => _sizePercentageHandle = value; }
        [BrainPropertyValueName("SizePercentageHandle", typeof(IMonaVariablesFloatValue))] public string SizePercentageHandleName { get => _sizePercentageHandleName; set => _sizePercentageHandleName = value; }

        [SerializeField] private float _sizePixelsHandle = 128f;
        [SerializeField] private string _sizePixelsHandleName;
        [BrainPropertyShow(nameof(SizeHandlePixelDisplay), (int)UIDisplayType.Show)]
        [BrainPropertyShowLabel(nameof(SizeHandlePixelDisplay), (int)UIDisplayType.Show, "Size")]
        [BrainProperty(false)] public float SizePixelsHandle { get => _sizePixelsHandle; set => _sizePixelsHandle = value; }
        [BrainPropertyValueName("SizePixelsHandle", typeof(IMonaVariablesFloatValue))] public string SizePixelsHandleName { get => _sizePixelsHandleName; set => _sizePixelsHandleName = value; }

        [SerializeField] private ScreenJoypadScaleWithMagnitudeType _inputScalesElement = ScreenJoypadScaleWithMagnitudeType.DoNotChange;
        [BrainPropertyEnum(false)] public ScreenJoypadScaleWithMagnitudeType InputScalesElement { get => _inputScalesElement; set => _inputScalesElement = value; }

        [SerializeField] private float _minScalePercentage = 0.5f;
        [SerializeField] private string _minScalePercentageName;
        [BrainPropertyShow(nameof(InputScalesElement), (int)ScreenJoypadScaleWithMagnitudeType.ChangeWithMagnitude)]
        [BrainProperty(false)] public float MinScalePercentage { get => _minScalePercentage; set => _minScalePercentage = value; }
        [BrainPropertyValueName("MinScalePercentage", typeof(IMonaVariablesFloatValue))] public string MinScalePercentageName { get => _minScalePercentageName; set => _minScalePercentageName = value; }

        [SerializeField] private ScreenJoypadScaleWithMagnitudeType _inputFadesElement = ScreenJoypadScaleWithMagnitudeType.DoNotChange;
        [BrainPropertyEnum(false)] public ScreenJoypadScaleWithMagnitudeType InputFadesElement { get => _inputFadesElement; set => _inputFadesElement = value; }

        [SerializeField] private float _minFadePercentage = 0f;
        [SerializeField] private string _minFadePercentageName;
        [BrainPropertyShow(nameof(InputFadesElement), (int)ScreenJoypadScaleWithMagnitudeType.ChangeWithMagnitude)]
        [BrainProperty(false)] public float MinFadePercentage { get => _minFadePercentage; set => _minFadePercentage = value; }
        [BrainPropertyValueName("MinFadePercentage", typeof(IMonaVariablesFloatValue))] public string MinFadePercentageName { get => _minFadePercentageName; set => _minFadePercentageName = value; }

        public SetJoypadVisualsInstructionTile() { }

        private IMonaBrain _brain;
        private EasyUIScreenInput _screenInput;
        private Sprite _formattedSprite;

        public UIDisplayType SizeGeneralPercentageDisplay => _elementType != ScreenJoypadElementBaseType.Handle && _sizeType == ScreenJoypadElementSize.Percentage ? UIDisplayType.Show : UIDisplayType.Hide;
        public UIDisplayType SizeGeneralPixelDisplay => _elementType != ScreenJoypadElementBaseType.Handle && _sizeType == ScreenJoypadElementSize.Pixels ? UIDisplayType.Show : UIDisplayType.Hide;
        public UIDisplayType SizeHandlePercentageDisplay => _elementType == ScreenJoypadElementBaseType.Handle && _sizeType == ScreenJoypadElementSize.Percentage ? UIDisplayType.Show : UIDisplayType.Hide;
        public UIDisplayType SizeHandlePixelDisplay => _elementType == ScreenJoypadElementBaseType.Handle && _sizeType == ScreenJoypadElementSize.Pixels ? UIDisplayType.Show : UIDisplayType.Hide;

        private float ElementSizePercentage => _elementType != ScreenJoypadElementBaseType.Handle ? _sizePercentageGeneral : _sizePercentageHandle;
        private float ElementSizePixels => _elementType != ScreenJoypadElementBaseType.Handle ? _sizePixelsGeneral : _sizePixelsHandle;
        

        public enum UIDisplayType
        {
            Show = 0,
            Hide = 10
        }

        public void Preload(IMonaBrain brain)
        {
            _brain = brain;
            _screenInput = MonaGlobalBrainRunner.Instance.ScreenInput;
            _screenInput.Initialize();
            SetupTexture();
        }

        private void SetupTexture()
        {
            if (string.IsNullOrEmpty(_spriteAsset))
                return;

            IMonaTextureAssetItem textureAsset = (IMonaTextureAssetItem)_brain.GetMonaAsset(_spriteAsset);
            Texture2D tex = TextureToTexture2D(textureAsset.Value);
            _formattedSprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        }

        private Texture2D TextureToTexture2D(Texture texture)
        {
            Texture2D texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
            Graphics.Blit(texture, renderTexture);

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();

            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(renderTexture);

            return texture2D;
        }

        public override InstructionTileResult Do()
        {
            if (_brain == null || _screenInput == null)
                return Complete(InstructionTileResult.Failure, MonaBrainConstants.INVALID_VALUE);

            if (!string.IsNullOrEmpty(_sizePercentageGeneralName))
                _sizePercentageGeneral = _brain.Variables.GetFloat(_sizePercentageGeneralName);

            if (!string.IsNullOrEmpty(_sizePixelsGeneralName))
                _sizePixelsGeneral = _brain.Variables.GetFloat(_sizePixelsGeneralName);

            if (!string.IsNullOrEmpty(_sizePercentageHandleName))
                _sizePercentageHandle = _brain.Variables.GetFloat(_sizePercentageHandleName);

            if (!string.IsNullOrEmpty(_sizePixelsHandleName))
                _sizePixelsHandle = _brain.Variables.GetFloat(_sizePixelsHandleName);

            if (!string.IsNullOrEmpty(_minScalePercentageName))
                _minScalePercentage = _brain.Variables.GetFloat(_minScalePercentageName);

            if (!string.IsNullOrEmpty(_minFadePercentageName))
                _minFadePercentage = _brain.Variables.GetFloat(_minFadePercentageName);

            EasyUIJoypadVisualElement element = new EasyUIJoypadVisualElement
            {
                ElementSprite = _formattedSprite,
                ElementColor = _elementColor,
                DisplayType = _displayType,
                SizeType = _sizeType,
                SizePercentage = ElementSizePercentage,
                SizePixels = ElementSizePixels,
                InputScalesElement = _inputScalesElement,
                MinScalePercentage = _minScalePercentage,
                InputFadesElement = _inputFadesElement,
                MinFadePercentage = _minFadePercentage,
            };

            _screenInput.UpdateJoypadVisualElement(_screenSide, _elementType, element);

            return Complete(InstructionTileResult.Success);
        }
    }


}