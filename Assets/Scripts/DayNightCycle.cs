using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public enum DayTime {
    Morning,
    Day,
    Evening,
    Night
}
public class DayNightCycle : MonoBehaviour
{
    public float cycleLength = 15f;
    private float currentCycleTime = 15f;

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
    // Start is called before the first frame update
    void Start()
    {
        currentCycleTime = cycleLength;
        UpdateTargetLight();
        
        globalLight.intensity = targetIntensity;
        globalLight.color = targetColor;
        cycleMessage.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (Math.Abs(globalLight.intensity - targetIntensity) > 0.05f)
        {
            globalLight.intensity = Mathf.Lerp(globalLight.intensity, targetIntensity, 0.01f);
            globalLight.color = Color.Lerp(globalLight.color, targetColor, 0.01f);
        }

        if (currentCycleTime > 0f)
        {
            currentCycleTime -= Time.deltaTime;
        }
        else
        {
            Debug.Log($"Change Cycle Time: {currentCycle}");
            StartCoroutine(ShowCycleMessage(GetCycleMessage(currentCycle)));
            currentCycle = GetNextCycle(currentCycle);
            currentCycleTime = cycleLength;
            UpdateTargetLight();
        }
    }

    private IEnumerator ShowCycleMessage(string getCycleMessage)
    {
        cycleMessage.alpha = 0f;
        cycleMessage.text = GetCycleMessage(currentCycle);
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
                return "The Night arrived! Hide and cower, for its terrors are upon you!";
            case DayTime.Night:
                return "The Night has passed. Bless the raising Dawn!";
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
}
