using System.Linq;
using BehaviorTree;
using Environment;
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
                .Where(wood => wood != null && wood.Item.ItemType == ItemType.Wood)
                .OrderBy(wood => Vector2.Distance(transform.position, wood.transform.position))
                .ToList();

            if (closestWood.Any())
            {
                aiState.MaybeTarget = closestWood.Select(wood => wood.transform).FirstOrEmpty();
                aiState.TargetAction = StrangerState.Gather;
                State = NodeState.Success;
            }

            return State;
        }
    }
}