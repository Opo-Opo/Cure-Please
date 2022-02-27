using System.Collections.Generic;
using EliteMMO.API;

namespace CurePlease.App
{
    public static class Extensions
    {
        public static bool IsInSpellCastingRange(this EliteAPI.XiEntity entity)
        {
            return entity.Distance is < 21 and > 0;
        }

        public static IEnumerable<EliteAPI.InventoryItem> GetContainer(this EliteAPI.InventoryTools tools, Bags bag)
            => tools.GetContainer((int) bag);

        public static IEnumerable<EliteAPI.InventoryItem> GetContainer(this EliteAPI.InventoryTools tools, int containerId)
        {
            var itemIndex = 0;
            while (itemIndex < 80)
            {
                var item = tools.GetContainerItem(containerId, itemIndex++);

                if (item is null) yield break;

                yield return item;
            }
        }
    }
}