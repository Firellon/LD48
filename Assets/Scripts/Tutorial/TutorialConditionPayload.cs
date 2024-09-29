using System;
using Day;
using Inventory;
using Utilities.Monads;

namespace Tutorial
{
    [Serializable]
    public class TutorialConditionPayload
    {
        public int DayNumber { get; set; }
        public DayTime DayTime { get; set; }
        public IItemContainer Inventory { get; set; }
        public double Visibility { get; set; }
        public IMaybe<Item> MaybeHandItem { get; set;  }
    }
}