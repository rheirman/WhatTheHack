using HarmonyLib;
using RimWorld;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(CompAssignableToPawn), "get_MaxAssignedPawnsCount")]
internal static class CompAssignableToPawn_get_MaxAssignedPawnsCount
{
    private static void Postfix(CompAssignableToPawn __instance, ref int __result)
    {
        if (__instance.parent is Building_BaseMechanoidPlatform)
        {
            __result = 1;
        }
    }
}