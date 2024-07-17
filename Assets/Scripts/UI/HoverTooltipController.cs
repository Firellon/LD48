using System.Collections;
using System.Linq;
using DG.Tweening;
using Signals;
using TMPro;
using UI.Signals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class HoverTooltipController : MonoBehaviour
    {
        [SerializeField] private GameObject tooltipGameObject;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Vector3 tooltipOffsetTop = new(0, 60);
        [SerializeField] private Vector3 tooltipOffsetBottom = new(0, -20);

        private RectTransform tooltipRectTransform;
        
        private Coroutine startTooltipHoverCoroutine;
        private Coroutine hideHoverTooltipCoroutine;

        private void OnEnable()
        {
            SignalsHub.AddListener<ShowHoverTooltipCommand>(ShowHoverTooltip);
            SignalsHub.AddListener<HideHoverTooltipCommand>(HideHoverTooltip);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<ShowHoverTooltipCommand>(ShowHoverTooltip);
            SignalsHub.RemoveListener<HideHoverTooltipCommand>(HideHoverTooltip);
        }

        private void Start()
        {
            tooltipRectTransform = tooltipGameObject.transform as RectTransform;
            HideHoverTooltip(new HideHoverTooltipCommand());
        }

        private void Update()
        {
            if (!UIExtensions.IsPointerOverUIElement() && UIExtensions.IsPointerOverElement(IsHoverTooltipTarget, out var hoverTooltipTargets))
            {
                var firstRaycastTarget = hoverTooltipTargets.First();
                var firstTooltipTarget = firstRaycastTarget.gameObject.GetComponent<IHoverTooltipTarget>();
                var targetScreenPosition = Camera.main.WorldToScreenPoint(firstRaycastTarget.gameObject.transform.position); // TODO: Inject
                ShowHoverTooltip(new ShowHoverTooltipCommand(
                    targetScreenPosition,
                    firstTooltipTarget.TooltipText));
            }
            else
            {
                HideHoverTooltip(new HideHoverTooltipCommand());
            }
        }

        private bool IsHoverTooltipTarget(RaycastResult raycastResult)
        {
            return raycastResult.gameObject.GetComponent<IHoverTooltipTarget>() != null;
        }

        private void ShowHoverTooltip(ShowHoverTooltipCommand command)
        {
            tooltipRectTransform.pivot = new Vector2(
                command.HoverPosition.x >= Screen.width / 2f ? 1 : 0, 
                0.5f);
            var pivotedOffset = command.HoverPosition.y >= Screen.height / 2f ? tooltipOffsetBottom : tooltipOffsetTop;
            tooltipRectTransform.position = command.HoverPosition + pivotedOffset;

            tooltipText.text = command.TooltipText;
            tooltipGameObject.SetActive(true);
            
            if (startTooltipHoverCoroutine != null) return;
            startTooltipHoverCoroutine = StartCoroutine(StartHoverTooltipCoroutine());
        }
        
        private IEnumerator StartHoverTooltipCoroutine()
        {
            yield return tooltipText.DOFade(1f, 1f);
            startTooltipHoverCoroutine = null;
        }

        private void HideHoverTooltip(HideHoverTooltipCommand command)
        {
            if (hideHoverTooltipCoroutine != null) return;
            hideHoverTooltipCoroutine = StartCoroutine(HideHoverTooltipCoroutine());
        }

        private IEnumerator HideHoverTooltipCoroutine()
        {
            yield return tooltipText.DOFade(0f, 1f);
            tooltipGameObject.SetActive(false);
            hideHoverTooltipCoroutine = null;
        }
    }
}