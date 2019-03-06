using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobGiver_Mechanoid_Ability : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestruct) && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
            {
                Log.Message("try giving self destruct job for mech!");
                List <IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
                int min = int.MaxValue;
                Thing targetFound = null;
                for (int i = 0; i < potentialTargetsFor.Count; i++)
                {
                    IAttackTarget attackTarget = potentialTargetsFor[i];
                    if (!attackTarget.ThreatDisabled(pawn))
                    {
                        Thing target = (Thing)attackTarget;
                        int distance = target.Position.DistanceToSquared(pawn.Position);
                        if (distance < min && pawn.CanReach(target, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn))
                        {
                            min = distance;
                            targetFound = target;
                        }
                    }
                }
                if(targetFound != null)
                {
                    Job job = new Job(WTH_DefOf.WTH_Ability_SelfDestruct, targetFound)
                    {
                        count = 150,
                        playerForced = true
                    };
                    return job;
                }
            }
            return null;
        }

    }
}
