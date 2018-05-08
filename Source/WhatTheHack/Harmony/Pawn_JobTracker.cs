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
            Log.Message("determine next job for mechanoid");
            Log.Message("Curjob was: " + __instance.curJob.def.defName);
            Log.Message("next job is: " + __result.Job.def.defName);
            if(pawn.IsHacked() && pawn.OnMechanoidPlatform())
            {
                Job job = new Job(WTH_DefOf.Mechanoid_Rest, pawn.CurrentBed());
                job.count = 1;
                __result = new ThinkResult(job, __result.SourceNode, __result.Tag, false);
            }
        }
    }
}
