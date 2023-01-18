using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs;

internal class JobDriver_ClearHackingTable : JobDriver
{
    protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    protected Building_HackingTable HackingTable => (Building_HackingTable)job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        //return true;
        return pawn.Reserve(Takee, job) && pawn.Reserve(HackingTable, job);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOnAggroMentalState(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B)
            .FailOn(() => !pawn.CanReach(Takee, PathEndMode.OnCell, Danger.Deadly))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        var toil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = delegate
            {
                Takee.jobs.EndCurrentJob(JobCondition.InterruptForced);
                Takee.health.surgeryBills.Clear();
            }
        };
        yield return toil;
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Misc.FindRandomAdjacentReachableCell(TargetIndex.B, TargetIndex.C);
        yield return Toils_Goto.GotoThing(TargetIndex.C, PathEndMode.OnCell);

        //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A)
        yield return new Toil
        {
            initAction = delegate
            {
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Direct, out _);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}