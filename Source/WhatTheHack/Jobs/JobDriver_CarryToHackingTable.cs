using System.Collections.Generic;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs;

internal class JobDriver_CarryToHackingTable : JobDriver
{
    protected Pawn Takee => (Pawn)job.GetTarget(TargetIndex.A).Thing;

    protected Building_HackingTable HackingTable => (Building_HackingTable)job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Takee, job) && pawn.Reserve(HackingTable, job);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedOrNull(TargetIndex.A);
        this.FailOnDestroyedOrNull(TargetIndex.B);
        this.FailOnAggroMentalState(TargetIndex.A);
        //this.FailOn(() => !this.DropPod.Accepts(this.Takee));
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() =>
                HackingTable.GetCurOccupant(Building_HackingTable.SLOTINDEX) != null &&
                HackingTable.GetCurOccupant(Building_HackingTable.SLOTINDEX).OnHackingTable())
            .FailOn(() => !pawn.CanReach(Takee, PathEndMode.OnCell, Danger.Deadly))
            .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);

        //yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.A)
        yield return new Toil
        {
            initAction = delegate
            {
                pawn.carryTracker.TryDropCarriedThing(HackingTable.GetSleepingSlotPos(Building_HackingTable.SLOTINDEX),
                    ThingPlaceMode.Direct, out _);
                pawn.ClearAllReservations();
                HackingTable.TryAddPawnForModification(Takee, WTH_DefOf.WTH_HackMechanoid);
                Takee.Position = HackingTable.GetSleepingSlotPos(Building_HackingTable.SLOTINDEX);
                var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(Takee);
                pawnData.isActive = false;
                pawnData.canWorkNow = false;
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}