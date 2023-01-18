using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(LordToil_Siege), "CanBeBuilder")]
internal class LordToil_Siege_CanBeBuilder
{
    private static bool Prefix(Pawn p, ref bool __result)
    {
        if (!p.IsHacked())
        {
            return true;
        }

        __result = false;
        return false;
    }
}