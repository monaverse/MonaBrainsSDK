using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mona.SDK.Core.EasyUI;

namespace Mona.SDK.Brains.Core.ScriptableObjects.Dialog
{
    [Serializable]
    public enum DialogObjectSpace
    {
        Above = 0,
        AboveRight = 1,
        AboveLeft = 2,
        Below = 10,
        BelowRight = 11,
        BelowLeft = 12,
        Center = 20,
        CenterRight = 21,
        CenterLeft = 22
    }

    [Serializable]
    public enum DialogScreenSpace
    {
        Bottom = 0,
        BottomCenter = 5,
        Center = 20,
        Top = 30,
        TopCenter = 35
    }

    [Serializable]
    public enum DialogAudioType
    {
        None = 0,
        CustomFile = 10,
        GameBabble = 20
    }

    [Serializable]
    public enum DialogTextRevealSpeed
    {
        Instant = 0,
        VerySlow = 10,
        Slow = 20,
        Medium = 30,
        Fast = 40,
        VeryFast = 50
    }

    [Serializable]
    public enum DialogDismissalType
    {
        InputBased = 0,
        DistanceBased = 10,
        TimeBased = 20,
        VocalizationBased = 30,
        None = 50
    }

    [Serializable]
    public enum DialogBoxSize
    {
        Autosize,
        FixedSize
    }

    [Serializable]
    public enum DialogDisplaySpace
    {
        HeadsUpDisplay,
        OnObject
    }

    [Serializable]
    public struct DialogShadowSettings
    {
        public bool enabled;

        [DrawIfBrain("enabled", true)]
        public Vector2 offset;

        [DrawIfBrain("enabled", true)]
        public Color color;
    }

    [CreateAssetMenu(menuName = "Mona Brains/Dialog Style")]
    [Serializable]
    public class DialogStyle : ScriptableObject, IDialogStyle
    {
        [SerializeField] private TMP_FontAsset _textFont;
        [SerializeField] private Color _textColor = Color.black;
        [SerializeField] private EasyUITextAlignment _textAlignment;
        [SerializeField] private DialogShadowSettings _textShadow;
        public Color TextColor { get => _textColor; }
        public TMP_FontAsset TextFont { get => _textFont; }
        public EasyUITextAlignment TextAlignment { get => _textAlignment; }
        public DialogShadowSettings TextShadow { get => _textShadow; }

        [SerializeField] private Sprite _dialogBoxSprite;
        [SerializeField] private Color _dialogBoxColor = Color.white;
        [SerializeField] private Sprite _tailSprite;
        [SerializeField] private Color _tailColor = Color.white;
        [SerializeField] private DialogShadowSettings _boxShadow;
        public Sprite DialogBoxSprite { get => _dialogBoxSprite; }
        public Color DialogBoxColor { get => _dialogBoxColor; }
        public Sprite TailSprite { get => _tailSprite; }
        public Color TailColor { get => _tailColor; }
        public DialogShadowSettings BoxShadow { get => _boxShadow; }

        [SerializeField] private DialogDisplaySpace _displaySpace = DialogDisplaySpace.OnObject;

        [DrawIfBrain("_displaySpace", DialogDisplaySpace.HeadsUpDisplay)]
        [SerializeField] private DialogScreenSpace _screenLocation = DialogScreenSpace.Bottom;

        [DrawIfBrain("_displaySpace", DialogDisplaySpace.OnObject)]
        [SerializeField] private DialogObjectSpace _objectLocation = DialogObjectSpace.Above;

        [DrawIfBrain("_displaySpace", DialogDisplaySpace.OnObject)]
        [SerializeField] private Vector2 _displayOffset = Vector2.zero;

        [DrawIfBrain("_displaySpace", DialogDisplaySpace.OnObject)]
        [SerializeField] private DialogBoxSize _dialogBoxSize = DialogBoxSize.Autosize;

        [DrawIfBrain("_displaySpace", DialogDisplaySpace.OnObject)]
        [SerializeField] private Vector2 _maxSize = new Vector2(50, 20);

        public DialogDisplaySpace DisplaySpace { get => _displaySpace; }
        public DialogScreenSpace ScreenLocation { get => _screenLocation; }
        public DialogObjectSpace ObjectLocation { get => _objectLocation; }
        public Vector2 DisplayOffset { get => _displayOffset; }
        public DialogBoxSize DialogBoxSize { get => _dialogBoxSize; }
        public Vector2 MaxSize { get => _maxSize; }

        [SerializeField] private DialogAudioType _audioType = DialogAudioType.None;
        [SerializeField] private DialogTextRevealSpeed _textRevealSpeed = DialogTextRevealSpeed.Instant;
        public DialogAudioType AudioType { get => _audioType; }
        public DialogTextRevealSpeed TextRevealSpeed { get => _textRevealSpeed; }
    }
}
