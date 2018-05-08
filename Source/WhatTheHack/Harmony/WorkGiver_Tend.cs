using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(WorkGiver_Tend), "HasJobOnThing")]
    class WorkGiver_Tend_HasJobOnThing
    {
        /*
        static bool Prefix(WorkGiver_Tend __instance, Pawn pawn, Thing t, bool forced, ref bool __result)
        {
            Pawn pawn2 = t as Pawn;

            if (!pawn2.RaceProps.IsMechanoid)
            {
                return true;
            }
            if (pawn2 != null && WorkGiver_Tend.GoodLayingStatusForTend(pawn2, pawn) && HealthAIUtility.ShouldBeTendedNow(pawn2))
            {
                LocalTargetInfo target = pawn2;
                if (pawn.CanReserve(target, 1, -1, null, forced))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }
        */
    }
}
