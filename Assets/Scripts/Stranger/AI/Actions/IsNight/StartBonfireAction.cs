using BehaviorTree;
using Inventory;
using LD48;
using Zenject;

namespace Stranger.AI.Actions
{
    public class StartBonfireAction : Node
    {
        [Inject] private IInventory inventory;

        private readonly StrangerAICalculationState aiState;

        public StartBonfireAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            inventory.IfHandItem(ItemType.Bonfire, _ =>
            {
                aiState.TargetAction = StrangerState.StartBonfire;
                State = NodeState.Success;
            });

            return State;
        }
    }
}