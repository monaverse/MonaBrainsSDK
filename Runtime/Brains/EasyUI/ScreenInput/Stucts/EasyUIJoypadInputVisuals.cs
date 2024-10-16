using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;

namespace Mona.SDK.Brains.EasyUI.ScreenInput.Structs
{
    [System.Serializable]
    public class EasyUIJoypadVisualElement
    {
        public Sprite ElementSprite;
        public Color ElementColor = Color.white;
        public ScreenJoypadElementDisplayType DisplayType = ScreenJoypadElementDisplayType.AlwaysEnabled;
        public ScreenJoypadElementSize SizeType = ScreenJoypadElementSize.Percentage;

        [SerializeField] private float _sizePercentage = 1f;
        [SerializeField] private float _sizePixels = 256f;

        public ScreenJoypadScaleWithMagnitudeType InputScalesElement = ScreenJoypadScaleWithMagnitudeType.DoNotChange;
        [SerializeField] private float _minScalePercentage = 0.1f;

        public ScreenJoypadScaleWithMagnitudeType InputFadesElement = ScreenJoypadScaleWithMagnitudeType.DoNotChange;
        [SerializeField] private float _minFadePercentage = 0.1f;

        [HideInInspector] public bool ScaleWasLerped;
        [HideInInspector] public bool AlphaWasLerped;

        public float SizePercentage
        {
            get { return _sizePercentage; }
            set { _sizePercentage = Mathf.Abs(value); }
        }

        public float SizePixels
        {
            get { return _sizePixels; }
            set { _sizePixels = Mathf.Abs(value); }
        }

        public float MinScalePercentage
        {
            get { return _minScalePercentage; }
            set { _minScalePercentage = Mathf.Abs(value); }
        }

        public float MinFadePercentage
        {
            get { return _minFadePercentage; }
            set { _minFadePercentage = Mathf.Clamp01(Mathf.Abs(value)); }
        }
    }

    [System.Serializable]
    public class EasyUIJoypadBaseVisuals
    {
        public EasyUIJoypadVisualElement Background = new EasyUIJoypadVisualElement();
        public EasyUIJoypadVisualElement Handle = new EasyUIJoypadVisualElement();
        public EasyUIJoypadVisualElement Pointer = new EasyUIJoypadVisualElement();
    }

    [System.Serializable]
    public class EasyUIJoypadAxisGroupVisuals
    {
        public EasyUIJoypadBaseVisuals AllVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals EightWayVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals FourWayVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals HorizontalVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals VerticalVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals UpVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals DownVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals LeftVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals RightVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals HalfCircleUpVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals HalfCircleDownVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals HalfCircleLeftVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals HalfCircleRightVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals UpAndLeftVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals UpAndRightVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals DownAndLeftVisuals = new EasyUIJoypadBaseVisuals();
        public EasyUIJoypadBaseVisuals DownAndRightVisuals = new EasyUIJoypadBaseVisuals();

        public EasyUIJoypadBaseVisuals GetVisuals(ScreenJoypadAxisType axisType)
        {
            switch (axisType)
            {
                case ScreenJoypadAxisType.All:
                    return AllVisuals;
                case ScreenJoypadAxisType.EightWay:
                    return EightWayVisuals;
                case ScreenJoypadAxisType.FourWay:
                    return FourWayVisuals;
                case ScreenJoypadAxisType.Horizontal:
                    return HorizontalVisuals;
                case ScreenJoypadAxisType.Vertical:
                    return VerticalVisuals;
                case ScreenJoypadAxisType.Up:
                    return UpVisuals;
                case ScreenJoypadAxisType.Down:
                    return DownVisuals;
                case ScreenJoypadAxisType.Left:
                    return LeftVisuals;
                case ScreenJoypadAxisType.Right:
                    return RightVisuals;
                case ScreenJoypadAxisType.HalfCircleUp:
                    return HalfCircleUpVisuals;
                case ScreenJoypadAxisType.HalfCircleDown:
                    return HalfCircleDownVisuals;
                case ScreenJoypadAxisType.HalfCircleLeft:
                    return HalfCircleLeftVisuals;
                case ScreenJoypadAxisType.HalfCircleRight:
                    return HalfCircleRightVisuals;
                case ScreenJoypadAxisType.UpAndLeft:
                    return UpAndLeftVisuals;
                case ScreenJoypadAxisType.UpAndRight:
                    return UpAndRightVisuals;
                case ScreenJoypadAxisType.DownAndLeft:
                    return DownAndLeftVisuals;
                case ScreenJoypadAxisType.DownAndRight:
                    return DownAndRightVisuals;
            }

            return AllVisuals;
        }
    }

    [System.Serializable]
    public class EasyUIJoypadInputVisuals
    {
        public EasyUIJoypadAxisGroupVisuals AnalogVisuals = new EasyUIJoypadAxisGroupVisuals();
        public EasyUIJoypadAxisGroupVisuals DigitalVisuals = new EasyUIJoypadAxisGroupVisuals();

        public EasyUIJoypadBaseVisuals GetVisuals(ScreenJoypadInputType inputType, ScreenJoypadAxisType axisType)
        {
            EasyUIJoypadAxisGroupVisuals groupVisuals = inputType == ScreenJoypadInputType.Analog ? AnalogVisuals : DigitalVisuals;
            return groupVisuals.GetVisuals(axisType);
        }
    }
}
