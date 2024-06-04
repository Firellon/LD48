using Utilities.Monads;

namespace Inventory.Signals
{
    public class PlayerHandItemUpdatedEvent
    {
        public IMaybe<Item> MaybeItem { get; }
        public PlayerHandItemUpdatedEvent(IMaybe<Item> maybeItem)
        {
            MaybeItem = maybeItem;
        }
    }
}