using MTM101BaldAPI.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public static class DaycareDoorAssets
    {
        internal static SoundObject open;
        internal static SoundObject shut;
        internal static SoundObject unlock;

        internal static StandardDoorMats template;
        internal static StandardDoorMats locked;

        internal static Material mask;
        private static readonly Dictionary<int, StandardDoorMats> materials = new Dictionary<int, StandardDoorMats>();

        private static PosterObject dummyPoster;

        public static StandardDoorMats GetMaterial(int num)
        {
            if (materials.ContainsKey(num))
                return materials[num];

            if (dummyPoster == null)
            {
                dummyPoster = PosterObject.CreateInstance<PosterObject>();
                dummyPoster.name = "MrDaycare_DummyPoster";
                dummyPoster.textData = new PosterTextData[]
                {
                    new PosterTextData()
                    {
                        alignment = TextAlignmentOptions.Center,
                        color = Color.white,
                        fontSize = 72,
                        font = BaldiFonts.ComicSans36.FontAsset(),
                        position = new IntVector2(32, 64)
                    }
                };
            }

            StandardDoorMats material = GameObject.Instantiate(template);
            material.name = template.name + "_" + num;

            dummyPoster.textData[0].textKey = num.ToString();

            material.shut = new Material(template.shut);
            material.shut.name = template.shut.name + "_" + num;

            dummyPoster.baseTexture = (Texture2D)template.shut.mainTexture;
            material.shut.mainTexture = BaseGameManager.Instance.ec.TextTextureGenerator.GenerateTextTexture(dummyPoster);
            material.shut.mainTexture.name = template.shut.name + "_" + num;

            materials.Add(num, material);
            return material;
        }
    }
}
