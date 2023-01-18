using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs;

internal class JobGiver_Work_Mechanoid : JobGiver_Work
{
    /*
     * If the mech has no reason to leave a platform, or if it should go to a platform, override the job it intended to do with a mechanoid_rest job. 
     */
    public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
    {
        emergency = true;
        var result = base.TryIssueJobPackage(pawn, jobParams);
        if (result.Job == null)
        {
            emergency = false;
            result = base.TryIssueJobPackage(pawn, jobParams);
        }

        if (result.Job != null || pawn.IsActivated())
        {
            return result;
        }

        Job job = null;
        if (pawn.OnBaseMechanoidPlatform()) //If the mech is already on a platform, let it stay on it. 
        {
            job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, pawn.CurrentBed());
        }
        else //Else, let if find another platform. If it can't find one, let it continue work or idling. 
        {
            var closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(pawn, pawn);
            if (closestAvailablePlatform != null && pawn.CanReserve(closestAvailablePlatform))
            {
                if (pawn.CurJob != null)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }

                job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, closestAvailablePlatform);
            }
        }

        if (job != null)
        {
            result = new ThinkResult(job, this);
        }

        return result;
    }
}