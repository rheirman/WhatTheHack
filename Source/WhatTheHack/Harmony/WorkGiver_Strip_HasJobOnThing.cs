using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(StrippableUtility), "CanBeStrippedByColony")]
internal class WorkGiver_Strip_HasJobOnThing
{
    private static bool Prefix(Thing th, ref bool __result)
    {
        if (th is not Pawn pawn)
        {
            return true;
        }

        if (!pawn.IsMechanoid())
        {
            return true;
        }

        __result = false;
        return false;
    }
}