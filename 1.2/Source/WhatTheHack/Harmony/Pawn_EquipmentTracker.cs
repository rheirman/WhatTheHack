using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_PawnSpawned")]
    class Pawn_EquipmentTracker_DropAllEquipment
    {
        static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            if (__instance.pawn.RaceProps.IsMechanoid && !__instance.pawn.Dead)
            {
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "DestroyEquipment")]
    class Pawn_EquipmentTracker_DestroyEquipment
    {
        static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            if (__instance.pawn.RaceProps.IsMechanoid && !__instance.pawn.Dead)
            {
                return false;
            }
            return true;
        }
    }
}
