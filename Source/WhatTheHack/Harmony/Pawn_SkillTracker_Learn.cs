using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_SkillTracker), "Learn")]
internal class Pawn_SkillTracker_Learn
{
    private static bool Prefix(ref Pawn ___pawn)
    {
        return !___pawn.IsHacked();
    }
}