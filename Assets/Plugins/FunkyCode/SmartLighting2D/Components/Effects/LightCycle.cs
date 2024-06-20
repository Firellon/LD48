using UnityEngine;

namespace FunkyCode
{
    [System.Serializable]
    public class LightCycleBuffer
    {
        [GradientUsage(true)] public Gradient gradient = new Gradient();
    }

    [System.Serializable]
    public class LightDayProperties
    {
        [Range(0, 360)]
        public float shadowOffset = 0;

        public AnimationCurve shadowHeight = new AnimationCurve();

        public AnimationCurve shadowAlpha = new AnimationCurve();  
    }

    public interface ILightCycle
    {
        public float Time { get; }
    }

    [ExecuteInEditMode]
    public class LightCycle : MonoBehaviour, ILightCycle
    {
        [Range(0, 1)]
        [SerializeField] private float time = 0;
        public float Time => time;

        public float maxDayLighting = 0.1f;

        public LightDayProperties dayProperties = new LightDayProperties();

        public LightCycleBuffer[] nightProperties = new LightCycleBuffer[1]; // lightmap

        public void SetTime(float setTime)
        {
            time = setTime;

            UpdateGlobalDayLighting();
        }

        void LateUpdate()
        {
            LightingSettings.LightmapPresetList lightmapPresets = Lighting2D.Profile.lightmapPresets;

            if (lightmapPresets == null)
            {
                return;
            }

            /*
            if (Input.GetMouseButton(0)&& Input.touchCount > 1) { // 
                time += Time.deltaTime * 0.05f;

                time = time % 1;
            }*/

            float time360 = (time * 360);

            // Day Lighting Properties
            float height = dayProperties.shadowHeight.Evaluate(time);
            float alpha = dayProperties.shadowAlpha.Evaluate(time);

            if (height < 0.01f)
            {
                height = 0.01f;
            }

            if (alpha < 0)
            {
                alpha = 0;
            }

            Lighting2D.DayLightingSettings.height = height;
            Lighting2D.DayLightingSettings.ShadowColor.a = alpha;
            Lighting2D.DayLightingSettings.direction = time360 + dayProperties.shadowOffset;

            UpdateGlobalDayLighting();

            // Dynamic Properties
            for (int i = 0; i < nightProperties.Length; i++)
            {
                if (i >= lightmapPresets.list.Length)
                {
                    return;
                }

                LightCycleBuffer buffer = nightProperties[i];

                if (buffer == null)
                {
                    continue;
                }

                Color color = buffer.gradient.Evaluate(time);

                LightingSettings.LightmapPreset lightmapPreset = lightmapPresets.list[i];
                lightmapPreset.darknessColor = color;
            }
        }

        private void UpdateGlobalDayLighting()
        {
            Shader.SetGlobalFloat("_DayLightGlobalLevel", Mathf.Max(maxDayLighting, time));
        }
    }
}