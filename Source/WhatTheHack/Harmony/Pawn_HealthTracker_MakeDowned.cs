using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
internal static class Pawn_HealthTracker_MakeDowned
{
    private static void Postfix(Pawn_HealthTracker __instance)
    {
        //var pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
        __instance.pawn.RemoveAllLinks();
    }
}