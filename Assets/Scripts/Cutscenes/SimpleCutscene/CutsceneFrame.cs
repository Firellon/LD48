using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LD48.Cutscenes.SimpleCutscene
{
    [Serializable]
    public class CutsceneFrame
    {
        [PreviewField(Height = 100)] public Sprite Sprite;
        [TextArea] public string Text;
    }
}