using System.Collections.Generic;
using Human;
using Inventory;
using LD48;
using UnityEngine;
using Utilities.RandomService;
using Zenject;

namespace Environment
{
    public class MapCorpse : MonoBehaviour
    {
        [SerializeField] private MapCrate itemContainer; // TODO Inject IItemContainer
        [SerializeField] private SpriteRenderer spriteRenderer; // TODO: inject
        [SerializeField] private Sprite maleSprite;
        [SerializeField] private Sprite femaleSprite;
        [SerializeField] private List<ItemType> startingItemTypes;

        [Inject] private IRandomService randomService;
        [Inject] private IItemRegistry itemRegistry;

        private bool isSetUp;

        public void Start()
        {
            if (isSetUp) return;
            SetUp(randomService.Sample(new List<HumanGender> {HumanGender.Female, HumanGender.Male}));
            var startingItemAmount = randomService.Int(0, 3);
            for (var i = 0; i < startingItemAmount; i++)
            {
                var itemType = randomService.Sample(startingItemTypes);
                var item = itemRegistry.GetItem(itemType);
                itemContainer.AddItem(item);
            }
        }

        public void SetUp(HumanGender gender)
        {
            if (isSetUp) return;
            isSetUp = true;

            if (gender == HumanGender.Male) spriteRenderer.sprite = maleSprite;
            if (gender == HumanGender.Female) spriteRenderer.sprite = femaleSprite;
        }
    }
}