using System.Collections.Generic;
using Inventory;
using LD48;
using UnityEngine;
using Utilities.Monads;

namespace Stranger.AI
{
    public class StrangerAICalculationState
    {
        public Transform Transform { get; set; }
        public IInventory Inventory { get; set; }
        public List<GameObject> Threats { get; set; } = new();
        public StrangerState TargetAction { get; set; }
        public IMaybe<Transform> MaybeTarget { get; set; } = Maybe.Empty<Transform>();
        public ItemType TargetItemType { get; set; } = ItemType.None;
    }
}