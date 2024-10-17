using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mona.SDK.Brains.EasyUI.ScreenInput.Enums;

namespace Mona.SDK.Brains.EasyUI.ScreenInput.Structs
{
    [System.Serializable]
    public class EasyUIJoypadInputParameters
    {
        public bool Enabled = true;
        public ScreenJoypadInputType InputType = ScreenJoypadInputType.Analog;
        public ScreenJoypadAxisType Axes = ScreenJoypadAxisType.All;
        public ScreenJoypadBaseControlType ControlType = ScreenJoypadBaseControlType.Default;
        public ScreenJoypadPlacementType Placement = ScreenJoypadPlacementType.CenterOnTap;
        public ScreenJoypadMoveMagnitudeType MoveMagnitude = ScreenJoypadMoveMagnitudeType.StartAtDeadzone;
        public ScreenJoypadDisplayType Visiblity = ScreenJoypadDisplayType.AlwaysVisible;

        [SerializeField] private float _deadZone = 0f;
        [SerializeField] private float _handleExtents = 1f;
        [SerializeField] private float _trackingThreshold = 1f;
        [SerializeField] private float _digitalDiagonalThreshold = 0.25f;
        [SerializeField] private Vector2 _startLocation = new Vector2(0.15f, 0.5f);

        public float DeadZone
        {
            get { return _deadZone; }
            set { _deadZone = Mathf.Abs(value); }
        }

        public float HandleExtents
        {
            get { return _handleExtents; }
            set { _handleExtents = Mathf.Abs(value); }
        }

        public float TrackingThreshold
        {
            get { return _trackingThreshold; }
            set { _trackingThreshold = Mathf.Abs(value); }
        }

        public Vector2 StartLocation
        {
            get { return _startLocation; }
            set { _startLocation = value; }
        }

        public float DigitalDiagonalThreshold
        {
            get { return _digitalDiagonalThreshold; }
            set { _digitalDiagonalThreshold = Mathf.Clamp(Mathf.Abs(value), 0f, 0.4f); }
        }
    }
}
