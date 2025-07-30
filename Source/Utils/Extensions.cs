using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI.AssetTools;
using System;
using System.IO;
using System.Reflection;

using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    partial class RecommendedCharsPlugin
    {
        internal static void PatchCompat(Type type, string guid)
        {
            if (Chainloader.PluginInfos.ContainsKey(guid))
                Hooks.PatchAll(type);
        }

        internal static X SwapComponentSimple<T, X>(T original) where T : MonoBehaviour where X : T
        {
            X val = original.gameObject.AddComponent<X>();

            FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields)
                fieldInfo.SetValue(val, fieldInfo.GetValue(original));

            GameObject.DestroyImmediate(original);
            return val;
        }

        internal static void AddAudioClipsToAssetMan(string path, string prefix)
        {
            string[] files = Directory.GetFiles(path, "*.wav");
            for (int i = 0; i < files.Length; i++)
            {
                AudioClip aud = AssetLoader.AudioClipFromFile(files[i], AudioType.WAV);
                AssetMan.Add(prefix + aud.name, aud);
                aud.name = "RecChars_" + aud.name;
            }
        }
    }
}
