using System;
using System.Collections;
using DG.Tweening;
using Plugins.Sirenix.Odin_Inspector.Modules;
using FunkyCode;
using LD48.AudioTool;
using Signals;
using TMPro;
using UI.Signals;
using UnityEngine;

namespace Day
{
    public class DayNightCycle : MonoBehaviour, IDayNightCycle
    {
        private TerrainGenerator terrainGenerator;

        [SerializeField] private float cycleLength = 15f;

        [SerializeField] private int nightLengthGrowth = 2;
        
        [SerializeField] private float dayIntensity = 0f;
        [SerializeField] private float nightIntensity = 1f;

        [SerializeField] private DayTimeToDayTimeDictionary nextDayTime;
        [SerializeField] private DayTimeToStringDictionary dayTimeMessages = new()
        {
            {DayTime.DayComing, "Day grows in power..."},
            {DayTime.Day, "Sunset approaches..."},
            {DayTime.NightComing, "The Night arrived!\n Hide and cower, for its terrors are upon you!"},
            {DayTime.Night, "Night has passed.\n Dawn of Day {0}"},
        };

        private DayTime currentCycle = DayTime.Day;

        public DayTime CurrentCycle => currentCycle;

        private int currentDay = 1;

        private LightCycle lightCycle;

        public float TargetIntensity { get; }

        void Start()
        {
            lightCycle = FindObjectOfType<LightCycle>();

            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();

            StartCoroutine(DayNightCycleProcess());
        }

        private void SetDarknessLevel(float value)
        {
            lightCycle.SetTime(value);
        }

        public float GetDarknessLevel()
        {
            return lightCycle.Time;
        }

        private IEnumerator DayNightCycleProcess()
        {
            SetDarknessLevel(GetTargetLightIntensity(CurrentCycle));

            while (true)
            {
                if (CurrentCycle == DayTime.DayComing || CurrentCycle == DayTime.NightComing)
                {
                    var nextLightIntensity = GetTargetLightIntensity(GetNextCycle(CurrentCycle));
                    
                    var currentCycleTime = GetCycleLength(CurrentCycle);

                    yield return DOVirtual
                        .Float(GetDarknessLevel(), nextLightIntensity, currentCycleTime, SetDarknessLevel)
                        .WaitForCompletion();
                }
                else
                {
                    var currentCycleTime = GetCycleLength(CurrentCycle);
                    yield return new WaitForSeconds(currentCycleTime);
                }

                currentCycle = GetNextCycle(CurrentCycle);

                ShowCycleMessage(CurrentCycle);

                if (CurrentCycle == DayTime.DayComing)
                {
                    currentDay++;
                    terrainGenerator.DestroyGhosts();
                }

                if (CurrentCycle == DayTime.Night)
                {
                    terrainGenerator.GenerateGhosts();
                    // terrainGenerator.GenerateItems(0.05f); // TODO: Only generate renewable items here
                    terrainGenerator.GenerateStrangers(0.05f);

                    SignalsHub.DispatchAsync(new PlayMusicSignal
                    {
                        Type = MusicType.NightMusic,
                    });
                }

                if (CurrentCycle == DayTime.Day)
                {
                    SignalsHub.DispatchAsync(new PlayMusicSignal
                    {
                        Type = MusicType.DayMusic,
                    });
                }
            }
        }

        public int GetCurrentDay()
        {
            return currentDay;
        }

        private float GetCycleLength(DayTime dayTime)
        {
            if (dayTime == DayTime.Night)
            {
                return cycleLength + currentDay * nightLengthGrowth;
            }

            // if (dayTime == DayTime.DayComing || dayTime == DayTime.NightComing)
            // {
            //     return cycleLength / 2f;
            // }

            return cycleLength;
        }

        private void ShowCycleMessage(DayTime cycle)
        {
            SignalsHub.DispatchAsync(new SetPlayerTipCommand(GetCycleMessage(cycle)));
        }

        private string GetCycleMessage(DayTime dayTime)
        {
            return dayTimeMessages.ContainsKey(dayTime)
                ? string.Format(dayTimeMessages[dayTime], currentDay)
                : GetDayTimeNotFoundMessage(dayTime);
        }

        private string GetDayTimeNotFoundMessage(DayTime dayTime)
        {
            return $"[DayTime {dayTime} not found!]";
            switch (dayTime)
            {
                case DayTime.DayComing:
                    return "Day grows in power...";
                case DayTime.Day:
                    return "Sunset approaches...";
                case DayTime.NightComing:
                    return "The Night arrived!\n Hide and cower, for its terrors are upon you!";
                case DayTime.Night:
                    return $"Night has passed.\n Dawn of Day {currentDay}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dayTime), dayTime, null);
            }
        }

        private DayTime GetNextCycle(DayTime cycle)
        {
            return nextDayTime[cycle];
        }

        private float GetTargetLightIntensity(DayTime time)
        {
            switch (time)
            {
                case DayTime.Day:
                    return dayIntensity;
                case DayTime.Night:
                    return nightIntensity;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DayTime GetCurrentCycle()
        {
            return CurrentCycle;
        }
    }

    [Serializable]
    public class DayTimeToColorDictionary : UnitySerializedDictionary<DayTime, Color>
    {
    }

    [Serializable]
    public class DayTimeToFloatDictionary : UnitySerializedDictionary<DayTime, float>
    {
    }

    [Serializable]
    public class DayTimeToStringDictionary : UnitySerializedDictionary<DayTime, string>
    {
    }

    [Serializable]
    public class DayTimeToDayTimeDictionary : UnitySerializedDictionary<DayTime, DayTime>
    {
    }
}