using BehaviorTree;
using Inventory;
using LD48;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI.Actions.IsNight
{
    public class GetHandBonfireAction : Node
    {
        [Inject] private IInventory inventory;
        [Inject] private IItemRegistry itemRegistry;

        private readonly StrangerAICalculationState aiState;

        public GetHandBonfireAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;
            
            var bonfireItem = itemRegistry.GetItem(ItemType.Bonfire);

            inventory.IfHandItem(ItemType.Bonfire, _ => { }, () =>
            {
                if (inventory.HasItem(ItemType.Bonfire) || bonfireItem.CanBeCraftedWith(inventory))
                {
                    aiState.TargetItemType = ItemType.Bonfire;
                    aiState.TargetAction = StrangerState.GetHandItem;
                    State = NodeState.Success;    
                }
            });

            return State;
        }
    }
}