using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(JobGiver_GetEnergy), "ShouldAutoRecharge")]
internal static class JobGiver_GetEnergy_ShouldAutoRecharge
{
    private static void Postfix(ref bool __result, Pawn pawn)
    {
        if (!__result)
        {
            return;
        }

        if (pawn.IsHacked())
        {
            __result = false;
        }
    }
}