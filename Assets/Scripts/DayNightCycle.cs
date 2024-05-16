using System;
using System.Collections;
using DG.Tweening;
using FunkyCode;
using TMPro;
using UnityEngine;


namespace LD48
{
    public enum DayTime {
        Day = 1,
        Night = 2,
        DayComing = 3,
        NightComing = 4,
    }

    public class DayNightCycle : MonoBehaviour
    {
        private TerrainGenerator terrainGenerator;

        [SerializeField] private float cycleLength = 15f;

        public int nightLengthGrowth = 2;

        public TMP_Text cycleMessage;

        public float morningIntensity = 0.2f;
        public float dayIntensity = 0f;
        public float eveningIntensity = 0.2f;
        public float nightIntensity = 1f;

        private DayTime currentCycle = DayTime.Day;

        private int currentDay = 1;

        private LightCycle lightCycle;

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
            return lightCycle.time;
        }

        private IEnumerator DayNightCycleProcess()
        {
            SetDarknessLevel(GetTargetLightIntensity(currentCycle));
            cycleMessage.SetText("");

            while (true)
            {
                if (currentCycle == DayTime.DayComing || currentCycle == DayTime.NightComing)
                {
                    var nextLightIntensity = GetTargetLightIntensity(GetNextCycle(currentCycle));
                    
                    var currentCycleTime = GetCycleLength(currentCycle);

                    yield return DOVirtual
                        .Float(GetDarknessLevel(), nextLightIntensity, currentCycleTime, SetDarknessLevel)
                        .WaitForCompletion();
                }
                else
                {
                    var currentCycleTime = GetCycleLength(currentCycle);
                    yield return new WaitForSeconds(currentCycleTime);
                }

                currentCycle = GetNextCycle(currentCycle);

                StartCoroutine(ShowCycleMessage(currentCycle));

                if (currentCycle == DayTime.DayComing)
                {
                    currentDay++;
                    terrainGenerator.DestroyGhosts();
                }

                if (currentCycle == DayTime.Night)
                {
                    terrainGenerator.GenerateGhosts();
                    terrainGenerator.GenerateItems(0.05f);
                    terrainGenerator.GenerateStrangers(0.05f);
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

        private IEnumerator ShowCycleMessage(DayTime cycle)
        {
            cycleMessage.alpha = 0f;
            cycleMessage.text = GetCycleMessage(cycle);

            yield return cycleMessage.DOFade(1f, 0.5f);

            yield return new WaitForSeconds(5f);

            yield return cycleMessage.DOFade(0f, 0.5f);
        }

        private string GetCycleMessage(DayTime dayTime)
        {
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
            return cycle switch
            {
                DayTime.Night => DayTime.DayComing,
                DayTime.DayComing => DayTime.Day,
                DayTime.Day => DayTime.NightComing,
                DayTime.NightComing => DayTime.Night,
                _ => throw new ArgumentOutOfRangeException(nameof(cycle), currentCycle, null)
            };
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
            return currentCycle;
        }
    }
}

