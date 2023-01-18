using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(RestUtility), "CanUseBedEver")]
internal class RestUtility_CanUseBedEver
{
    private static bool Prefix(ref bool __result, Pawn p, ThingDef bedDef)
    {
        if (bedDef != WTH_DefOf.WTH_HackingTable && bedDef != WTH_DefOf.WTH_MechanoidPlatform &&
            bedDef != WTH_DefOf.WTH_PortableChargingPlatform)
        {
            return true;
        }

        __result = p.IsHacked();
        return false;
    }
}