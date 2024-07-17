using UnityEngine;

namespace LD48
{
    [CreateAssetMenu(menuName = "LD48/Create VisualsConfig", fileName = "VisualsConfig", order = 0)]
    public class VisualsConfig : ScriptableObject
    {
        [SerializeField] private Material regularInteractableShader;
        [SerializeField] private Material highlightedInteractableShader;
        [SerializeField] private GameObject temporaryItemPrefab;

        public Material RegularInteractableShader => regularInteractableShader;
        public Material HighlightedInteractableShader => highlightedInteractableShader;
        public GameObject TemporaryItemPrefab => temporaryItemPrefab;
    }
}