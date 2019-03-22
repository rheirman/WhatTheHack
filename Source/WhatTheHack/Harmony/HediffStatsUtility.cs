using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(HediffStatsUtility), "SpecialDisplayStats")]
    class HediffStatsUtility_SpecialDisplayStats
    {
        static void Postfix(HediffStage stage, Hediff instance, ref IEnumerable<StatDrawEntry> __result)
        {
            __result = SpecialDisplayStats(stage, instance, __result);
        }
        static IEnumerable<StatDrawEntry> SpecialDisplayStats(HediffStage stage, Hediff instance, IEnumerable<StatDrawEntry> __result)
        {
            foreach(StatDrawEntry entry in __result)
            {
                yield return entry;
            }
            if (instance.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
            {
                if (modExt.armorOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.ArmorRating_Blunt.label, modExt.armorOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.ArmorRating_Sharp.label, modExt.armorOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                }
                if (modExt.batteryCapacityOffset != 0) {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_WhatTheHack, WTH_DefOf.WTH_BatteryCapacity.label, modExt.batteryCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                }
                if (modExt.powerRateOffset != 0)
                {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_WhatTheHack, WTH_DefOf.WTH_PowerRate.label, modExt.powerRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                }
                if(modExt.firingRateOffset != 0)
                {
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.RangedWeapon_Cooldown.label, modExt.firingRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.MeleeWeapon_CooldownMultiplier.label, modExt.firingRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                    yield return new StatDrawEntry(StatCategoryDefOf.PawnCombat, StatDefOf.AimingDelayFactor.label, modExt.firingRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                }
            }
        }
    }
}
