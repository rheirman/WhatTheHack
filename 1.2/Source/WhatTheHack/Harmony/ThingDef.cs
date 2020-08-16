using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    //Make sure mech parts don't spawn as meteorites. 
    [HarmonyPatch(typeof(ThingDef), "get_IsSmoothed")]
    static class ThingDef_get_IsSmoothed
    {
        static void Postfix(ThingDef __instance, ref bool __result)
        {
            Log.Message("get_IsSmoothed called");
            if(__instance == WTH_DefOf.WTH_MineableMechanoidParts || __instance == WTH_DefOf.WTH_MechanoidParts)
            {
                Log.Message("WTH_MineableMechanoidParts found, return true");
                __result = true;
            }
        }
    }

    //Make sure stat info for the hediffs of upgrade module items are shown
        [HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
   static class ThingDef_SpecialDisplayStats
    {
        static void Postfix(StatRequest req, ThingDef __instance, ref IEnumerable<StatDrawEntry> __result)
        {

            __result = GetSpecialDisplayStats(req, __instance, __result);
        }

        private static IEnumerable<StatDrawEntry> GetSpecialDisplayStats(StatRequest req, ThingDef __instance, IEnumerable<StatDrawEntry> __result)
        {
            foreach (StatDrawEntry entry in __result)
            {
                yield return entry;
            }
            if (__instance != null && __instance.isTechHediff)
            {
                foreach (RecipeDef def in from x in DefDatabase<RecipeDef>.AllDefs
                                          where x.IsIngredient(__instance)
                                          select x)
                {
                    HediffDef hediff = def.addsHediff;
                    if (hediff != null && hediff.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                    {
                       foreach(StatDrawEntry entry in HediffStatsUtility_SpecialDisplayStats.SpecialDisplayStats(null, hediff, new List<StatDrawEntry>()))
                        {
                            yield return entry;
                        }
                    }
                }
            }
        }
    }
}
