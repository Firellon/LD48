using System.Linq;
using BehaviorTree;
using LD48;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI
{
    public class AttackAction : Node
    {
        [Inject] private StrangerAiConfig config;

        private readonly StrangerAICalculationState aiState;

        public AttackAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            // TODO: Check for something shootable instead
            var hasPistol = aiState.Inventory.HandItem.Match(handItem => handItem.ItemType == ItemType.Pistol, false) ||
                            aiState.Inventory.HasItem(ItemType.Pistol);
            var threatsCount = aiState.Threats.Count;
            
            if (threatsCount > 0 && threatsCount <= config.Bravery && hasPistol)
            {
                aiState.MaybeTarget = aiState.Threats.Select(threat => threat.transform).FirstOrEmpty();
                aiState.TargetAction = StrangerState.Fight;
                State = NodeState.Success;
            }

            return State;
        }
    }
}