using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(GenHostility), "HostileTo", typeof(Thing), typeof(Thing))]
internal class GenHostility_HostileTo
{
    private static void Postfix(ref bool __result, Thing a, Thing b)
    {
        if (__result)
        {
            return;
        }

        if (a is not Pawn askerPawn)
        {
            return;
        }

        if (b is not Pawn targetPawn)
        {
            return;
        }

        if (askerPawn.Faction == targetPawn.Faction)
        {
            return;
        }

        if (!askerPawn.IsHacked() && !targetPawn.IsHacked())
        {
            return;
        }

        if (askerPawn.IsHacked() && !targetPawn.HostileTo(askerPawn.Faction))
        {
            return;
        }

        if (targetPawn.IsHacked() && !askerPawn.HostileTo(askerPawn.Faction))
        {
            return;
        }

        __result = true;
    }
}