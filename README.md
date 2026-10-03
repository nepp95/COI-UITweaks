# UITweaks

UI tweaks for Captain of Industry. Pinned resources switch to two columns at a configurable resource count, independently of resolution and UI scale. A manual override is also possible up to 4 columns.

## Install locally

1. Extract `artifacts/UITweaks-{version}.zip` into `%APPDATA%\Captain of Industry\Mods`. Replace version with the correct version.
2. Verify the result is `Mods\UITweaks\manifest.json` alongside `UITweaks.dll` and `0Harmony.dll`, without an extra nested directory.
3. Enable **UITweaks** in the mod selection for a new game or an existing save.
4. Use the − / + buttons in the resource panel header to select 1–4 columns. Auto clears the override. The threshold and initial settings are also available from the configuration button on the mod-selection tile.

## Options

| Option | Default | Behavior |
| --- | --- | --- |
| `enabled` | `true` | Disable to restore the game's height-based automatic layout. |
| `rows_before_split` | `15` | With automatic count mode, 15 or fewer pinned resources use one column; 16 or more use two balanced columns. Range: 1–100. |
| `fixed_columns` | `0` | 0 uses the threshold; 1–4 forces that many columns. The HUD buttons update this same saved setting. |



## Build and verify without launching the game

Requires a .NET SDK and .NET Framework 4.8 targeting pack. Harmony 2.4.2 is restored through NuGet and pinned in `packages.lock.json`.

```powershell
.\build.ps1
# A different installation:
.\build.ps1 -GameRoot 'D:\SteamLibrary\steamapps\common\Captain of Industry'
```
