using BehaviorTree;
using LD48;
using Zenject;

namespace Stranger.AI
{
    public class SurrenderAction : Node
    {
        [Inject] private StrangerAiConfig config;

        private readonly StrangerAICalculationState aiState;

        public SurrenderAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            if (aiState.Threats.Count >= config.Bravery)
            {
                aiState.TargetAction = StrangerState.Surrender;
                State = NodeState.Success;
            }

            return State;
        }
    }
}