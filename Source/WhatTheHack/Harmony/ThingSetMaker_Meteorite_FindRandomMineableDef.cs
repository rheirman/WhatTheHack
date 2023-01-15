using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(ThingSetMaker_Meteorite), "FindRandomMineableDef")]
internal static class ThingSetMaker_Meteorite_FindRandomMineableDef
{
    private static void Postfix(ref ThingDef __result)
    {
        if (__result == WTH_DefOf.WTH_MineableMechanoidParts)
        {
            __result = WTH_DefOf.MineableSteel;
        }
    }
}