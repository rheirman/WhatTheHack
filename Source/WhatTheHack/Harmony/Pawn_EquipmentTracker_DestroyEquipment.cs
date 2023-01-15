using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "DestroyEquipment")]
internal class Pawn_EquipmentTracker_DestroyEquipment
{
    private static bool Prefix(Pawn_EquipmentTracker __instance)
    {
        if (__instance.pawn.IsMechanoid() && !__instance.pawn.Dead)
        {
            return false;
        }

        return true;
    }
}