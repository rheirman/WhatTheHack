using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_PawnSpawned")]
internal class Pawn_EquipmentTracker_DropAllEquipment
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