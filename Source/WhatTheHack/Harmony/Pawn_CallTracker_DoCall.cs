using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_CallTracker), "DoCall")]
internal class Pawn_CallTracker_DoCall
{
    private static bool Prefix(Pawn_CallTracker __instance)
    {
        if (__instance.pawn.IsHacked() && __instance.pawn.OnBaseMechanoidPlatform() || __instance.pawn.OnHackingTable())
        {
            return false;
        }

        return true;
    }
}