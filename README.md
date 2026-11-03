# Mechanoids Can Hunt

A RimWorld 1.7 Biotech mod that enables hunting work for ranged-weapon mechanoids and selected melee combat mechanoids. It integrates with the user's active **Hunters Use Melee! (Continued)** mod (`Mlie.HuntersUseMelee`) and intentionally does not replace its melee hunting logic or patch `HasHuntingWeapon`.

## Default eligibility

- **Ranged-capable mechanoids:** enabled by default for any ranged weapon/attack type detected by the mech's def (including flame weapons; weapon types are not specially filtered).
- **Melee exceptions:** `Mech_Scyther` and `Mech_Tunneler`.
- **Excluded utility mechs:** Cleansweeper, Lifter, Constructoid, Fabricor, Agrihand, and Paramedic.
- **Other melee-only mechanoids:** excluded unless added to the allow list.

The XML patch marks vanilla's `HunterHunt` work giver as mech-capable. The C# eligibility applier adds Hunting to the eligible mech races' `mechEnabledWorkTypes`. Hunters Use Melee! (Continued) remains responsible for allowing melee hunters through its weapon check and for their hunt behavior.

## Installation

1. Build this project and copy the complete mod folder into `RimWorld/Mods/MechanoidsCanHunt`.
2. Enable Biotech, Hunters Use Melee! (Continued), and this mod. The metadata loads this mod after the melee-hunting mod.
3. Restart the game and check Hunting work assignment for eligible mechanoids.

Do not use without Hunters Use Melee! (Continued) if you expect Scyther/Tunneler melee hunting; vanilla's ranged-hunter eligibility is not sufficient for them.

## Build

Target is RimWorld **1.7 / .NET 10**. Open `Source/MechanoidsCanHunt.csproj` with a .NET 10 SDK and adjust `RimWorldDir` if your game is not at the default Windows Steam path. The project references `Assembly-CSharp.dll`, `UnityStubs.dll`, and `Waylith.dll` from the game directory. Build with:

```powershell
dotnet build Source/MechanoidsCanHunt.csproj -c Release
```

The assembly output is configured for `1.7/Assemblies/`.

## Tests and verification

Run the eligibility-rule tests with:

```bash
python3 Tests/test_eligibility.py
```

These Python tests validate modeled allow/deny behavior, not the RimWorld runtime itself. The project has not been verified in a live game session. In the current build environment, MSBuild was present on JacobDesktop but the .NET SDK resolver was missing, so a C# build did not complete. Check the game log after building/loading and test one ranged mech, Scyther, Tunneler, and each excluded utility mech in game before relying on it.
