using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs;

internal class JobDriver_Mechanoid_Rest : JobDriver
{
    protected Building_Bed RestingPlace => (Building_Bed)job.GetTarget(TargetIndex.A).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (pawn.HasReserved(RestingPlace))
        {
            return true;
        }

        //pawn.reserve
        var result = pawn.Reserve(RestingPlace, job);
        return result;
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        //this.AddFinishAction(new Action(delegate { Log.Message("finish action called for job!");  }));
        this.FailOnDespawnedOrNull(TargetIndex.A);
        if (RestingPlace is Building_BaseMechanoidPlatform)
        {
            yield return Toils_Bed.ClaimBedIfNonMedical(TargetIndex.A);
            yield return GoToPlatform(TargetIndex.A);
        }

        //goToPlatform.AddPreInitAction(new Action(delegate { Log.Message("first toil pre-initaction"); }));
        var toil = new Toil
        {
            defaultCompleteMode = ToilCompleteMode.Never,
            initAction = delegate
            {
                if (pawn.health.hediffSet.HasNaturallyHealingInjury() || pawn.OnHackingTable())
                {
                    pawn.jobs.posture = PawnPosture.LayingInBed;
                }

                job.expiryInterval = 50;
                job.checkOverrideOnExpire = true;
                pawn.ClearAllReservations();
                pawn.Position = RestingPlace.GetSleepingSlotPos(RestingPlace is Building_HackingTable
                    ? Building_HackingTable.SLOTINDEX
                    : Building_BaseMechanoidPlatform.SLOTINDEX);
            },
            tickAction = delegate
            {
                if (RestingPlace is Building_BaseMechanoidPlatform && pawn.ownership.OwnedBed != RestingPlace)
                {
                    ReadyForNextToil();
                }

                if (RestingPlace.TryGetComp<CompAssignableToPawn_Bed>() is { } compAssignable &&
                    compAssignable.AssignedPawns.FirstOrDefault(p => p != pawn) is { })
                {
                    pawn.ownership.UnclaimBed();
                    ReadyForNextToil();
                }

                if (pawn.health.hediffSet.HasNaturallyHealingInjury() || pawn.OnHackingTable())
                {
                    pawn.jobs.posture = PawnPosture.LayingInBed;
                }
                else
                {
                    pawn.jobs.posture = PawnPosture.Standing;
                    RotateToSouth();
                }
            }
        };

        yield return toil;
    }


    public static Toil GoToPlatform(TargetIndex bedIndex)
    {
        var gotoBed = new Toil();
        gotoBed.initAction = delegate
        {
            var actor = gotoBed.actor;
            var bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
            var bedSleepingSlotPosFor = RestUtility.GetBedSleepingSlotPosFor(actor, bed);
            if (actor.Position == bedSleepingSlotPosFor)
            {
                actor.jobs.curDriver.ReadyForNextToil();
            }
            else
            {
                actor.pather.StartPath(bedSleepingSlotPosFor, PathEndMode.OnCell);
            }
        };
        gotoBed.tickAction = delegate
        {
            var actor = gotoBed.actor;
            var building_Bed = (Building_Bed)actor.CurJob.GetTarget(bedIndex).Thing;
            var curOccupantAt = building_Bed.GetCurOccupantAt(actor.pather.Destination.Cell);
            if (curOccupantAt != null && curOccupantAt != actor)
            {
                actor.pather.StartPath(RestUtility.GetBedSleepingSlotPosFor(actor, building_Bed), PathEndMode.OnCell);
            }
        };
        gotoBed.defaultCompleteMode = ToilCompleteMode.PatherArrival;
        gotoBed.FailOnPlatformNoLongerUsable(bedIndex);
        //gotoBed.FailOnBedNoLongerUsable(bedIndex);
        return gotoBed;
    }


    private void RotateToSouth()
    {
        pawn.CurJob.SetTarget(TargetIndex.C,
            new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
        rotateToFace = TargetIndex.C;
    }
}