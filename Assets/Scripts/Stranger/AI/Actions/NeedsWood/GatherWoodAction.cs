using System.Linq;
using BehaviorTree;
using Inventory;
using LD48;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI
{
    public class GatherWoodAction : Node
    {
        [Inject] private StrangerAiConfig config;
        [Inject] private Transform transform;
        [Inject] private IItemContainer inventory;

        private readonly StrangerAICalculationState aiState;

        public GatherWoodAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            var closestWood = Physics2D
                .OverlapCircleAll(transform.position, config.ItemGatherRadius, config.ItemLayerMask)
                .Select(collider => collider.gameObject.GetComponent<ItemController>())
                .Where(itemController => itemController != null && itemController.Item.ItemType == ItemType.Wood)
                .OrderBy(wood => Vector2.Distance(transform.position, wood.transform.position))
                .ToList();

            if (closestWood.Any())
            {
                aiState.MaybeTarget = closestWood.Select(wood => wood.transform).FirstOrEmpty();
                aiState.TargetAction = StrangerState.Gather;
                State = NodeState.Success;
            }
            else
            {
                var closestContainersWithWood = Physics2D
                    .OverlapCircleAll(transform.position, config.ItemGatherRadius, config.ItemContainerLayerMask)
                    .Select(collider => collider.gameObject.GetComponent<IItemContainer>())
                    .Where(itemContainer => itemContainer != null && itemContainer != inventory &&
                                            itemContainer.HasItem(ItemType.Wood) && itemContainer.CanTakeItem())
                    .OrderBy(container => Vector2.Distance(transform.position, container.Transform.position))
                    .ToList();
                if (closestContainersWithWood.Any())
                {
                    aiState.MaybeTarget = closestContainersWithWood.Select(itemContainer => itemContainer.Transform)
                        .FirstOrEmpty();
                    aiState.TargetAction = StrangerState.Gather;
                    aiState.TargetItemType = ItemType.Wood;
                    State = NodeState.Success;
                }
            }

            return State;
        }
    }
}