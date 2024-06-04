using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Random = UnityEngine.Random;

namespace VolFx
{
    [ExecuteAlways]
    public class DitherBackgroundEffect : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float scaleValue;

        [SerializeField] private float minScale = 1f;
        [SerializeField] private float maxScale = 100f;
        
        [SerializeField] private Texture2D ditherTex;

        [SerializeField] private Vector4 ditherMad;
        [SerializeField] private Vector4 patternData;

        #if UNITY_EDITOR
        [Button("Generate")]
        private void Generate()
        {
            var renderer = GetComponent<Renderer>();

            var material = renderer.sharedMaterial;

            var scale = Mathf.Lerp(minScale, maxScale, scaleValue);

            var patternDepth = (float)(ditherTex.width / ditherTex.height);

            ditherMad = new Vector4();

            var aspect = 1f;//Screen.width / (float)Screen.height;

            ditherMad.x = scale * aspect;
            ditherMad.y = scale; 

            // snap to pattern pixels
            var step = ditherTex.width / patternDepth;

            ditherMad.z = Mathf.Round(Random.value * step) / step;
            ditherMad.w = Mathf.Round(Random.value * step) / step;

            patternData = new Vector4(
                ditherMad.x * (ditherTex.width / patternDepth),
                ditherMad.y * ditherTex.height,
                1f / patternDepth, patternDepth
            );

            material.SetVector("_DitherMad", ditherMad);
            material.SetVector("_PatternData", patternData);
            material.SetTexture("_DitherTex", ditherTex);

            EditorUtility.SetDirty(material);
        }
        #endif
    }
}