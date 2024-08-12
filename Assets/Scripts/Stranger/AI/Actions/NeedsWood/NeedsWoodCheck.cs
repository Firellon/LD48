using BehaviorTree;
using Human;
using Inventory;
using LD48;
using UnityEngine;
using Zenject;

namespace Stranger.AI.Actions
{
    public class NeedsWoodCheck: Node
    {
        [Inject] private StrangerAiConfig config;
        [Inject] private IInventory inventory;
        [Inject] private Transform transform;
        
        private readonly StrangerAICalculationState aiState;
        
        public NeedsWoodCheck(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }
        
        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            if (!inventory.CanAddItem()) return State;

            if (inventory.GetItemAmount(ItemType.Wood) < config.MinWoodToSurvive)
            {
                State = NodeState.Success;
            }

            return State;
        }
    }
}