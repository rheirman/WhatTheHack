using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;

namespace WhatTheHack.Jobs
{
    class JobDriver_Mechanoid_Rest : JobDriver
    {
        protected Building_MechanoidPlatform MechanoidPlatform
        {
            get
            {
                return (Building_MechanoidPlatform)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }
        public override bool TryMakePreToilReservations()
        {
            return this.pawn.Reserve(this.MechanoidPlatform, this.job, 1, -1, null);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            /*
            Toil goToPlatformToil = new Toil();
            goToPlatformToil.AddPreInitAction(delegate
            {
                Building_Bed bed = (Building_Bed)pawn.CurJob.GetTarget(TargetIndex.A).Thing;
                IntVec3 bedSleepingSlotPosFor = RestUtility.GetBedSleepingSlotPosFor(pawn, bed);
                if (pawn.Position == bedSleepingSlotPosFor)
                {
                    pawn.jobs.curDriver.ReadyForNextToil();
                }
                else
                {
                    Log.Message("start path");
                    pawn.pather.StartPath(RestUtility.GetBedSleepingSlotPosFor(pawn, bed), PathEndMode.OnCell);
                }
            });
            goToPlatformToil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            */
            
            Toil goToPlatform = Toils_Bed.GotoBed(TargetIndex.A);
            yield return goToPlatform;

            Toil layDownToil = Toils_LayDown.LayDown(TargetIndex.A, true, false, false, false);
            layDownToil.defaultCompleteMode = ToilCompleteMode.Never;
            layDownToil.AddPreTickAction(delegate
            {
                pawn.ClearAllReservations();
                if (!pawn.health.hediffSet.HasNaturallyHealingInjury())
                {
                    ReadyForNextToil();
                }
            });
            yield return layDownToil;
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                pawn.jobs.posture = PawnPosture.Standing;
                pawn.Position = MechanoidPlatform.GetSleepingSlotPos(Building_MechanoidPlatform.SLOTINDEX);
                pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
                this.rotateToFace = TargetIndex.C;
            };
            yield return toil;

        }
    }
}
