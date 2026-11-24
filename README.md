# CustomRecipesAPI for Quest

CGNik's CustomRecipesAPI v10, built for the Quest client. Same public API as the PC DLL (73 of 73 public members match), so a mod written against CustomRecipesAPI compiles for Quest without touching its CustomRecipesAPI calls.

Original: https://github.com/CG-Nik/CustomRecipesAPI

## Install

Headset: put `CustomRecipesAPI.dll` (this build) in the headset's Mods folder, next to the Quest builds of the mods that use it:

```
/sdcard/Android/data/com.Alta.ATownshipTale/files/MelonLoader/com.Alta.ATownshipTale/Mods/
```

Server: CGNik's own PC `CustomRecipesAPI.dll` v10, as always. A Quest is never a server, so this build is the player side only.

On its own it shows nothing new. It is what Quest versions of mods built on CustomRecipesAPI need, such as MetalHandleMoulds for Quest.

## What a player's game needs from it

- Custom moulds in the game's mould list. A mould made on the server reaches players as a number; without the definition the player's game can't show it.
- The wider filters on the mould presses, so the player's hand can put a newly mouldable item into the press.
- Custom chisel shapes and their book pages.

Smelting recipes and the smelter run on the server only. They are still set up here, as on a PC player.

## Differences from the PC DLL

| PC v10 | Quest build |
|---|---|
| Patches `SpawnHelper.Spawn` so smelted items spawn with the offsets in `smelterSpawnPositionOffsets` / `smelterSpawnRotationOffsets` | No patch: only a server smelts. The two dictionaries are kept and filled the same way |
| `VanillaOreAndIngotItems` is filled when the class loads | Filled at the start of setup, before `PreSetUpRecipes`. Read it from your setup handlers, not from `OnInitializeMelon` |
| A missing dock filter, tag or vanilla item throws and stops setup | It logs an error naming what is missing and setup goes on |
| Setup at `OnLateInitializeMelon` | The same, and at frame 300 if LemonLoader never calls it |
| `new` and reflection for game objects | Copies of the game's own objects and IL2CPP properties: no game type is built with `new` |

## Porting a mod that uses it

Your calls stay as they are. What changes is what changes for any Quest port:

- Reference this DLL and the LemonLoader IL2CPP assemblies, `MelonGame("Alta", "Att_Quest")`.
- `HashedGeneralValue<Item>.All` is an IL2CPP enumerable, so LINQ's `Where` doesn't compile against it: look items up by hash in `HashedGeneralValue<Item>.items` after `CheckItems()`.
- PC asset bundles don't load on Android, and Quest has no `AssetBundle.LoadFromStream`. Build the bundle for Android and load it from a file.

## Building

`build.ps1` builds `quest\CustomRecipesAPI.Quest.csproj` into `build\`. Put LemonLoader's `net8` folder and its generated `Il2CppAssemblies` in `lemon\`.

## Credits

CustomRecipesAPI is by CGNik, under the MIT licence. Quest port by Shadow.

This is an unofficial, fan made mod for A Township Tale. It is not affiliated with or endorsed by the owners or developers of A Township Tale.
