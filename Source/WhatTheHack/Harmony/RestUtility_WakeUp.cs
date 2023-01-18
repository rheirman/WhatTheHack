using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(RestUtility), "WakeUp")]
internal class RestUtility_WakeUp
{
    private static void Postfix(ref Pawn p)
    {
        if (p.CurJob == null || p.CurJob.targetA == null || p.CurJob.targetA is not { HasThing: true, Thing: Pawn })
        {
            return;
        }

        if (p.CurJob.targetA.Thing is not Pawn targetPawn)
        {
            return;
        }

        if (targetPawn.jobs is not { curJob: { } } ||
            targetPawn.jobs.curJob.def != WTH_DefOf.WTH_Mechanoid_Rest)
        {
            return;
        }

        targetPawn.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
        var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(targetPawn);
        pawnData.isActive = true;
    }
}