using UnityEngine;
using UncertainLuei.CaudexLib.Components;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public abstract class FoodFightEffect_AbstractMoveMod : ReusableEffect
    {
        protected abstract MovementModifier MoveMod { get; }

        protected override void Activated()
        {

            if (!Entity.ExternalActivity.moveMods.Contains(MoveMod))
                Entity.ExternalActivity.moveMods.Add(MoveMod);
        }

        protected override void Deactivated()
        {
            if (Entity.ExternalActivity.moveMods.Contains(MoveMod))
                Entity.ExternalActivity.moveMods.Remove(MoveMod);
        }

        protected override void ActiveUpdate()
        {}

        protected override void Reactivated()
        {}
    }

    public class FoodFightEffect_Apple : FoodFightEffect_AbstractMoveMod
    {
        private readonly MovementModifier moveMod = new(default, 0);
        protected override MovementModifier MoveMod => moveMod;
    }

    public class FoodFightEffect_Hotdog : FoodFightEffect_AbstractMoveMod
    {
        private readonly MovementModifier moveMod = new(default, -1);
        protected override MovementModifier MoveMod => moveMod;
    }

    public class FoodFightEffect_Beans : FoodFightEffect_AbstractMoveMod
    {
        private readonly MovementModifier moveMod = new(default, 0.1f), playerMoveMod = new(default, 0.25f);
        protected override MovementModifier MoveMod => EntType == EntityType.Player ? playerMoveMod : moveMod;
    }
}