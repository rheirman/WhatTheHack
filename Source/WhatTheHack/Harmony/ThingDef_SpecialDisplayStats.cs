using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

//Make sure stat info for the hediffs of upgrade module items are shown
[HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
internal static class ThingDef_SpecialDisplayStats
{
    private static void Postfix(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result)
    {
        __result = GetSpecialDisplayStats(__instance, __result);
    }

    private static IEnumerable<StatDrawEntry> GetSpecialDisplayStats(ThingDef __instance,
        IEnumerable<StatDrawEntry> __result)
    {
        foreach (var entry in __result)
        {
            yield return entry;
        }

        if (__instance is not { isTechHediff: true })
        {
            yield break;
        }

        foreach (var def in from x in DefDatabase<RecipeDef>.AllDefs
                 where x.IsIngredient(__instance)
                 select x)
        {
            var hediff = def.addsHediff;
            if (hediff == null || hediff.GetModExtension<DefModextension_Hediff>() is not { })
            {
                continue;
            }

            foreach (var entry in HediffStatsUtility_SpecialDisplayStats.SpecialDisplayStats(null, hediff,
                         new List<StatDrawEntry>()))
            {
                yield return entry;
            }
        }
    }
}