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

            // TODO: Check for a weapon instead - something to Rob with
            if (!inventory.HasItem(ItemType.Pistol) && !inventory.IsHandItem(ItemType.Pistol)) return State;

            var closestPeopleToRob = Physics2D.OverlapCircleAll(transform.position, config.RobRadius,
                    config.RobLayerMask)
                .Select(collider => collider.gameObject)
                .Where(other =>
                {
                    if (other.gameObject == transform.gameObject) return false;
                    var otherHuman = other.GetComponent<HumanController>();
                    return otherHuman != null && inventory.DoesHumanHaveWoodILack(config, otherHuman);
                })
                .OrderBy(threat => Vector2.Distance(transform.position, threat.transform.position))
                .ToList();

            if (closestPeopleToRob.Any())
            {
                aiState.MaybeTarget = closestPeopleToRob.First().transform.ToMaybe();
                aiState.TargetAction = StrangerState.Rob;
                aiState.TargetItemType = ItemType.Wood;
                State = NodeState.Success;
            }

            return State;
        }
    }
}