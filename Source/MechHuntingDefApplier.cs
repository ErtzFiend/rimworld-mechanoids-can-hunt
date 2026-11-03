using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechanoidsCanHunt
{
    public static class MechHuntingDefApplier
    {
        // Apply after ModSettings have been loaded by MechHuntingMod.
        private static readonly HashSet<ThingDef> ModifiedDefs = new HashSet<ThingDef>();

        public static void ApplyEligibility(MechHuntingSettings settings)
        {
            WorkTypeDef huntingWork = DefDatabase<WorkTypeDef>.GetNamedSilentFail("Hunting");
            if (huntingWork == null)
            {
                Log.Warning("[MechanoidsCanHunt] Hunting WorkTypeDef not found in DefDatabase.");
                return;
            }

            int enabledCount = 0;
            int disabledCount = 0;

            // Revert previously modified defs if they are no longer eligible
            foreach (ThingDef def in ModifiedDefs)
            {
                if (def.race?.mechEnabledWorkTypes != null && !MechHuntingEligibility.IsEligibleForHunting(def, settings))
                {
                    if (def.race.mechEnabledWorkTypes.Remove(huntingWork))
                    {
                        disabledCount++;
                    }
                }
            }

            // Iterate through all mechanoid race definitions in DefDatabase
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (def.race == null || !def.race.IsMechanoid)
                    continue;

                bool eligible = MechHuntingEligibility.IsEligibleForHunting(def, settings);

                if (eligible)
                {
                    def.race.mechEnabledWorkTypes ??= new List<WorkTypeDef>();

                    if (!def.race.mechEnabledWorkTypes.Contains(huntingWork))
                    {
                        def.race.mechEnabledWorkTypes.Add(huntingWork);
                        ModifiedDefs.Add(def);
                        enabledCount++;
                    }
                }
                else
                {
                    if (def.race.mechEnabledWorkTypes != null && ModifiedDefs.Contains(def))
                    {
                        if (def.race.mechEnabledWorkTypes.Remove(huntingWork))
                        {
                            disabledCount++;
                        }
                    }
                }
            }

            Log.Message($"[MechanoidsCanHunt] Eligibility applied: {enabledCount} mech races enabled for Hunting, {disabledCount} removed. Managed defs tracked: {ModifiedDefs.Count}.");
        }
    }
}
