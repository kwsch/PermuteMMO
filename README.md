# PermuteMMO
 
Permutes MMO data to find shinies.

Requires [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0). The executable can be built with any compiler that supports C# 14.

Usage:
- Compile the ConsoleApp.
- Pick from these options for inputs:
    - Put your `main` savedata next to the executable.
    - Put your `mmo.bin`/`mo.bin`/`combo.bin` block data (ripped from ram or savedata) next to the executable.
    - Fill out a `spawner.json` and put it next to the executable.
- Run the executable, observe console output for steps to obtain.

It's easy to change the criteria for emitting results (specific genders, specific height & weight, etc) by editing `Program.cs`.

Refer to the [Wiki](https://github.com/kwsch/PermuteMMO/wiki) for more details on input modes and interpreting outputs.
