using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mona.SDK.Core.EasyUI;

namespace Mona.SDK.Brains.Core.ScriptableObjects.Dialog
{
    public interface IDialogStyle
    {
        TMP_FontAsset TextFont { get; }
        Color TextColor { get; }
        EasyUITextAlignment TextAlignment { get; }
        DialogShadowSettings TextShadow { get; }

        Sprite DialogBoxSprite { get; }
        Color DialogBoxColor { get; }
        Sprite TailSprite { get; }
        Color TailColor { get; }
        DialogShadowSettings BoxShadow { get; }

        DialogDisplaySpace DisplaySpace { get; }
        DialogScreenSpace ScreenLocation { get; }
        DialogObjectSpace ObjectLocation { get; }
        Vector2 DisplayOffset { get; }
        DialogBoxSize DialogBoxSize { get; }
        Vector2 MaxSize { get; }

        DialogAudioType AudioType { get; }
        DialogTextRevealSpeed TextRevealSpeed { get; }
    }
}

