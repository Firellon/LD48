using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace LD48
{
    public enum DayTime {
        Morning,
        Day,
        Evening,
        Night
    }
    public class DayNightCycle : MonoBehaviour
    {
        private TerrainGenerator terrainGenerator;
        
        public float cycleLength = 10f;
        private float currentCycleTime = 15f;

        public int nightLengthGrowth = 2;

        public Light2D globalLight;
        public TMP_Text cycleMessage;

        public Color morningColor;
        public float morningIntensity = 0.8f;
        public Color dayColor;
        public float dayIntensity = 1f;
        public Color eveningColor;
        public float eveningIntensity = 0.5f;
        public Color nightColor;
        public float nightIntensity = 0.2f;

        private DayTime currentCycle = DayTime.Morning;
        private float targetIntensity = 1f;
        private Color targetColor = Color.white;

        private int currentDay = 1;
        // Start is called before the first frame update
        void Start()
        {
            terrainGenerator = Camera.main.GetComponent<TerrainGenerator>();
            
            currentCycleTime = cycleLength;
            UpdateTargetLight();
            
            globalLight.intensity = targetIntensity;
            globalLight.color = targetColor;
            cycleMessage.text = "";
            StartCoroutine(ShowCycleMessage(DayTime.Night));
        }

        // Update is called once per frame
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
            switch (dayTime)
            {
                case DayTime.Morning:
                    return "Day grows in power...";
                case DayTime.Day:
                    return "Sunset approaches...";
                case DayTime.Evening:
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
                DayTime.Night => DayTime.Morning,
                DayTime.Evening => DayTime.Night,
                DayTime.Day => DayTime.Evening,
                DayTime.Morning => DayTime.Day,
                _ => throw new ArgumentOutOfRangeException(nameof(cycle), currentCycle, null)
            };
        }

        private void UpdateTargetLight()
        {
            switch (currentCycle)
            {
                case DayTime.Morning:
                    targetIntensity = morningIntensity;
                    targetColor = morningColor;
                    break;
                case DayTime.Day:
                    targetIntensity = dayIntensity;
                    targetColor = dayColor;
                    break;
                case DayTime.Evening:
                    targetIntensity = eveningIntensity;
                    targetColor = eveningColor;
                    break;
                case DayTime.Night:
                    targetIntensity = nightIntensity;
                    targetColor = nightColor;
                    break;
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

