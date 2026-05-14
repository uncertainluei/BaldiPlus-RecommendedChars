using System;
using System.Collections.Generic;
using System.IO;

using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;

using PlusStudioLevelFormat;
using PlusStudioLevelLoader;

using UncertainLuei.CaudexLib.Objects;
using UncertainLuei.CaudexLib.Util.Extensions;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    internal static class ObjectCreation
    {
        internal static PosterObject CreatePoster(Texture2D tex, string name, string editorAlias, params PosterTextData[] posterTextData)
        {
            PosterObject poster = ObjectCreators.CreatePosterObject(tex, posterTextData);
            poster.name = name;
            RecommendedCharsPlugin.ObjMan.Add("Pst/"+name, poster);
            LevelLoaderPlugin.Instance.posterAliases.Add("recchars_"+editorAlias, poster);
            return poster;
        }

        internal static PosterObject CreatePoster(Texture2D tex, string name, params PosterTextData[] posterTextData)
            => CreatePoster(tex, name, name.ToLower(), posterTextData);
        internal static PosterObject CreatePoster(string path, string name, string editorAlias, params PosterTextData[] posterTextData)
            => CreatePoster(RecommendedCharsPlugin.AssetMan.Get<Texture2D>(path), name, editorAlias, posterTextData);
        internal static PosterObject CreatePoster(string path, string name, params PosterTextData[] posterTextData)
            => CreatePoster(path, name, name.ToLower(), posterTextData);

        internal static SpriteRenderer CreateSpriteBillboard(Sprite sprite, Vector3 position = default, Transform parent = null, string name = "Sprite")
        {
            GameObject spriteObject = new(name, typeof(SpriteRenderer));
            spriteObject.ConvertToPrefab(true);
            spriteObject.layer = LayerMask.NameToLayer("Billboard");

            if (parent)
                spriteObject.transform.parent = parent;
            spriteObject.transform.localPosition = position;

            SpriteRenderer spriteImage = spriteObject.GetComponent<SpriteRenderer>();
            spriteImage.sprite = sprite;
            spriteImage.material = RecommendedCharsPlugin.AssetMan.Get<Material>("Mat/SpriteBillboard");    
            return spriteImage;
        }

        internal static void AddTexturesToAssetMan(string prefix, string[] paths)
            => RecommendedCharsPlugin.AssetMan.AddRange(AssetLoader.TexturesFromMod(RecommendedCharsPlugin.Plugin, "*.png", paths), x => prefix+x.name);

        internal static void AddAudioToAssetMan(string prefix, string[] paths)
        {
            string[] files = Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(RecommendedCharsPlugin.Plugin), Path.Combine(paths)), "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip aud = AssetLoader.AudioClipFromFile(files[i], AudioType.WAV);
                RecommendedCharsPlugin.AssetMan.Add(prefix + aud.name, aud);
                aud.name = "RecChars_" + aud.name;
            }
        }

        internal static WeightedRoomAsset[] RoomAssetsFromDirectory(string dir, params int[] weights)
            => RoomAssetsFromDirectory(null, dir, weights);

        internal static WeightedRoomAsset[] RoomAssetsFromDirectory(string dir)
            => RoomAssetsFromDirectory(dir, 100);

        internal static WeightedRoomAsset[] RoomAssetsFromDirectory(CaudexRoomBlueprint blueprint, string dir)
            => RoomAssetsFromDirectory(blueprint, dir, 100);

        internal static WeightedRoomAsset[] RoomAssetsFromDirectory(CaudexRoomBlueprint blueprint, string dir, params int[] weights)
        {
            if (weights == null || weights.Length == 0)
                weights = [100];

            int idx = 0;
            List<WeightedRoomAsset> rooms = [];

            foreach (string file in Directory.GetFiles(Path.Combine(AssetLoader.GetModPath(RecommendedCharsPlugin.Plugin), "Layouts", dir), "*.rbpl"))
            {
                BinaryReader reader = new(File.OpenRead(file));
                BaldiRoomAsset formatAsset = BaldiRoomAsset.Read(reader);
                reader.Close();

                ExtendedRoomAsset asset = LevelImporter.CreateRoomAsset(formatAsset);
                if (blueprint != null)
                {
                    asset.roomFunctionContainer = blueprint.functionContainer;
                    asset.lightPre = blueprint.lightObj;
                    asset.windowObject = blueprint.windowSet;
                    asset.windowChance = blueprint.windowChance;
                    asset.posters = blueprint.posters;
                    asset.posterChance = blueprint.posterChance;
                    asset.mapMaterial = blueprint.mapMaterial;
                    asset.basicSwaps = blueprint.objectSwaps;
                    asset.name = blueprint.name+"_"+Path.GetFileNameWithoutExtension(file);
                }
                else
                    asset.name = asset.category.ToStringExtended()+"_"+Path.GetFileNameWithoutExtension(file);

                ((ScriptableObject)asset).name = asset.type.ToString()+"_"+asset.name;
                rooms.Add(asset.Weighted(weights[Math.Min(idx,weights.Length-1)]));
                idx++;
            }
            return rooms.ToArray();
        }

        internal static X SwapComponentSimple<T, X>(this T original) where T : MonoBehaviour where X : T
            => original.gameObject.SwapComponent<T, X>(original, false);
    }
}