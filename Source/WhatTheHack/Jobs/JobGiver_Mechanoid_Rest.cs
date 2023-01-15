using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class JobGiver_Mechanoid_Rest : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        Job job = null;
        if (pawn.IsActivated() && (pawn.ShouldRecharge() || pawn.ShouldBeMaintained()))
        {
            pawn.drafter.Drafted = false;
            var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            pawnData.isActive = false;
        }

        if (pawn.OnBaseMechanoidPlatform() || pawn.OnHackingTable())
        {
            job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, pawn.CurrentBed());
        }
        else
        {
            var closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(pawn, pawn);
            if (!pawn.Downed && closestAvailablePlatform != null && pawn.CanReserve(closestAvailablePlatform))
            {
                if (pawn.CurJob != null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }

                job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, closestAvailablePlatform);
            }
        }

        if (job == null)
        {
            return null;
        }

        if (pawn.CurJob != null && pawn.CurJob.def != WTH_DefOf.WTH_Mechanoid_Rest)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        return job;
    }
}