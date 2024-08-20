using Human;
using Inventory;
using LD48;

namespace Stranger.AI
{
    public static class AiExtensions
    {
        public static bool DoesHumanHaveWoodILack(this IItemContainer myInventory, StrangerAiConfig config, HumanController otherHumanController)
        {
            var currentWoodAmount = myInventory.GetItemAmount(ItemType.Wood);
            return currentWoodAmount < config.MinWoodToSurvive &&
                   otherHumanController.Inventory.HasItem(ItemType.Wood);
        }
    }
}