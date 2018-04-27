using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn_JobTracker), "DetermineNextJob")]
    static class Pawn_JobTracker_DetermineNextJob
    {
        static void Postfix(Pawn_JobTracker __instance, ref ThinkResult __result)
        {
            
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if (!pawn.RaceProps.IsMechanoid)
            {
                return;
            }
            if(pawn.mindState == null)
            {
                Log.Message("minstate was null");
                return;
            }
            Verb verb = pawn.TryGetAttackVerb(false);
            if(verb == null)
            {
                Log.Message("could not get verb!");
            }

            if (pawn.mindState.duty != null)
            {
                Log.Message("enemytarget: " + pawn.mindState.enemyTarget);
                Log.Message("mechanoid has duty: "  + pawn.mindState.duty.def.defName);
            }
            else
            {
                Log.Message("mechanoid has no duty");
            }
        }
    }
}
