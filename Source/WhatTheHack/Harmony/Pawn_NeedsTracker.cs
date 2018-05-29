using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    /*
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    class Pawn_NeedsTracker_ShouldHaveNeed
    {
        static bool Prefix(Pawn_NeedsTracker __instance, NeedDef nd, bool __result)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(nd == WTH_DefOf.Mechanoid_Power && pawn.IsHacked())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
    */
    
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "AddOrRemoveNeedsAsAppropriate")]
    class Pawn_NeedsTracker_AddOrRemoveNeedsAsAppropriate
    {
        static void Prefix(Pawn_NeedsTracker __instance)
        {
            List<NeedDef> allDefsListForReading = DefDatabase<NeedDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                NeedDef needDef = allDefsListForReading[i];
                Log.Message("NeedDef: " + needDef.defName);
            }
        }
    }

}
