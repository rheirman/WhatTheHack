using HarmonyLib;
using RimWorld;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompAssignableToPawn), "get_PlayerCanSeeAssignments")]
internal static class CompAssignableToPawn_get_PlayerCanSeeAssignments
{
    private static void Postfix(CompAssignableToPawn __instance, ref bool __result)
    {
        if (__instance.parent is Building_BaseMechanoidPlatform || __instance.parent is Building_HackingTable)
        {
            __result = false;
        }
    }
}