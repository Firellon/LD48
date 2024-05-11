using System;
using System.Collections;
using DG.Tweening;
using Plugins.Sirenix.Odin_Inspector.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Day
{
    public class DayNightCycle : MonoBehaviour, IDayNightCycle
    {
        private TerrainGenerator terrainGenerator;

        public float cycleLength = 10f;
        private float currentCycleTime = 15f;

        public int nightLengthGrowth = 2;

        public UnityEngine.Rendering.Universal.Light2D globalLight;
        public TMP_Text cycleMessage;

        [SerializeField, FormerlySerializedAs("DayTimeColor")]
        private DayTimeToColorDictionary dayTimeColor = new();

        [SerializeField, FormerlySerializedAs("DayTimeIntensity")]
        private DayTimeToFloatDictionary dayTimeIntensity = new();

        // TODO: Replace with loca terms
        [SerializeField] private DayTimeToStringDictionary dayTimeMessages = new()
        {
            {DayTime.Morning, "Day grows in power..."},
            {DayTime.Day, "Sunset approaches..."},
            {DayTime.Evening, "The Night arrived!\n Hide and cower, for its terrors are upon you!"},
            {DayTime.Night, "Night has passed.\n Dawn of Day {0}"},
        };

        [SerializeField] private DayTimeToDayTimeDictionary nextDayTime = new()
        {
            {DayTime.Morning, DayTime.Day},
            {DayTime.Day, DayTime.Evening},
            {DayTime.Evening, DayTime.Night},
            {DayTime.Night, DayTime.Morning},
        };

        private DayTime currentCycle = DayTime.Morning;
        private float targetIntensity = 1f;
        private Color targetColor = Color.white;

        private int currentDay = 1;

        void Start()
        {
            // TODO: Inject
            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();

            currentCycleTime = cycleLength;
            UpdateTargetLight();

            globalLight.intensity = targetIntensity;
            globalLight.color = targetColor;
            cycleMessage.text = "";
            StartCoroutine(ShowCycleMessage(DayTime.Night));
        }

        void Update()
        {
            if (Math.Abs(globalLight.intensity - targetIntensity) > 0.05f)
            {
                globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, 0.005f);
                globalLight.color = Color.Lerp(globalLight.color, targetColor, 0.01f);
            }

            if (currentCycleTime > 0f)
            {
                currentCycleTime -= Time.deltaTime;
            }
            else
            {
                Debug.Log($"Change Cycle Time: {currentCycle}");
                StartCoroutine(ShowCycleMessage(currentCycle));
                currentCycle = GetNextCycle(currentCycle);
                currentCycleTime = GetCycleLength(currentCycle);
                UpdateTargetLight();
                if (currentCycle == DayTime.Night)
                {
                    terrainGenerator.GenerateGhosts();
                    terrainGenerator.GenerateItems(0.05f);
                    terrainGenerator.GenerateStrangers(0.05f);
                }

                if (currentCycle == DayTime.Morning)
                {
                    currentDay++;
                    terrainGenerator.DestroyGhosts();
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
            return dayTimeMessages.ContainsKey(dayTime)
                ? string.Format(dayTimeMessages[dayTime], currentDay)
                : GetDayTimeNotFoundMessage(dayTime);
        }

        private string GetDayTimeNotFoundMessage(DayTime dayTime)
        {
            return $"[DayTime {dayTime} not found!]";
        }

        private DayTime GetNextCycle(DayTime cycle)
        {
            return nextDayTime[cycle];
        }

        private void UpdateTargetLight()
        {
            targetIntensity = dayTimeIntensity[currentCycle];
            targetColor = dayTimeColor[currentCycle];
        }

        public DayTime GetCurrentCycle()
        {
            return currentCycle;
        }

        public float TargetIntensity => targetIntensity;
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