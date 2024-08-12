using BehaviorTree;
using LD48;
using Zenject;

namespace Stranger.AI
{
    public class WanderAction : Node
    {
        [Inject] private StrangerAiConfig config;

        private readonly StrangerAICalculationState aiState;

        public WanderAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            aiState.TargetAction = StrangerState.Wander;
            State = NodeState.Success;

            return State;
        }
    }
}