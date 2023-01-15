using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs;

internal class WorkGiver_HackRogueAI : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        var store = Base.Instance.GetExtendedDataStorage();

        var mapData = store?.GetExtendedDataFor(pawn.Map);
        return mapData?.rogueAI is not { goingRogue: true };
    }


    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is not Building_RogueAI { goingRogue: true } rogueAI)
        {
            return false;
        }

        LocalTargetInfo target = rogueAI;
        return pawn.CanReserveAndReach(target, PathEndMode.ClosestTouch, Danger.Deadly, 10, 1, null, forced);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t is Building_RogueAI rogueAI)
        {
            return new Job(WTH_DefOf.WTH_HackRogueAI, rogueAI);
        }

        return null;
    }

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }
}