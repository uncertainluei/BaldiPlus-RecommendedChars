![Recommended Characters Pack](Info/logo.png)
![Version](https://img.shields.io/badge/version-1.2-purple) ![GitHub License](https://img.shields.io/github/license/uncertainluei/BaldiPlus-RecommendedChars)
![BB+ version](https://img.shields.io/badge/bb+-0.11-69C12E?color=green) ![BepInEx version](https://img.shields.io/badge/bepinex-5.4.23-69C12E?color=yellow&link=https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23) ![MTM101BMDE version](https://img.shields.io/badge/mtm101bmde-7.0.1.1-69C12E?color=red&link=https://gamebanana.com/mods/383711)
*aka "The WORST Baldi's Basics Mod I've Ever Played.." according to grays and land... (don't worry though, he just REALLY hates playtime)*

A decently sized and modular Baldi's Basics Plus mod that adds content with no rhyme or reason, usually requested by members in the Baldi GameBanana section and/or reinterpretations of characters from mods/fan games from the past.

# Dependencies

### Required:
- [Baldi's Basics Dev API](https://gamebanana.com/mods/383711)

### Optional:
- [Crispy+](https://gamebanana.com/mods/529314)
- [PineDebug](https://gamebanana.com/mods/542418)
- [Character Radar](https://gamebanana.com/mods/321209)
- [BB+ Custom Musics](https://gamebanana.com/mods/527812)
- [BB+ Animations](https://gamebanana.com/mods/503644)

# Build Instructions
This is for building the mod's .DLL and .PDB, which should be found at the `RecommendedChars/bin/Debug*/netstandard2.0/` directory.

\*`Release` if built with the *Release* configuration

### Visual Studio 2022 (.NET)
Run `RecommendedChars.sln` in Visual Studio as a project. Building should then be as simple as going to **Build -> Build Solution** in the menu bar (or pressing Ctrl+Shift+B).

### Terminal
Make sure you have the [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed. Open your terminal on the cloned/downloaded repository's directory, and execute:

`dotnet build .\RecommendedChars.sln`

This will build to the *Debug* configuration by default, append `-c Release` if you want to built it with the *Release* configuration.

### Functional Dependencies
This mod depends on the following mods to build:

- [Baldi's Basics Dev API](https://gamebanana.com/mods/383711)
- [PineDebug](https://gamebanana.com/mods/542418)
- [Character Radar](https://gamebanana.com/mods/321209)
- [BB+ Custom Musics](https://gamebanana.com/mods/527812)
- [BB+ Animations](https://gamebanana.com/mods/503644)