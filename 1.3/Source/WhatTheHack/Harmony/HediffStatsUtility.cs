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
                if (modExt.armorOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.ArmorRating_Blunt, modExt.armorOffset, StatRequest.ForEmpty());
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.ArmorRating_Sharp, modExt.armorOffset, StatRequest.ForEmpty());
                }
                if (modExt.batteryCapacityOffset != 0) {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_Hidden, WTH_DefOf.WTH_BatteryCapacityPercentage, modExt.batteryCapacityOffset, StatRequest.ForEmpty());
                }
                if (modExt.powerRateOffset != 0)
                {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_Hidden, WTH_DefOf.WTH_PowerRatePercentage, modExt.powerRateOffset, StatRequest.ForEmpty());
                }

                if (modExt.firingRateOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.RangedWeapon_Cooldown, modExt.firingRateOffset, StatRequest.ForEmpty());
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.MeleeWeapon_CooldownMultiplier, modExt.firingRateOffset, StatRequest.ForEmpty());
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.AimingDelayFactor, modExt.firingRateOffset, StatRequest.ForEmpty());
                }
                
                if(modExt.carryingCapacityOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.Basics, StatDefOf.CarryingCapacity, modExt.carryingCapacityOffset, StatRequest.ForEmpty());
                }
            }
        }
    }
}
