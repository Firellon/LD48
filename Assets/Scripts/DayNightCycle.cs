using System;
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
            currentCycle = GetNextCycle(currentCycle);
            currentCycleTime = cycleLength;
            UpdateTargetLight();
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
