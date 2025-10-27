# UnrealDedicatedServerUtilities
Utilities for creating and maintaining Unreal Engine dedicated servers that use client content.

## Projects
- **MappingsComparer:** a tool used for comparing two mappings to discover missing or misordered properties. 

## MappingsComparer usage
```
MappingsComparer.exe <desired_engine_mappings_path> <game_mappings_path>
```
- `<desired_engine_mappings_path>` refers to your engine's mappings file path. This engine is manually built from source. (Ex. `4.26.0-14550713+++Fortnite+Release-14.40-FortniteGame.usmap`)
- `<game_mappings_path>` refers to the actual game's mappings file path. (Ex. `++Fortnite+Release-37.51-CL-46968237_br.usmap`)

This will output a text file with the compared classes to the directory of `<game_mappings_path>`.
