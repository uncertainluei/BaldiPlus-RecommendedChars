using BaldisBasicsPlusAdvanced.API;
using BaldisBasicsPlusAdvanced.Game.Objects.Plates.KitchenStove;

using BepInEx;

using System;
using System.Collections.Generic;
using System.Linq;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.Advanced
{
    internal static class AdvancedCompatHelper
    {
        internal static void AddStoveRecipe(PluginInfo info, ItemObject[] ingredients, ItemObject[] result)
        {
            new FoodRecipeData(info)
                .SetRawFood(ingredients)
                .SetCookedFood(result)
                .RegisterRecipe();
        }

        internal static void RemoveStoveRecipes(PluginInfo target, Func<ItemObject[], ItemObject[], bool> predicate)
        {
            List<FoodRecipeData> baseRecipes = ApiManager.GetAllKitchenStoveRecipesFrom(target);
            foreach (FoodRecipeData recipe in baseRecipes.Where(x => predicate(x.RawFood, x.CookedFood)))
                ApiManager.RemoveKitchenStoveRecipe(recipe);
        }
    }
}
