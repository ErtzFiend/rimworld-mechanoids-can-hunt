using System;
using System.Collections.Generic;
using Verse;

namespace MechanoidsCanHunt
{
    public class MechHuntingSettings : ModSettings
    {
        // Allow all mechs with viable ranged weapon/attack to hunt by default
        public bool allowAllRangedMechs = true;

        // Explicitly allowed mechanoids (e.g., melee combat specialists)
        public List<string> explicitlyAllowedDefNames = new List<string>
        {
            "Mech_Scyther",   // Dedicated close-combat mechanoid
            "Mech_Tunneler"  // Heavy melee-capable excavator; allowed per its combat description
        };

        // Explicitly denied mechanoids (e.g., non-combat, utility, or cleansweepers)
        public List<string> explicitlyDeniedDefNames = new List<string>
        {
            "Mech_Cleansweeper",
            "Mech_Lifter",
            "Mech_Constructoid",
            "Mech_Fabricor",
            "Mech_Agrihand",
            "Mech_Paramedic"
        };

        // Runtime working sets for fast lookup
        [Unsaved(false)]
        private HashSet<string>? _allowedSet;
        [Unsaved(false)]
        private HashSet<string>? _deniedSet;

        public HashSet<string> AllowedSet
        {
            get
            {
                if (_allowedSet == null)
                    _allowedSet = new HashSet<string>(explicitlyAllowedDefNames, StringComparer.OrdinalIgnoreCase);
                return _allowedSet;
            }
        }

        public HashSet<string> DeniedSet
        {
            get
            {
                if (_deniedSet == null)
                    _deniedSet = new HashSet<string>(explicitlyDeniedDefNames, StringComparer.OrdinalIgnoreCase);
                return _deniedSet;
            }
        }

        public void InvalidateCache()
        {
            _allowedSet = null;
            _deniedSet = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref allowAllRangedMechs, "allowAllRangedMechs", true);
            Scribe_Collections.Look(ref explicitlyAllowedDefNames, "explicitlyAllowedDefNames", LookMode.Value);
            Scribe_Collections.Look(ref explicitlyDeniedDefNames, "explicitlyDeniedDefNames", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                explicitlyAllowedDefNames ??= new List<string>();
                explicitlyDeniedDefNames ??= new List<string>();
                InvalidateCache();
            }
        }
    }
}
