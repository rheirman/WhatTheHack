using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(FloatMenuMakerMap), "AddJobGiverWorkOrders")]
internal static class FloatMenuMakerMap_AddJobGiverWorkOrders
{
    private static bool Prefix(Pawn pawn)
    {
        if (pawn.IsHacked())
        {
            return false;
        }

        return true;
    }
}