using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs;

internal class JobDriver_HackRogueAI : JobDriver
{
    protected Building_RogueAI RogueAI => (Building_RogueAI)job.targetA.Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(RogueAI, job, 1, -1, null, errorOnFailed))
        {
            return false;
        }

        return true;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOn(() => RogueAI.goingRogue == false);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
        var duration = (int)(3000 + (1f / pawn.GetStatValue(WTH_DefOf.WTH_HackingMaintenanceSpeed) * 500f));
        var effect = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault(ed => ed.defName == "Repair");
        yield return Toils_General.Wait(duration).FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch)
            .WithProgressBarToilDelay(TargetIndex.A).WithEffect(effect, TargetIndex.A);
        var finalizeHacking = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                Messages.Message("WTH_Message_HackedRogueAI".Translate(pawn.Name.ToStringShort),
                    new GlobalTargetInfo(pawn.Position, Map), MessageTypeDefOf.PositiveEvent);
                RogueAI.StopGoingRogue();
            }
        };
        yield return finalizeHacking;
    }
}