using PlusStudioLevelLoader;
using UncertainLuei.CaudexLib.Objects;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.LevelLoader
{
    internal static class LevelLoaderCompatHelper
    {
        internal static void AddRoom(RoomBlueprint blueprint) => AddRoom(blueprint, "recchars_" + blueprint.name.ToLower());
        internal static void AddRoom(RoomBlueprint blueprint, string id)
        {
            RoomSettings settings = new(blueprint.category, blueprint.type, blueprint.color, blueprint.doorMats, blueprint.mapMaterial);
            settings.container = blueprint.functionContainer;
            LevelLoaderPlugin.Instance.roomSettings.Add(id, settings);
        }
    }
}
