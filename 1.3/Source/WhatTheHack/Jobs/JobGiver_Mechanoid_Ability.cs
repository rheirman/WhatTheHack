using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    class JobGiver_Mechanoid_Ability : ThinkNode_JobGiver
    {
        float powerDrain = 40f; //Should store this globally. 
        float fuelConsumption = 5f; //Should store this globally. 
        public override Job TryGiveJob(Pawn pawn)
        {
            if(pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestruct) && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
            {
                Thing targetFound = FindTargetFor(pawn);
                if (targetFound != null)
                {
                    Job job = new Job(WTH_DefOf.WTH_Ability_SelfDestruct, targetFound)
                    {
                        count = 150, //should store this in xml. 
                        playerForced = true
                    };
                    return job;
                }
            }
            if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) &&
                pawn.health.summaryHealth.SummaryHealthPercent < 0.8f &&
                !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing) &&
                pawn.TryGetComp<CompRefuelable>() is CompRefuelable comp &&
                pawn.needs.TryGetNeed<Need_Power>() is Need_Power powerNeed &&
                comp.Fuel >= fuelConsumption &&
                powerNeed.CurLevel >= powerDrain
                )
            {
                Job job = new Job(WTH_DefOf.WTH_Ability_Repair, pawn);
                return job;
            }
            if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_OverdriveModule))
            {
                Thing targetFound = FindTargetFor(pawn);

                if(targetFound != null && targetFound.Position.DistanceToSquared(pawn.Position) < 25 * 25 && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Overdrive) && pawn.needs.TryGetNeed<Need_Power>() is Need_Power need)
                {
                    if(need.CurLevelPercentage > 0.5f)
                    {
                        Job job = new Job(WTH_DefOf.WTH_Ability_Overdrive, pawn);
                        return job;
                    }
                }
            }

            return null;
        }

        private static Thing FindTargetFor(Pawn pawn)
        {
            List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
            int min = int.MaxValue;
            Thing targetFound = null;
            for (int i = 0; i < potentialTargetsFor.Count; i++)
            {
                IAttackTarget attackTarget = potentialTargetsFor[i];
                if (!attackTarget.ThreatDisabled(pawn))
                {
                    Thing target = (Thing)attackTarget;
                    int distance = target.Position.DistanceToSquared(pawn.Position);
                    if (distance < min && pawn.CanReach(target, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
                    {
                        min = distance;
                        targetFound = target;
                    }
                }
            }

            return targetFound;
        }
    }
}
