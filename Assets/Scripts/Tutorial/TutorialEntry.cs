using System.Collections.Generic;
using System.Linq;
using Dialogue.Entry;
using UnityEngine;

namespace Tutorial
{
    [CreateAssetMenu(menuName = "LD48/Create Tutorial Entry SO", fileName = "New Tutorial Entry", order = 1)]
    public class TutorialEntry : ScriptableObject
    {
        [SerializeField] private DialogueEntry dialogue;
        [SerializeField] private List<TutorialCondition> conditions = new();

        public DialogueEntry DialogueEntry => dialogue;

        public bool IsReadyToShow(TutorialConditionPayload conditionPayload)
        {
            var unsatisfiedConditions = conditions.Where(condition => !condition.IsSatisfied(conditionPayload)).ToList();
            if (unsatisfiedConditions.Any())
            {
                Debug.Log($"IsReadyToShow({name}) > false, unsatisfied conditions: [{string.Join(",", unsatisfiedConditions.Select(c => c.Type))}]");
                return false;
            }

            return true;
        }
    }
}