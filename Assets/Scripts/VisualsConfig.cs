using UnityEngine;

namespace LD48
{
    [CreateAssetMenu(menuName = "LD48/Create VisualsConfig", fileName = "VisualsConfig", order = 0)]
    public class VisualsConfig : ScriptableObject
    {
        [SerializeField] private Material regularInteractableShader;
        [SerializeField] private Material highlightedInteractableShader;

        public Material RegularInteractableShader => regularInteractableShader;
        public Material HighlightedInteractableShader => highlightedInteractableShader;
    }
}