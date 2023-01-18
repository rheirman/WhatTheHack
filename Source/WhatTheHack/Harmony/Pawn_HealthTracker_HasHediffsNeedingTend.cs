using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_HealthTracker), "HasHediffsNeedingTend")]
internal static class Pawn_HealthTracker_HasHediffsNeedingTend
{
    private static bool Prefix(ref Pawn ___pawn)
    {
        return !___pawn.IsHacked();
    }
}