using System;
using UnityEngine;

namespace LD48.Cutscenes
{
    [Serializable]
    public struct CutsceneData
    {
        public CutsceneType cutsceneType;
        public GameObject cutsceneObject;
    }
}