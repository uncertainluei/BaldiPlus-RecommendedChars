using System.Collections.Generic;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class ITM_LunchBox : Item
    {
        private static readonly Dictionary<int, ITM_LunchBox> instances = [];

        internal static Items itemEnum = (Items)(-1);
        internal static Items randomDummyEnum = (Items)(-1);
        internal static WeightedItemObject[] weightedItems; 

        public WeightedItemObject[] possibleFoodItems;

        private ItemObject lastItem;
        private float duplicateChance = 0f;

        public override bool Use(PlayerManager pm)
        {
            if (!instances.ContainsKey(pm.playerNumber))
                instances.Add(pm.playerNumber, this);
            else if (instances[pm.playerNumber] == null)
                instances[pm.playerNumber] = this;
            else if (instances[pm.playerNumber] != this)
            {
                Destroy(gameObject);
                return instances[pm.playerNumber].Use(pm);
            }
            if (pm.itm.InventoryFull()) return false;
            if (Random.value >= duplicateChance)
            {
                lastItem = WeightedItemObject.RandomSelection(possibleFoodItems);
                duplicateChance = Mathf.Clamp(11f/lastItem.value, 0.02f, 0.65f);
            }
            pm.itm.AddItem(lastItem);
            return true;
        }
    }
}
