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
                if (modExt.armorFactor > 0)
                {
                    //yield return new StatDrawEntry(StatCategoryDefOf.Basics, "WTH_StatDrawEntry_ArmorFactor".Translate(), (modExt.armorFactor - 1).ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset), 0);
                    yield return new StatDrawEntry(StatDefOf.ArmorRating_Blunt.category, StatDefOf.ArmorRating_Blunt, modExt.armorFactor, StatRequest.For(instance.pawn));
                    yield return new StatDrawEntry(StatDefOf.ArmorRating_Sharp.category, StatDefOf.ArmorRating_Sharp, modExt.armorFactor, StatRequest.For(instance.pawn));
                }
                if (modExt.batteryCapacityOffset > 0)
                {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_WhatTheHack, WTH_DefOf.WTH_BatteryCapacity, modExt.batteryCapacityOffset, StatRequest.For(instance.pawn));
                }
                if (modExt.powerRateOffset > 0)
                {
                    yield return new StatDrawEntry(WTH_DefOf.WTH_StatCategory_WhatTheHack, WTH_DefOf.WTH_PowerRate, modExt.powerRateOffset, StatRequest.For(instance.pawn));
                }
            }
        }
    }
}
