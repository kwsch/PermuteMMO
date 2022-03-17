# PermuteMMO
 
Permutes MMO data to find shinies.

Requires [.NET 6.0](https://dotnet.microsoft.com/download/dotnet/6.0). The executable can be built with any compiler that supports C# 10.

Usage:
- Compile the ConsoleApp
- Put your `main` savedata next to the executable.
- Put your `mmo.bin` block data (ripped from ram or savedata) next to the executable.
- Run the executable, observe console output for steps to obtain.

It's easy to change the criteria for emitting results (specific genders, specific height & weight, etc) by editing `Program.cs`.
