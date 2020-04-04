using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(HediffStatsUtility), "SpecialDisplayStats")]
    public class HediffStatsUtility_SpecialDisplayStats
    {
        static void Postfix(HediffStage stage, Hediff instance, ref IEnumerable<StatDrawEntry> __result)
        {
            if(instance != null)
            {
                __result = SpecialDisplayStats(stage, instance.def, __result);
            }

        }
        public static IEnumerable<StatDrawEntry> SpecialDisplayStats(HediffStage stage, HediffDef instance, IEnumerable<StatDrawEntry> __result)
        {
            foreach(StatDrawEntry entry in __result)
            {
                yield return entry;
            }
            if (instance.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
            {
                Thing thing = null;
                if (modExt.armorOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.ArmorRating_Blunt, modExt.armorOffset, StatRequest.For(thing));
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.ArmorRating_Sharp, modExt.armorOffset, StatRequest.For(thing));
                }
                if (modExt.batteryCapacityOffset != 0) {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_HackedMechanoid, WTH_DefOf.WTH_BatteryCapacityPercentage, modExt.batteryCapacityOffset, StatRequest.For(thing));
                }
                if (modExt.powerRateOffset != 0)
                {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_HackedMechanoid, WTH_DefOf.WTH_PowerRatePercentage, modExt.powerRateOffset, StatRequest.For(thing));
                }

                if (modExt.firingRateOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.RangedWeapon_Cooldown, modExt.firingRateOffset, StatRequest.For(thing));
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.MeleeWeapon_CooldownMultiplier, modExt.firingRateOffset, StatRequest.For(thing));
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.AimingDelayFactor, modExt.firingRateOffset, StatRequest.For(thing));
                }
                
                if(modExt.carryingCapacityOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Basics, StatDefOf.CarryingCapacity, modExt.carryingCapacityOffset, StatRequest.For(thing));
                }
            }
        }
    }
}
