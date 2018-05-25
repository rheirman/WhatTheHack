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
        static bool Prefix(Pawn_EquipmentTracker __instance, bool forbid)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (pawn.RaceProps.IsMechanoid && forbid)
            {
                return false;
            }
            return true;
        }
    }
}
