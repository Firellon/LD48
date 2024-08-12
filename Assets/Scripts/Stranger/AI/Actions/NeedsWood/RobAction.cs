using System.Linq;
using BehaviorTree;
using Human;
using Inventory;
using LD48;
using UnityEngine;
using Utilities.Monads;
using Zenject;

namespace Stranger.AI.Actions
{
    public class RobAction : Node
    {
        [Inject] private IInventory inventory;
        [Inject] private StrangerAiConfig config;
        [Inject] private Transform transform;
        
        private readonly StrangerAICalculationState aiState;
        
        public RobAction(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }

        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            // TODO: Potentially replace with a RobLayerMask, separete from the ThreatLayerMask
            var closestPeopleToRob = Physics2D.OverlapCircleAll(transform.position, config.ThreatRadius,
                    config.ThreatLayerMask)
                .Select(collider => collider.gameObject)
                .Where(other =>
                {
                    if (other.gameObject == transform.gameObject) return false;
                    var otherHuman = other.GetComponent<HumanController>();
                    return otherHuman != null && DoesHumanHaveWoodILack(otherHuman);
                })
                .OrderBy(threat => Vector2.Distance(transform.position, threat.transform.position))
                .ToList();

            if (closestPeopleToRob.Any())
            {
                aiState.MaybeTarget = closestPeopleToRob.First().transform.ToMaybe();
                aiState.TargetAction = StrangerState.Rob;
                State = NodeState.Success;
            }

            return State;
        }

        private bool DoesHumanHaveWoodILack(HumanController otherHumanController)
        {
            var currentWoodAmount = inventory.GetItemAmount(ItemType.Wood);
            return currentWoodAmount < config.MinWoodToSurvive &&
                   otherHumanController.Inventory.GetItemAmount(ItemType.Wood) > currentWoodAmount + 2;
        }
    }
}