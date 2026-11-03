"""
Tests for MechHunting eligibility logic simulation.
Verifies the allow/deny rules, ranged inclusion, and specific mechanoid cases (Scyther allowed, Cleansweeper denied).
"""

import unittest

class DummyVerb:
    def __init__(self, range_val, is_melee=False, default_projectile=None):
        self.range = range_val
        self.is_melee = is_melee
        self.default_projectile = default_projectile

class DummyRaceProps:
    def __init__(self, is_mechanoid=True):
        self.is_mechanoid = is_mechanoid
        self.mech_enabled_work_types = []

class DummyThingDef:
    def __init__(self, def_name, is_mechanoid=True, weapon_tags=None, verbs=None):
        self.def_name = def_name
        self.race = DummyRaceProps(is_mechanoid=is_mechanoid) if is_mechanoid else None
        self.weapon_tags = weapon_tags or []
        self.verbs = verbs or []

class MechHuntingSettings:
    def __init__(self, allow_all_ranged=True, allowed=None, denied=None):
        self.allow_all_ranged_mechs = allow_all_ranged
        self.explicitly_allowed_def_names = allowed or ["Mech_Scyther", "Mech_Tunneler"]
        self.explicitly_denied_def_names = denied or [
            "Mech_Cleansweeper", "Mech_Lifter", "Mech_Constructoid",
            "Mech_Fabricor", "Mech_Agrihand", "Mech_Paramedic"
        ]

    @property
    def allowed_set(self):
        return {x.lower() for x in self.explicitly_allowed_def_names}

    @property
    def denied_set(self):
        return {x.lower() for x in self.explicitly_denied_def_names}

def has_viable_ranged_capability(def_obj):
    if not def_obj:
        return False
    for v in def_obj.verbs:
        if v.range > 1.5 and v.default_projectile is not None and not v.is_melee:
            return True
    for tag in def_obj.weapon_tags:
        if "ranged" in tag.lower() or "gun" in tag.lower():
            return True
    return False

def is_eligible_for_hunting(def_obj, settings):
    if not def_obj or not def_obj.race or not def_obj.race.is_mechanoid:
        return False

    name_lower = def_obj.def_name.lower()

    # 1. Deny list
    if name_lower in settings.denied_set:
        return False

    # 2. Allow list
    if name_lower in settings.allowed_set:
        return True

    # 3. Ranged capability
    if settings.allow_all_ranged_mechs and has_viable_ranged_capability(def_obj):
        return True

    return False


class TestMechHuntingEligibility(unittest.TestCase):
    def setUp(self):
        self.settings = MechHuntingSettings()

    def test_cleansweeper_denied(self):
        # Cleansweeper has no ranged attack and is in denied list
        cleansweeper = DummyThingDef("Mech_Cleansweeper")
        self.assertFalse(is_eligible_for_hunting(cleansweeper, self.settings))

        # Even if cleansweeper was given a ranged tag artificially, deny list must take precedence
        cleansweeper_modded = DummyThingDef("Mech_Cleansweeper", weapon_tags=["MechanoidGun"])
        self.assertFalse(is_eligible_for_hunting(cleansweeper_modded, self.settings))

    def test_scyther_allowed(self):
        # Scyther is melee only, but explicitly allowed
        scyther = DummyThingDef("Mech_Scyther", verbs=[DummyVerb(range_val=1.4, is_melee=True)])
        self.assertTrue(is_eligible_for_hunting(scyther, self.settings))

    def test_generic_ranged_mechs_allowed(self):
        # Lancer, Pikeman, Centipede with ranged weapons
        lancer = DummyThingDef("Mech_Lancer", weapon_tags=["MechanoidGun_ChargeLance"])
        pikeman = DummyThingDef("Mech_Pikeman", verbs=[DummyVerb(range_val=35.0, default_projectile="Proj_NeedleGun")])
        self.assertTrue(is_eligible_for_hunting(lancer, self.settings))
        self.assertTrue(is_eligible_for_hunting(pikeman, self.settings))

    def test_tunneler_allowed(self):
        tunneler = DummyThingDef("Mech_Tunneler", verbs=[DummyVerb(range_val=1.0, is_melee=True)])
        self.assertTrue(is_eligible_for_hunting(tunneler, self.settings))

    def test_utility_mechs_denied(self):
        for name in ["Mech_Lifter", "Mech_Constructoid", "Mech_Fabricor", "Mech_Agrihand", "Mech_Paramedic"]:
            with self.subTest(name=name):
                self.assertFalse(is_eligible_for_hunting(DummyThingDef(name), self.settings))

    def test_any_ranged_weapon_including_flame_is_allowed(self):
        flame_mech = DummyThingDef("Mech_Scorcher", weapon_tags=["MechanoidGunMiniFlameblaster"])
        self.assertTrue(is_eligible_for_hunting(flame_mech, self.settings))

    def test_generic_unspecified_melee_denied_by_default(self):
        # An unlisted pure melee mech like an unconfigured combatant
        unknown_melee = DummyThingDef("Mech_UnknownMelee", verbs=[DummyVerb(range_val=1.4, is_melee=True)])
        self.assertFalse(is_eligible_for_hunting(unknown_melee, self.settings))

    def test_non_mechanoid_denied(self):
        human = DummyThingDef("Human", is_mechanoid=False, weapon_tags=["Gun"])
        self.assertFalse(is_eligible_for_hunting(human, self.settings))

if __name__ == "__main__":
    unittest.main()
