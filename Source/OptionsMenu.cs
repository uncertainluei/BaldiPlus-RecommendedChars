using BepInEx;
using BepInEx.Configuration;

using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.OptionsAPI;
using MTM101BaldAPI.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using TMPro;

using UncertainLuei.CaudexLib.Util.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UncertainLuei.BaldiPlus.RecommendedChars
{
    public class RecommendedCharsOptionsMenu : CustomOptionsCategory
    {
        private List<OptionsSubpage> pages = [];
        private byte currentPage = 0;
        private TextMeshProUGUI pageTitle;

        private GameObject confirmMenu, blackCover;
        private TMP_Text confirmMenuText;
        private StandardMenuButton confirmBtnYes, confirmBtnNo;

        public override void Build()
        {
            pageTitle = CreateText("PageTitle", "Page", new Vector3(0, 56), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(224, 28), Color.black, false);
            CreateButton(() => ChangePage(-1), menuArrowLeft, menuArrowLeftHighlight, "PrevPage", new(-112, 56), null);
            CreateButton(() => ChangePage(1), menuArrowRight, menuArrowRightHighlight, "NextPage", new(112, 56), null);

            BuildSubpage<OptionsSubpage_Modules>("Modules");
            BuildSubpage<OptionsSubpage_Behaviors>("Behaviors");
            BuildSubpage<OptionsSubpage_Misc>("Misc");

            Transform confirmMenuTransform = Instantiate(transform.parent.Find("Data/Confirm"), transform);
            DestroyImmediate(confirmMenuTransform.Find("FileName").gameObject);
            confirmMenu = confirmMenuTransform.gameObject;
            confirmMenu.name = "Confirm";
            confirmMenu.SetActive(false);

            confirmMenuText = confirmMenu.GetComponentInChildren<TMP_Text>();
            StandardMenuButton[] buttons = confirmMenu.GetComponentsInChildren<StandardMenuButton>();
            confirmBtnYes = buttons[0];
            confirmBtnNo = buttons[1];

            blackCover = Instantiate(Resources.FindObjectsOfTypeAll<MainMenu>().First(x => x.gameObject.scene == this.gameObject.scene).blackCover, transform);

            UpdatePageTitle();
        }

        // Failsafe if player picks back button on confirm menu
        private void OnDisable()
        {
            confirmMenu.SetActive(false);
            pages[currentPage].gameObject.SetActive(true);
        }

        private void DisplayConfirmMenu(string textKey, OptionsSubpage currentMenu, Action yesAction = null, bool returnWhenYes = true)
        {
            confirmMenuText.text = textKey.Localize();

            currentMenu.gameObject.SetActive(false);
            confirmMenu.SetActive(true);

            confirmBtnYes.OnPress = new();
            if (yesAction != null)
                confirmBtnYes.OnPress.AddListener(() => yesAction());
            if (returnWhenYes)
                confirmBtnYes.OnPress.AddListener(() =>
                {
                    currentMenu.gameObject.SetActive(true);
                    confirmMenu.SetActive(false);
                });

            confirmBtnNo.OnPress = new();
            confirmBtnNo.OnPress.AddListener(() =>
            {
                currentMenu.gameObject.SetActive(true);
                confirmMenu.SetActive(false);
            });
        }

        private void DisplayRestartPrompt(OptionsSubpage currentMenu)
            => DisplayConfirmMenu("Opt_RecChars_RestartRequired", currentMenu, () => StartCoroutine(CloseGame()), false);

        private IEnumerator CloseGame()
        {
            CursorController.Instance.Hide(true);
            GlobalCam.Instance.Transition(UiTransition.Dither, 0.01666667f);
            blackCover.SetActive(true);
            yield return new WaitWhile(() => GlobalCam.Instance.TransitionActive);
            Application.Quit();
        }

        private void ChangePage(int direction)
        {
            pages[currentPage].gameObject.SetActive(false);
            currentPage = (byte)((currentPage+direction+pages.Count) % pages.Count);
            pages[currentPage].gameObject.SetActive(true);
            UpdatePageTitle();
        }

        private void UpdatePageTitle()
        {
            string pageName = pages[currentPage].name;
            pageTitle.text = ("Opt_RecChars_"+pageName).Localize(pageName);
        }

        private T BuildSubpage<T>(string name) where T : OptionsSubpage
        {
            T subpage = new GameObject(name, typeof(RectTransform), typeof(T)).GetComponent<T>();
            subpage.transform.SetParent(transform, false);
            subpage.Build(this);

            if (pages.Count > 0)
                subpage.gameObject.SetActive(false);
            pages.Add(subpage);
            return subpage;
        }

        private class OptionsSubpage : MonoBehaviour
        {
            protected readonly Dictionary<ConfigEntry<bool>, MenuToggle> configTogglePairs = [];
            protected MenuToggle CreateToggle(string name, Vector2 pos, float width, ConfigEntry<bool> configEntry)
            {
                MenuToggle toggle = cat.CreateToggle(name, $"Opt_RecChars_{this.name}_{name}", configEntry.Value, pos, width-44f);
                toggle.transform.SetParent(transform, false);

                RectTransform rect = (RectTransform)toggle.hotspot.transform;
                rect.pivot = new(1f, 0.5f);
                rect.anchoredPosition = new(44f, 0f);
                rect.sizeDelta = new(width, 32f);

                cat.AddTooltip(toggle, $"Tip_RecChars_{this.name}_{name}");
                configTogglePairs[configEntry] = toggle;
                return toggle;
            }

            protected StandardMenuButton CreateApplyButton()
            {
                StandardMenuButton apply = cat.CreateApplyButton(() => Apply());
                apply.transform.SetParent(transform, false);
                cat.AddTooltip(apply, "Tip_RecChars_Apply"+name);
                return apply;
            }

            protected RecommendedCharsOptionsMenu cat;

            public void Build(RecommendedCharsOptionsMenu cat)
            {
                this.cat = cat;
                Build();
            }

            protected virtual void Build() {}
            protected virtual void Apply() { }
        }

        private class OptionsSubpage_Modules : OptionsSubpage
        {
            private static Texture2D moduleIconSheet;
            private static Sprite[] moduleBackgrounds;
            private static readonly Color darkGray = new(0.25f, 0.25f, 0.25f);

            private Image[] moduleToggleImages;
            private bool[] currentModuleToggles;

            protected override void Build()
            {
                int count = RecCharsModule.allModulesInConfig.Count;
                moduleToggleImages = new Image[count];
                currentModuleToggles = new bool[count];

                if (!moduleIconSheet)
                {
                    moduleIconSheet = ObjectCreation.AddTextureToAssetManWLegacy("Ui/ModuleIcons", ["Textures", "Gui", "ModuleIcons.png"]);
                    moduleBackgrounds = [
                        AssetLoader.SpriteFromMod(RecommendedCharsPlugin.Plugin, Vector2.one/2, 1f, "Textures", "Gui", "ModuleSelectBackground.png"),
                        AssetLoader.SpriteFromMod(RecommendedCharsPlugin.Plugin, Vector2.one/2, 1f, "Textures", "Gui", "ModuleBorder.png"),
                    ];
                }

                Sprite[] moduleIcons = AssetLoader.SpritesFromSpriteSheetCount(moduleIconSheet, 32, 32, 1, 20);

                cat.CreateText("InfoTxt", "Opt_RecChars_Modules_Instructions".Localize(), new Vector3(0, 20), BaldiFonts.ComicSans18, TextAlignmentOptions.Bottom, new Vector2(360, 18), Color.black, false)
                    .transform.SetParent(transform, false);

                Vector2 pos = new(-140f, -10f);
                byte i = 0;
                ConfigDefinition definition;
                string currentSection = "";

                foreach (RecCharsModule module in RecCharsModule.allModulesInConfig.OrderBy((x) => x.IconId))
                {
                    definition = module.Info.ConfigEntry.Definition;
                    currentModuleToggles[i] = module.Info.ConfigEntry.Value;

                    if (pos.x > 140f || (!currentSection.IsNullOrWhiteSpace() && definition.Section != currentSection))
                    {
                        pos.x = -140f;
                        pos.y -= 40f;
                    }
                    currentSection = definition.Section;

                    byte idx2 = i;
                    StandardMenuButton btn = cat.CreateButton(() => Toggle(idx2), moduleBackgrounds[0], $"{definition.Section.Replace('.','_')}_{definition.Key}", pos);
                    btn.transform.SetParent(transform, false);
                    btn.image.color = Color.clear;
                    btn.OnHighlight.AddListener(() => btn.image.color = Color.white);
                    btn.OffHighlight.AddListener(() => btn.image.color = Color.clear);
                    cat.AddTooltip(btn, "Tip_RecChars_"+btn.name);
                    moduleToggleImages[i] = cat.CreateImage(moduleIcons[module.IconId], "Module_Icon", Vector3.zero);
                    moduleToggleImages[i].raycastTarget = false;
                    moduleToggleImages[i].transform.SetParent(btn.transform, false);
                    Image border = cat.CreateImage(moduleBackgrounds[1], "Module_Border", Vector3.zero);
                    border.raycastTarget = false;
                    border.transform.SetParent(btn.transform, false);

                    UpdateHighlight(i);

                    i++;
                    pos.x += 40f;
                }

                CreateApplyButton();
            }

            private void Toggle(byte index)
            {
                currentModuleToggles[index] = !currentModuleToggles[index];
                UpdateHighlight(index);
            }

            private void UpdateHighlight(byte index)
            {
                moduleToggleImages[index].color = currentModuleToggles[index] ? Color.white : darkGray;
            }

            protected override void Apply()
            {
                bool changesFound = false;
                byte i = 0;
                foreach (RecCharsModule module in RecCharsModule.allModulesInConfig.OrderBy((x) => x.IconId))
                {
                    if (currentModuleToggles[i] != module.Info.ConfigEntry.Value)
                    {
                        changesFound = true;
                        module.Info.ConfigEntry.Value = currentModuleToggles[i];
                    }
                    i++;
                }
                if (!changesFound) return;
                RecommendedCharsPlugin.Plugin.Config.Save();
                cat.DisplayRestartPrompt(this);
            }
        }

        private class OptionsSubpage_Behaviors : OptionsSubpage
        {
            protected override void Build()
            {
                CreateToggle("OnlyOneNpcActivity", new(120f, 16f), 270f, RecommendedCharsConfig.onlyOneNpcActivity);
                CreateToggle("NerfCircle", new(120f, -20f), 180f, RecommendedCharsConfig.nerfCircle);
                CreateToggle("NerfMrDaycare", new(120f, -56f), 250f, RecommendedCharsConfig.nerfMrDaycare);
                CreateToggle("IntendedWires", new(120f, -92f), 320f, RecommendedCharsConfig.intendedWires);
                //CreateToggle("IntendedGifter", new(120f, -120f), 235f, RecommendedCharsConfig.intendedGifter);
            }

            // Apply changes when exiting the menu
            private void OnDisable()
            {
                bool changesFound = false;
                foreach (var config in configTogglePairs.Keys)
                {
                    if (config.Value != configTogglePairs[config].Value)
                    {
                        changesFound = true;
                        config.Value = configTogglePairs[config].Value;
                    }
                }
                if (!changesFound) return;
                RecommendedCharsPlugin.Plugin.Config.Save();
            }
        }

        private class OptionsSubpage_Misc : OptionsSubpage
        {
            private TMP_Text partyModeText;
            private RecommendedCharsConfig.PartyModeConfigMode partyModeSetting;

            protected override void Build()
            {
                partyModeSetting = RecommendedCharsConfig.partyMode.Value;
                string tooltip = "Tip_RecChars_Misc_PartyMode";

                Transform partyParent = cat.CreateText("PartyMode", "Opt_RecChars_Misc_PartyMode".Localize(), new Vector3(0, 12), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(224, 24), Color.black, false).transform;
                partyParent.SetParent(transform, false);
                partyModeText = cat.CreateText("Setting", "", new Vector3(0, -32, 0), BaldiFonts.ComicSans24, TextAlignmentOptions.Center, new Vector2(180, 24), Color.black, false);
                partyModeText.transform.SetParent(partyParent, false);
                ChangePartyMode(0);

                StandardMenuButton button = cat.AddTooltipRegion("Hotspot", new(0f, -20f), new(224f, 72f), tooltip);
                button.transform.SetParent(partyParent, false);
                cat.AddTooltip(button, tooltip);

                button = cat.CreateButton(() => ChangePartyMode(-1), cat.menuArrowLeft, cat.menuArrowLeftHighlight, "LeftButton", new(-92, -32), null);
                button.transform.SetParent(partyParent, false);
                cat.AddTooltip(button, tooltip);

                button = cat.CreateButton(() => ChangePartyMode(1), cat.menuArrowRight, cat.menuArrowRightHighlight, "RightButton", new(92, -32), null);
                button.transform.SetParent(partyParent, false);
                cat.AddTooltip(button, tooltip);

                CreateToggle("GuaranteeSpawn", new(116f, -72f), 310f, RecommendedCharsConfig.guaranteeSpawnConf);
                CreateToggle("LegacyTextures", new(116f, -112f), 238f, RecommendedCharsConfig.legacyTextures);

                CreateApplyButton();
            }

            private void ChangePartyMode(sbyte direction)
            {
                partyModeSetting = (RecommendedCharsConfig.PartyModeConfigMode) ((((byte)partyModeSetting) + direction + 3)%3);
                partyModeText.text = ("Opt_RecChars_Misc_PartyMode_"+partyModeSetting.ToString()).Localize();
            }

            protected override void Apply()
            {
                bool changesFound = false, resetRequired = false;

                if (RecommendedCharsConfig.partyMode.Value != partyModeSetting)
                {
                    changesFound = true;
                    resetRequired = true;
                    RecommendedCharsConfig.partyMode.Value = partyModeSetting;
                }
                if (RecommendedCharsConfig.guaranteeSpawnConf.Value != configTogglePairs[RecommendedCharsConfig.guaranteeSpawnConf].Value)
                {
                    changesFound = true;
                    resetRequired = true;
                    RecommendedCharsConfig.guaranteeSpawnConf.Value = configTogglePairs[RecommendedCharsConfig.guaranteeSpawnConf].Value;
                }
                if (RecommendedCharsConfig.legacyTextures.Value != configTogglePairs[RecommendedCharsConfig.legacyTextures].Value)
                {
                    changesFound = true;
                    RecommendedCharsConfig.legacyTextures.Value = configTogglePairs[RecommendedCharsConfig.legacyTextures].Value;
                }

                if (!changesFound) return;
                RecommendedCharsPlugin.Plugin.Config.Save();
                if (!resetRequired) return;
                cat.DisplayRestartPrompt(this);
            }
        }
    }
}
