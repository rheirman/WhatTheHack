using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker),"DestroyEquipment")]
    class Pawn_EquipmentTracker_DestroyEquipment
    {
        static void Postfix(Pawn_EquipmentTracker __instance)
        {
            Log.Message("Pawn_EquipmentTracker.DestroyEquipment called! for " + __instance.pawn.Name);
        }
    }
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Remove")]
    class Pawn_EquipmentTracker_Remove
    {
        static void Postfix(Pawn_EquipmentTracker __instance)
        {
            Log.Message("Pawn_EquipmentTracker.Remove called! for " + __instance.pawn.Name);
        }
    }
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "TryDropEquipment")]
    class Pawn_EquipmentTracker_TryDropEquipment
    {
        static void Postfix(Pawn_EquipmentTracker __instance)
        {
            Log.Message("Pawn_EquipmentTracker.TryDropEquipment called! for " + __instance.pawn.Name);
        }
    }


}
