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
        [SerializeField] private GameObject sanityGrowthArrow;
        [SerializeField] private GameObject sanityLossArrow;

        private int lastPlayerSanity = 0;

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
            } 
            else if (lastPlayerSanity < evt.Sanity)
            {
                sanityGrowthArrow.SetActive(true);
                sanityLossArrow.SetActive(false); 
            }

            lastPlayerSanity = evt.Sanity;
        }
    }
}