using System.Collections;
using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Sanity
{
    public class SanityCounterController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sanityText;
        [SerializeField] private Image sanityImage;
        [Space]
        [SerializeField] private GameObject sanityGrowthArrow;
        [SerializeField] private GameObject sanityLossArrow;
        [SerializeField] private float arrowShowSeconds = 1f;

        private int lastPlayerSanity = 0;
        private Coroutine hideHighlightCoroutine;

        private void OnEnable()
        {
            SignalsHub.AddListener<PlayerSanityUpdatedEvent>(OnPlayerSanityUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerSanityUpdatedEvent>(OnPlayerSanityUpdated);
        }

        private void Start()
        {
            sanityGrowthArrow.SetActive(false);
            sanityLossArrow.SetActive(false);
        }

        private void OnPlayerSanityUpdated(PlayerSanityUpdatedEvent evt)
        {
            sanityText.text = evt.Sanity.ToString();

            if (lastPlayerSanity > evt.Sanity)
            {
                sanityGrowthArrow.SetActive(false);
                sanityLossArrow.SetActive(true);
                
                if (hideHighlightCoroutine != null) StopCoroutine(hideHighlightCoroutine);
                hideHighlightCoroutine = StartCoroutine(HideHighlightCoroutine());
            } 
            else if (lastPlayerSanity < evt.Sanity)
            {
                sanityGrowthArrow.SetActive(true);
                sanityLossArrow.SetActive(false); 
                
                if (hideHighlightCoroutine != null) StopCoroutine(hideHighlightCoroutine);
                hideHighlightCoroutine = StartCoroutine(HideHighlightCoroutine());
            }

            lastPlayerSanity = evt.Sanity;
        }

        private IEnumerator HideHighlightCoroutine()
        {
            yield return new WaitForSeconds(arrowShowSeconds);
            
            sanityGrowthArrow.SetActive(false);
            sanityLossArrow.SetActive(false);
        }
    }
}