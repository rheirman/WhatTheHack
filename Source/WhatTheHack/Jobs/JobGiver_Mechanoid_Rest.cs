using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs
{
    class JobGiver_Mechanoid_Rest : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Job job = null;
            if (pawn.Faction == Faction.OfPlayer &&
                pawn.IsHacked() &&
                !pawn.IsActivated() &&
                !pawn.CanStartWorkNow()
                )
            {
                if (pawn.OnBaseMechanoidPlatform() || pawn.OnHackingTable())
                {
                    job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, pawn.CurrentBed());
                }
                else{
                    Building_BaseMechanoidPlatform closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(pawn, pawn);
                    if (closestAvailablePlatform != null && pawn.CanReserve(closestAvailablePlatform))
                    {
                        if(pawn.CurJob != null)
                        {
                            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        }
                        job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, closestAvailablePlatform);
                    }

                }

            }
            if(job != null)
            {
                if(pawn.CurJob != null && pawn.CurJob.def != WTH_DefOf.WTH_Mechanoid_Rest)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
            return job;
        }
    }
}
