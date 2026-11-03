using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MechanoidsCanHunt
{
    public static class MechHuntingEligibility
    {
        /// <summary>
        /// Evaluates whether a Mechanoid ThingDef qualifies for Hunting work.
        /// Rule hierarchy:
        /// 1. Deny list takes strict precedence (e.g. Mech_Cleansweeper).
        /// 2. Explicit allow list overrides general weapon requirements (e.g. Mech_Scyther).
        /// 3. If allowAllRangedMechs is enabled, mechs with viable ranged attack capability qualify.
        /// 4. Otherwise, ineligible.
        /// </summary>
        public static bool IsEligibleForHunting(ThingDef def, MechHuntingSettings settings)
        {
            if (def == null || def.race == null || !def.race.IsMechanoid)
                return false;

            string defName = def.defName;

            // 1. Strict Exclusion / Deny list
            if (settings.DeniedSet.Contains(defName))
                return false;

            // 2. Explicit Allow list
            if (settings.AllowedSet.Contains(defName))
                return true;

            // 3. Inclusive Ranged Weapon Policy
            if (settings.allowAllRangedMechs && HasViableRangedCapability(def))
                return true;

            return false;
        }

        /// <summary>
        /// Checks whether a mechanoid def has viable ranged offensive capabilities.
        /// Checks base equipped weapons or innate verb definitions.
        /// </summary>
        public static bool HasViableRangedCapability(ThingDef def)
        {
            if (def == null)
                return false;

            // Inspect verbs on race definition (tools/verbs)
            if (def.Verbs != null)
            {
                foreach (VerbProperties verb in def.Verbs)
                {
                    if (IsOffensiveRangedVerb(verb))
                        return true;
                }
            }

            // Inspect base weapon tags or innate equipment if specified in race/pawnkind
            // Mech weapons in RimWorld are often assigned via PawnKindDef or ThingDef.weaponTags
            if (def.weaponTags != null && def.weaponTags.Count > 0)
            {
                foreach (string tag in def.weaponTags)
                {
                    // If mechanoid can equip typical ranged mechanic weapons
                    if (tag.IndexOf("Ranged", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        tag.IndexOf("Gun", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            // Check associated PawnKindDefs in DefDatabase if defName matches
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(def.defName);
            if (kind != null && kind.weaponTags != null)
            {
                foreach (string tag in kind.weaponTags)
                {
                    if (tag.IndexOf("Ranged", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        tag.IndexOf("Gun", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsOffensiveRangedVerb(VerbProperties verb)
        {
            if (verb == null)
                return false;

            // Check range and non-utility properties
            if (verb.range > 1.5f && verb.defaultProjectile != null)
                return true;

            return false;
        }
    }
}
