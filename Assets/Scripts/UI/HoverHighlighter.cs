using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(IHighlightable))]
    public class HoverHighlighter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private IHighlightable highlightable;

        private void Start()
        {
            highlightable = GetComponent<IHighlightable>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            highlightable?.SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            highlightable?.SetHighlight(false);
        }
    }
}