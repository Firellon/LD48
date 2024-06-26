using FunkyCode;
using UnityEngine;
using Zenject;

namespace Shaders.Clouds
{
    public class CloudsController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Material cloudsMaterial;

        [Inject] private ILightCycle lightCycle;

        private static readonly int Transparency = Shader.PropertyToID("_Transparency");

        private void Awake()
        {
            cloudsMaterial = spriteRenderer.material;
        }

        private void Update()
        {
            cloudsMaterial.SetFloat(Transparency, 1f - lightCycle.Time);
        }
    }
}