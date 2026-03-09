using MTM101BaldAPI.Registers;
using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Util.Extensions;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class CarterItemObject : CaudexItemObject
    {
        private bool useSuffix = true;
        private ItemObject baseItm;

        public void Setup(Items itmEnum, bool useSuffix = true)
        {
            ItemObject itm = ItemMetaStorage.Instance.FindByEnum(itmEnum).value;
            this.useSuffix = useSuffix;
            if (useSuffix)
                nameKey = "Itm_RecChars_CarterItmGeneric";

            baseItm = itm;
            item = itm.item;
            itemSpriteSmall = itm.itemSpriteSmall;
            itemSpriteLarge = itm.itemSpriteLarge;
            audPickupOverride = itm.audPickupOverride;
            descKey = itm.descKey;
        }

        public override string LocalizedName => !useSuffix ? base.LocalizedName :
            string.Format("Itm_RecChars_CarterItmFormat".Localize(), baseItm.GetName(true));
        public override string LocalizedDesc => baseItm.GetDescription(true);
    }
}