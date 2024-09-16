using System.Collections.Generic;
using System.Linq;
using Dialogue;
using Dialogue.Entry;
using Human;
using Inventory;
using LD48;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities.RandomService;
using Zenject;

namespace Environment
{
    public class MapCorpse : MonoBehaviour, IClickDialogueTarget
    {
        [SerializeField] private MapCrate itemContainer; // TODO Inject IItemContainer
        [SerializeField] private Sprite maleSprite;
        [SerializeField] private Sprite femaleSprite;
        [SerializeField] private List<ItemType> startingItemTypes;

        [Inject] private IRandomService randomService;
        [Inject] private IItemRegistry itemRegistry;
        [Inject] private SpriteRenderer spriteRenderer;

        private bool isSetUp = false;
        private HumanGender gender = HumanGender.Male;

        private void OnEnable()
        {
            itemContainer.ItemsUpdatedEvent.AddListener(UpdateDialogueEntry);
        }

        private void OnDisable()
        {
            itemContainer.ItemsUpdatedEvent.RemoveListener(UpdateDialogueEntry);
        }

        public void Start()
        {
            if (isSetUp) return;
            SetHumanGender(randomService.Sample(new List<HumanGender> {HumanGender.Female, HumanGender.Male}));
            var startingItemAmount = randomService.Int(0, 3);
            for (var i = 0; i < startingItemAmount; i++)
            {
                var itemType = randomService.Sample(startingItemTypes);
                var item = itemRegistry.GetItem(itemType);
                itemContainer.AddItem(item);
            }
            UpdateDialogueEntry();
        }

        public void SetHumanGender(HumanGender newGender)
        {
            gender = newGender;
            if (newGender == HumanGender.Male) spriteRenderer.sprite = maleSprite;
            if (newGender == HumanGender.Female) spriteRenderer.sprite = femaleSprite;

            UpdateDialogueEntry();
            isSetUp = true;
        }

        public void SetItems(IEnumerable<Item> items)
        {
            itemContainer.RemoveItems();
            foreach (var item in items)
            {
                itemContainer.AddItem(item);
            }

            UpdateDialogueEntry();
            
            isSetUp = true;
        }

        #region Dialogue
        
        private void UpdateDialogueEntry()
        {
            DialogueEntry = new SerializedDialogueEntry
            {
                EntryDescription = itemContainer.Items.Any() 
                     ? $"This unlucky fellow seems to have something of value on {(gender == HumanGender.Male ? "him" : "her")}"
                     : "Poor soul..."
            };
        }
        public IDialogueEntry DialogueEntry { get; private set; } = new SerializedDialogueEntry();
        public void OnPointerClick(PointerEventData eventData)
        {
            SignalsHub.DispatchAsync(new ShowDialogueEntryCommand(DialogueEntry));
        }
        
        #endregion
    }
}