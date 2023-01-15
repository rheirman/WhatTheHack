using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs;

internal class WorkGiver_PerformMaintenance : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        if (pawn.skills == null || pawn.Faction != Faction.OfPlayer)
        {
            return true;
        }

        return pawn.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.IsHacked() && PawnNeedsMaintenance(p)) == null;
    }

    private bool PawnNeedsMaintenance(Pawn mech)
    {
        if (mech.needs == null ||
            mech.needs.TryGetNeed(WTH_DefOf.WTH_Mechanoid_Maintenance) is not Need_Maintenance need)
        {
            return false;
        }

        return need.CurLevel < GetThresHold(need);
    }

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Pawn { needs: { } } targetPawn ||
            targetPawn.needs.TryGetNeed<Need_Maintenance>() is not { } need ||
            !targetPawn.OnBaseMechanoidPlatform())
        {
            return false;
        }

        LocalTargetInfo target = targetPawn;
        return pawn.CanReserveAndReach(target, PathEndMode.ClosestTouch, Danger.Deadly, 10, 1, null, forced) &&
               need.CurLevel < GetThresHold(need) &&
               FindMechanoidParts(pawn, targetPawn, forced) != null;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var targetPawn = t as Pawn;
        var thing = FindMechanoidParts(pawn, targetPawn, forced);
        return new Job(WTH_DefOf.WTH_PerformMaintenance, targetPawn, thing);
    }

    protected virtual float GetThresHold(Need_Maintenance need)
    {
        return need.MaxLevel * 0.5f;
    }

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    private static Thing FindMechanoidParts(Pawn hacker, Pawn targetPawn, bool forced)
    {
        if (targetPawn.needs.TryGetNeed<Need_Maintenance>() == null)
        {
            return null;
        }

        bool Predicate(Thing m)
        {
            return !m.IsForbidden(hacker) &&
                   hacker.CanReserveAndReach(m, PathEndMode.ClosestTouch, Danger.Deadly, 10, 1, null, forced);
        }

        var position = targetPawn.Position;
        var map = targetPawn.Map;
        var searchSet = targetPawn.Map.listerThings.ThingsOfDef(WTH_DefOf.WTH_MechanoidParts);
        var peMode = PathEndMode.ClosestTouch;
        var traverseParams = TraverseParms.For(hacker);

        return GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f,
            Predicate);
    }
}