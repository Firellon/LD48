using BehaviorTree;
using Day;
using Zenject;

namespace Stranger.AI
{
    public class IsNightCheck : Node
    {
        [Inject] private IDayNightCycle dayNightCycle;

        private readonly StrangerAICalculationState aiState;

        public IsNightCheck(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            var currentCycle = dayNightCycle.GetCurrentCycle();
            if (currentCycle is DayTime.NightComing or DayTime.Night)
            {
                return NodeState.Success;
            }

            return NodeState.Failure;
        }
    }
}