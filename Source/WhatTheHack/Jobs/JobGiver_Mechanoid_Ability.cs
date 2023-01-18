using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs;

internal class JobGiver_Mechanoid_Ability : ThinkNode_JobGiver
{
    private readonly float fuelConsumption = 5f; //Should store this globally. 
    private readonly float powerDrain = 40f; //Should store this globally. 

    public override Job TryGiveJob(Pawn pawn)
    {
        Thing targetFound;
        Job job;
        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestruct) &&
            pawn.health.summaryHealth.SummaryHealthPercent < 0.5f)
        {
            targetFound = FindTargetFor(pawn);
            if (targetFound != null)
            {
                job = new Job(WTH_DefOf.WTH_Ability_SelfDestruct, targetFound)
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
            pawn.TryGetComp<CompRefuelable>() is { } comp &&
            pawn.needs.TryGetNeed<Need_Power>() is { } powerNeed &&
            comp.Fuel >= fuelConsumption &&
            powerNeed.CurLevel >= powerDrain
           )
        {
            job = new Job(WTH_DefOf.WTH_Ability_Repair, pawn);
            return job;
        }

        if (!pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_OverdriveModule))
        {
            return null;
        }

        targetFound = FindTargetFor(pawn);

        if (targetFound == null || targetFound.Position.DistanceToSquared(pawn.Position) >= 25 * 25 ||
            pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Overdrive) ||
            pawn.needs.TryGetNeed<Need_Power>() is not { } need)
        {
            return null;
        }

        if (!(need.CurLevelPercentage > 0.5f))
        {
            return null;
        }

        job = new Job(WTH_DefOf.WTH_Ability_Overdrive, pawn);
        return job;
    }

    private static Thing FindTargetFor(Pawn pawn)
    {
        var potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
        var min = int.MaxValue;
        Thing targetFound = null;
        foreach (var attackTarget in potentialTargetsFor)
        {
            if (attackTarget.ThreatDisabled(pawn))
            {
                continue;
            }

            var target = (Thing)attackTarget;
            var distance = target.Position.DistanceToSquared(pawn.Position);
            if (distance >= min || !pawn.CanReach(target, PathEndMode.OnCell, Danger.Deadly))
            {
                continue;
            }

            min = distance;
            targetFound = target;
        }

        return targetFound;
    }
}