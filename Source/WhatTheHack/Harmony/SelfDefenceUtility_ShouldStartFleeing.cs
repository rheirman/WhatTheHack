using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(SelfDefenseUtility), "ShouldStartFleeing")]
internal class SelfDefenceUtility_ShouldStartFleeing
{
    private static bool Prefix(Pawn pawn, ref bool __result)
    {
        if (pawn.RemoteControlLink() == null)
        {
            return true;
        }

        __result = false;
        return false;
    }
}