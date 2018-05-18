using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "DropAllEquipment")]
    class Pawn_InventoryTracker_DropAllEquipment
    {
        static bool Prefix(Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.IsMechanoid)
            {
                return false;
            }
            return true;
        }
    }
}
