using System;
using System.Collections.Generic;
using Signals;
using UnityEngine;

namespace LD48.Cutscenes
{
    public enum CutsceneType
    {
        Intro,
    }

    public struct StartCutsceneSignal
    {
        public CutsceneType Type;
    }

    public struct StopCutsceneSignal
    {
        
    }

    public class CutsceneManager : MonoBehaviour
    {
        [SerializeField] private List<CutsceneData> cutscenes = new();

        public static Dictionary<CutsceneType, GameObject> cutsceneDataBase = new();

        public static GameObject activeCutscene;

        private void Awake()
        {
            InitializeCutsceneDataBase();

            foreach (var cutscene in cutsceneDataBase)
            {
                cutscene.Value.SetActive(false);
            }
        }

        private void OnEnable()
        {
            SignalsHub.AddListener<StartCutsceneSignal>(OnStartCutsceneSignal);
            SignalsHub.AddListener<StopCutsceneSignal>(OnStopCutsceneSignal);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<StartCutsceneSignal>(OnStartCutsceneSignal);
            SignalsHub.RemoveListener<StopCutsceneSignal>(OnStopCutsceneSignal);
        }

        private void OnStartCutsceneSignal(StartCutsceneSignal signal)
        {
            StartCutscene(signal.Type);
        }

        private void OnStopCutsceneSignal(StopCutsceneSignal signal)
        {
            StopCutscene();
        }

        private void InitializeCutsceneDataBase()
        {
            cutsceneDataBase.Clear();

            for (int i = 0; i < cutscenes.Count; i++)
            {           
                cutsceneDataBase.Add(cutscenes[i].cutsceneType, cutscenes[i].cutsceneObject);
            }
        }

        private void StartCutscene(CutsceneType cutsceneKey)
        {
            if (!cutsceneDataBase.ContainsKey(cutsceneKey)) 
            {
                Debug.LogError($"Cannot find a cutscene with the name '{cutsceneKey}'");
                return;
            } 

            if (activeCutscene != null)
            {
                if (activeCutscene == cutsceneDataBase[cutsceneKey])
                {
                    return;
                }
            }

            activeCutscene = cutsceneDataBase[cutsceneKey];

            foreach (var cutscene in cutsceneDataBase)
            {
                cutscene.Value.SetActive(false);
            }

            cutsceneDataBase[cutsceneKey].SetActive(true);
        }

        private void StopCutscene()
        {
            if (activeCutscene != null)
            {
                activeCutscene.SetActive(false);
                activeCutscene = null;
            }
        }
    }

    [Serializable]
    public struct CutsceneData
    {
        public CutsceneType cutsceneType;
        public GameObject cutsceneObject;
    }
}