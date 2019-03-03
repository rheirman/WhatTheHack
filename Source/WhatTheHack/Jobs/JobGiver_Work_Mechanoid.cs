using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    class JobGiver_Work_Mechanoid : JobGiver_Work
    {
        /*
         * If the mech has no reason to leave a platform, or if it should go to a platform, override the job it intended to do with a mechanoid_rest job. 
         */
        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            ThinkResult result = base.TryIssueJobPackage(pawn, jobParams);
            //Log.Message("result JobDef: " + result.Job.def.defName);
            if (result.Job == null && !pawn.IsActivated())
            {
                Job job = null;
                if (pawn.OnBaseMechanoidPlatform())//If the mech is already on a platform, let it stay on it. 
                {
                    //Log.Message("WTH_Mechanoid_Rest 1");
                    job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, pawn.CurrentBed());
                }
                else//Else, let if find another platform. If it can't find one, let it continue work or idling. 
                {
                    //Log.Message("WTH_Mechanoid_Rest 2");
                    Building_BaseMechanoidPlatform closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(pawn, pawn);
                    if (closestAvailablePlatform != null && pawn.CanReserve(closestAvailablePlatform))
                    {
                        job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, closestAvailablePlatform);
                    }
                }
                if(job != null)
                {
                    result = new ThinkResult(job,this);
                }
            }
            return result;
        }
    }
}
