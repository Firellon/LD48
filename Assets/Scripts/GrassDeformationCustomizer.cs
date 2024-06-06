using UnityEngine;

namespace LD48
{
    public class GrassDeformationCustomizer : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;

        [SerializeField] private float divider = 100f;

        private static readonly int WorldPos = Shader.PropertyToID("_WorldPos");

        private void Update()
        {
            renderer.material.SetVector(WorldPos, transform.position / divider);
        }
    }
}