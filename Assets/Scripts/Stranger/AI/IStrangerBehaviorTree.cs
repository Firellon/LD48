using BehaviorTree.Common;

namespace Stranger.AI
{
    public interface IStrangerBehaviorTree : IBehaviorTree
    {
        public StrangerAICalculationState State { get; }
    }
}