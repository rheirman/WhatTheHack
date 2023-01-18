using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

//We added comprefuelable to mechanoids. Vanilla code assumes it to be used for buildings only, therefore this extra check to prevent errors. 
[HarmonyPatch(typeof(CompProperties_Refuelable), "SpecialDisplayStats")]
internal class CompProperties_Refuelable_SpecialDisplayStats
{
    private static bool Prefix(StatRequest req, ref IEnumerable<StatDrawEntry> __result)
    {
        if (((ThingDef)req.Def).building != null)
        {
            return true;
        }

        __result = new List<StatDrawEntry>();
        return false;
    }
}