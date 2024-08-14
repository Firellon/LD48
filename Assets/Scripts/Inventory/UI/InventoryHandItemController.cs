using System;
using Inventory.Signals;
using Map;
using Map.Actor;
using Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities.Monads;
using Zenject;

namespace Inventory.UI
{
    public class InventoryHandItemController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private Sprite emptyHandSprite;

        [Inject] private IMapActorRegistry mapActorRegistry;

        private IMaybe<Item> currentHandItem = Maybe.Empty<Item>();


        public void OnEnable()
        {
            SignalsHub.AddListener<PlayerHandItemUpdatedEvent>(OnHandItemUpdated);
        }

        private void OnDisable()
        {
            SignalsHub.RemoveListener<PlayerHandItemUpdatedEvent>(OnHandItemUpdated);
        }

        private void OnHandItemUpdated(PlayerHandItemUpdatedEvent signal)
        {
            SetHandItem(signal.MaybeItem);
        }

        private void Start()
        {
            mapActorRegistry.Player.IfPresent(player => { SetHandItem(player.HandItem); })
                .IfNotPresent(() => { SetHandItem(Maybe.Empty<Item>()); });
        }

        private void SetHandItem(IMaybe<Item> maybeItem)
        {
            currentHandItem = maybeItem;
            currentHandItem.IfPresent(item => { itemIcon.sprite = item.InventoryItemSprite; })
                .IfNotPresent(() => { itemIcon.sprite = emptyHandSprite; });
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            currentHandItem.IfPresent(handItem =>
            {
                mapActorRegistry.Player.IfPresent(player =>
                {
                    if (player.Inventory.CanAddItem())
                    {
                        player.Inventory.AddItem(handItem);
                        SetHandItem(Maybe.Empty<Item>());
                        player.Inventory.SetHandItem(Maybe.Empty<Item>());   
                    }
                    else
                    {
                        Debug.LogWarning("Can't Move Hand item into a full inventory!");
                        // TODO: Drop?
                    }
                });
            });
        }
    }
}