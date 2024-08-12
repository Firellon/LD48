using BehaviorTree;
using LD48;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI
{
    public class FleeAction : Node
    {
        [Inject] private StrangerAiConfig config;

        private readonly StrangerAICalculationState aiState;

        public FleeAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Success;
            aiState.TargetAction = StrangerState.Flee;

            return State;
        }
    }
}