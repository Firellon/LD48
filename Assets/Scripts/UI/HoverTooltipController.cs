using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class HoverTooltipController : MonoBehaviour
    {
        [SerializeField] private GameObject tooltipGameObject;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private float tooltipAnimationDurationSeconds = 1f;

        private RectTransform tooltipRectTransform;

        private Coroutine startTooltipHoverCoroutine;
        private Coroutine hideHoverTooltipCoroutine;

        private void Start()
        {
            tooltipRectTransform = tooltipGameObject.transform as RectTransform;
            HideHoverTooltip();
        }

        private void Update()
        {
            if (!UIExtensions.IsPointerOverUIElement() &&
                UIExtensions.IsPointerOverElement(IsHoverTooltipTarget, out var hoverTooltipTargets))
            {
                var firstRaycastTarget = hoverTooltipTargets.First();
                var firstTooltipTarget = firstRaycastTarget.gameObject.GetComponent<IHoverTooltipTarget>();
                var targetScreenPosition =
                    Camera.main.WorldToScreenPoint(firstRaycastTarget.gameObject.transform.position); // TODO: Inject
                ShowHoverTooltip(targetScreenPosition, firstTooltipTarget);
            }
            else
            {
                HideHoverTooltip();
            }
        }

        private bool IsHoverTooltipTarget(RaycastResult raycastResult)
        {
            return raycastResult.gameObject.GetComponent<IHoverTooltipTarget>() != null;
        }

        private void ShowHoverTooltip(Vector3 hoverPosition, IHoverTooltipTarget tooltipTarget)
        {
            var isRightScreenHalf = hoverPosition.x >= Screen.width / 2f;
            var isTopScreenHalf = hoverPosition.y >= Screen.height / 2f;
            tooltipRectTransform.pivot = new Vector2(
                isRightScreenHalf ? 1 : 0,
                0.5f);
            var pivotedOffset = new Vector3(
                isRightScreenHalf ? tooltipTarget.RightTopTooltipOffset.x : tooltipTarget.LeftBottomTooltipOffset.x,
                isTopScreenHalf ? tooltipTarget.RightTopTooltipOffset.y : tooltipTarget.LeftBottomTooltipOffset.y);
            tooltipRectTransform.position = hoverPosition + pivotedOffset;

            tooltipText.text = tooltipTarget.TooltipText;
            tooltipGameObject.SetActive(true);

            if (startTooltipHoverCoroutine != null) return;
            startTooltipHoverCoroutine = StartCoroutine(StartHoverTooltipCoroutine());
        }

        private IEnumerator StartHoverTooltipCoroutine()
        {
            yield return tooltipText.DOFade(1f, tooltipAnimationDurationSeconds);
            startTooltipHoverCoroutine = null;
        }

        private void HideHoverTooltip()
        {
            if (!tooltipGameObject.activeSelf || hideHoverTooltipCoroutine != null) return;
            hideHoverTooltipCoroutine = StartCoroutine(HideHoverTooltipCoroutine());
        }

        private IEnumerator HideHoverTooltipCoroutine()
        {
            yield return tooltipText.DOFade(0f, tooltipAnimationDurationSeconds);
            tooltipGameObject.SetActive(false);
            hideHoverTooltipCoroutine = null;
        }
    }
}