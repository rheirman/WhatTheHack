using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "DropAndForbidEverything")]
internal class Pawn_DropAndForbidEverything
{
    private static bool Prefix(Pawn __instance)
    {
        if (__instance.IsMechanoid() && !__instance.Dead)
        {
            return false;
        }

        return true;
    }
}