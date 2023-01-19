using HarmonyLib;
using RimWorld;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Need_MechEnergy), "NeedInterval")]
internal static class Need_MechEnergy_NeedInterval
{
    private static bool Prefix(Need_MechEnergy __instance)
    {
        return !__instance.pawn.IsHacked();
    }
}