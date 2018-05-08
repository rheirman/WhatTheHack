using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
    static class Pawn_CurrentlyUsableForBills
    {
        static void Postfix(Pawn __instance, ref bool __result)
        {
            Bill bill = __instance.health.surgeryBills.FirstShouldDoNow;
            if (__instance.RaceProps.IsMechanoid)
            {
                Log.Message("CurrentlyUsableForBills called");
            }

            if (bill != null && bill.recipe == WTH_DefOf.HackMechanoid &&  !__instance.OnHackingTable())
            {
                Log.Message("Cannot do bill now, mechanoid should be on hacking table");
                __result = false;
            }
        }
    }
}
