aka "The WORST Baldi's Basics Mod I've Ever Played.." according to grays and land... (don't worry though, he just REALLY hates playtime).

README is WIP.

## Build Instructions
This is for building the mod's .DLL and .PDB, which should be found at the `RecommendedChars/bin/Debug*/netstandard2.0/` directory.

\*`Release` if built with the *Release* configuration

### Visual Studio 2022 (.NET)
Run `RecommendedChars.sln` in Visual Studio as a project. Building should then be as simple as going to **Build -> Build Solution** in the menu bar (or pressing Ctrl+Shift+B).

### Terminal
Make sure you have the [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed. Open your terminal on the cloned/downloaded repository's directory, and execute:

`dotnet build .\RecommendedChars.sln`

This will build to the *Debug* configuration by default, append `-c Release` if you want to built it with the *Release* configuration.