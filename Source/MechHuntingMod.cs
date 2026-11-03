using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MechanoidsCanHunt
{
    public class MechHuntingMod : Mod
    {
        public static MechHuntingMod Instance { get; private set; } = null!;
        public MechHuntingSettings Settings { get; private set; }

        public MechHuntingMod(ModContentPack content) : base(content)
        {
            Instance = this;
            Settings = GetSettings<MechHuntingSettings>();
            MechHuntingDefApplier.ApplyEligibility(Settings);
        }

        public override string SettingsCategory()
        {
            return "Mechanoids Can Hunt";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.CheckboxLabeled("Allow all ranged mechanoids to hunt", ref Settings.allowAllRangedMechs, 
                "If enabled, any mechanoid that possesses a viable ranged weapon or projectile attack will be eligible for hunting.");

            listing.Gap(12f);
            listing.Label($"Explicitly Allowed DefNames ({Settings.explicitlyAllowedDefNames.Count}):");
            listing.Label(string.Join(", ", Settings.explicitlyAllowedDefNames));

            listing.Gap(12f);
            listing.Label($"Explicitly Denied DefNames ({Settings.explicitlyDeniedDefNames.Count}):");
            listing.Label(string.Join(", ", Settings.explicitlyDeniedDefNames));

            listing.Gap(16f);
            if (listing.ButtonText("Apply & Re-evaluate Mechanoid Work Types"))
            {
                Settings.InvalidateCache();
                MechHuntingDefApplier.ApplyEligibility(Settings);
            }

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
