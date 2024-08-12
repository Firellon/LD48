using System.Linq;
using BehaviorTree;
using Human;
using LD48;
using UnityEngine;
using Zenject;

namespace Stranger.AI
{
    public class IsThreatenedCheck : Node
    {
        [Inject] private StrangerAiConfig config; 
        
        private readonly StrangerAICalculationState aiState;
        
        public IsThreatenedCheck(StrangerAICalculationState aiState)
        {
            this.aiState = aiState;
        }
        
        public override NodeState Evaluate()
        {
            State = NodeState.Failure;

            var strangerTransform = aiState.Transform;
            var strangerPosition = strangerTransform.position;
            
            var closestThreats = Physics2D
                .OverlapCircleAll(strangerPosition, config.ThreatRadius, config.ThreatLayerMask)
                .Select(collider => collider.gameObject)
                .Where(threat =>
                {
                    if (threat.gameObject == strangerTransform.gameObject) return false;
                    var hittable = threat.GetComponent<IHittable>();
                    if (hittable == null) return false;
                    var otherHuman = threat.GetComponent<HumanController>();
                    if (otherHuman == null && hittable.IsThreat()) return true;
                    if (otherHuman == null) return false;
                    var humanVerticalDistance = Mathf.Abs(otherHuman.transform.position.y - strangerTransform.position.y);
                    var isHumanCloseAndPointingAtMe = otherHuman.IsThreat() &&
                                                      otherHuman.IsFacingTowards(strangerPosition) &&
                                                      humanVerticalDistance < config.ThreatRadius / 3;

                    return isHumanCloseAndPointingAtMe;
                })
                .OrderBy(threat => Vector2.Distance(strangerTransform.position, threat.transform.position))
                .ToList();
            
            if (closestThreats.Any())
            {
                aiState.Threats = closestThreats;
                State = NodeState.Success;
            }

            return State;
        }
    }
}