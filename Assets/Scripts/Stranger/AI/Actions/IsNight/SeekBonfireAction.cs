using System.Linq;
using BehaviorTree;
using Environment;
using LD48;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI.Actions
{
    public class SeekBonfireAction : Node
    {
        [Inject] private StrangerAiConfig config;
        [Inject] private Transform transform;

        private readonly StrangerAICalculationState aiState;

        public SeekBonfireAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            var closestBonfires = Physics2D
                .OverlapCircleAll(transform.position, config.BonfireRadius, config.BonfireLayerMask)
                .Select(otherCollider => otherCollider.gameObject.GetComponent<MapBonfire>())
                .Where(bonfire => bonfire != null && bonfire.IsBurning())
                .OrderBy(bonfire => Vector2.Distance(transform.position, bonfire.transform.position))
                .ToList();

            if (closestBonfires.Any())
            {
                aiState.MaybeTarget = closestBonfires.Select(bonfire => bonfire.transform).FirstOrEmpty();
                aiState.TargetAction = StrangerState.SeekBonfire;
                State = NodeState.Success;
            }

            return State;
        }
    }
}