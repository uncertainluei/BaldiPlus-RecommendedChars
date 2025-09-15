using brobowindowsmod;
using brobowindowsmod.ItemScripts;
using brobowindowsmod.NPCs;
using brobowindowsmod.Patches;

using System;
using System.Collections.Generic;

using HarmonyLib;
using MTM101BaldAPI;
using UnityEngine;

namespace UncertainLuei.BaldiPlus.RecommendedChars.Compat.FragileWindows
{
    internal static class FragileWindowsCompatHelper
    {
        internal static readonly Dictionary<string, Type> windowletFunctions = [];
        internal static void AddWindowlet<T>(string type, Sprite sprNormal, Sprite sprScream, Color particleColor, int shrdNo) where T : ExtWindowletVariant
        {
            LittleWindowGuy.windowVariants.Add(new(type, sprNormal, sprScream, particleColor, shrdNo));
            windowletFunctions.Add(type, typeof(T));
        }
    }

    [HarmonyPatch]
    internal static class FragileMiscPatches
    {
        // Window mask fix
        private static WindowObject _windowSet;
        [HarmonyPatch(typeof(RandomizeOnTimeScale), "CheckUpdate"), HarmonyPrefix]
        private static void GetWindow(Window _____instance) // Please refer to Big Thinker on why the window field is named like this
        {
            _windowSet = _____instance.windowObject;
        }
        [HarmonyPatch(typeof(RandomizeOnTimeScale), "CheckUpdate"), HarmonyPostfix]
        private static void FixWindowSetInconsistency(Window _____instance)
        {
            if (_windowSet != _____instance.windowObject)
            {
                _windowPatch = true;
                _____instance.UpdateTextures();
            }
        }
        private static bool _windowPatch;
        [HarmonyPatch(typeof(WindowPatch), "TexturePatch"), HarmonyPrefix]
        private static bool FixWindowPatch()
        {
            if (_windowPatch)
            {
                _windowPatch = false;
                return false;
            }
            return true;
        }

        // Fix Cup of Windows not applying forces to spawned windows
        [HarmonyPatch(typeof(ITM_CupOfWindow), "dump"), HarmonyPrefix]
        private static void AssignPmToCup(PlayerManager ___pmium, ref PlayerManager ___pm)
        {
            ___pm = ___pmium;
        }
    }

    [HarmonyPatch(typeof(LittleWindowGuy))]
    internal static class WindowletVariantPatches
    {
        private static readonly Dictionary<LittleWindowGuy, ExtWindowletVariant> variants = [];

        // Grab custom windowling function
        private static LittleWindowGuy _lastNpc;
        private static ExtWindowletVariant _lastVariant;
        private static ExtWindowletVariant GetVariantFunction(LittleWindowGuy npc)
        {
            if (_lastNpc == npc)
                return _lastVariant;

            _lastNpc = npc;
            if (!variants.ContainsKey(npc))
                variants.Add(npc, npc.GetComponent<ExtWindowletVariant>());
            _lastVariant = variants[npc];
            return _lastVariant;
        }

        [HarmonyPatch("Initialize"), HarmonyPostfix]
        private static void AddCacheFunction(LittleWindowGuy __instance)
        {
            if (__instance.GetComponent<WindowletCache>() != null) return;

            __instance.gameObject.AddComponent<WindowletCache>();
            if (FragileWindowsCompatHelper.windowletFunctions.ContainsKey(__instance.me.type))
            {
                _lastNpc = __instance;
                _lastVariant = (ExtWindowletVariant)__instance.gameObject.AddComponent(FragileWindowsCompatHelper.windowletFunctions[__instance.me.type]);
                _lastVariant.Initialize(__instance);
            }
        }

        [HarmonyPatch("LateUpdate"), HarmonyPrefix]
        private static bool LateUpdate(LittleWindowGuy __instance)
        {
            // Do not run LateUpdate before the hotspot's created
            if (__instance.hotspot == null) return false;

            ExtWindowletVariant var = GetVariantFunction(__instance);
            if (__instance.pickedUp)
                var?.HeldUpdate();
            else if (__instance.flying)
                var?.FlyingUpdate();
            else
                var?.WanderUpdate();

            return true;
        }

        [HarmonyPatch("Click"), HarmonyPrefix]
        private static void Click(LittleWindowGuy __instance, PlayerManager pem)
        {
            ExtWindowletVariant var = GetVariantFunction(__instance);
            if (!__instance.pickedUp)
                var?.Pickup(pem);
            else
            {
                DaycareGuiltManager.TryBreakRule(pem, "Throw", 1.6f, 0.25f);
                var?.Throw(pem);
            }
        }

        [HarmonyPatch("Boom"), HarmonyPrefix]
        private static bool Boom(LittleWindowGuy __instance)
        {
            ExtWindowletVariant var = GetVariantFunction(__instance);
            return var == null || var.Shatter();
        }

        private sealed class WindowletCache : MonoBehaviour
        {
            private LittleWindowGuy windowlet;

            private void Awake()
            {
                windowlet = GetComponent<LittleWindowGuy>();
            }

            private void OnDestroy()
            {
                // Remove current windowling from the dictionary once destroyed
                if (variants.ContainsKey(windowlet))
                    variants.Remove(windowlet);
            }
        }
    }

    internal abstract class ExtWindowletVariant : MonoBehaviour
    {
        protected LittleWindowGuy Windowlet { get; private set; }

        public void Initialize(LittleWindowGuy npc)
        {
            Windowlet = npc;
            Initialized();
        }

        protected virtual void Initialized()
        {}
        public virtual void FlyingUpdate()
        {}
        public virtual void HeldUpdate()
        {}
        public virtual void WanderUpdate()
        {}
        public virtual void Pickup(PlayerManager player)
        {}
        public virtual void Throw(PlayerManager player)
        {}
        public virtual bool Shatter() => true;
    }
}
