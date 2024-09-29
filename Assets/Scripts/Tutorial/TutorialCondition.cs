using System;
using Day;
using Inventory;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Monads;

namespace Tutorial
{
    [Serializable]
    public class TutorialCondition
    {
        [SerializeField] private TutorialConditionType type = TutorialConditionType.None;
        [SerializeField] private bool inverted = false;

        [ShowIf(nameof(IsItemCondition)), SerializeField] private Item item;
        [ShowIf(nameof(IsDayCondition)), SerializeField] private int dayNumber = 1;
        [ShowIf(nameof(IsDayTimeCondition)), SerializeField] private DayTime dayTime = DayTime.Day;
        [ShowIf(nameof(IsInDarknessCondition)), SerializeField] private float darknessThreshold = 0.85f;
        public object Type => type;

        private bool IsItemCondition() => type is TutorialConditionType.CanCraftItem or TutorialConditionType.HasItem or TutorialConditionType.HoldsItem;
        private bool IsDayCondition() => type == TutorialConditionType.SpecificDay;
        private bool IsDayTimeCondition() => type == TutorialConditionType.SpecificDayTime;
        private bool IsInDarknessCondition() => type == TutorialConditionType.InDarkness;

        public bool IsSatisfied(TutorialConditionPayload payload)
        {
            var isConditionPassing = type switch
            {
                TutorialConditionType.None => true,
                TutorialConditionType.SpecificDay => payload.DayNumber == dayNumber,
                TutorialConditionType.SpecificDayTime => payload.DayTime == dayTime,
                TutorialConditionType.CanCraftItem => item.CanBeCraftedWith(payload.Inventory),
                TutorialConditionType.InDarkness => payload.Visibility < darknessThreshold,
                TutorialConditionType.HasItem => payload.Inventory.HasItem(item),
                TutorialConditionType.HoldsItem => payload.MaybeHandItem.Match(handItem => handItem == item, false),
                _ => throw new ArgumentOutOfRangeException()
            };

            return inverted ? !isConditionPassing : isConditionPassing;
        }
    }
}