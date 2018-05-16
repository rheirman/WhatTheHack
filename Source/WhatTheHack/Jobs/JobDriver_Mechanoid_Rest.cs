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
        public override bool TryMakePreToilReservations()
        {
            return true;
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
                if (!pawn.health.hediffSet.HasNaturallyHealingInjury())
                {
                    Log.Message("ready for next toil!");
                    ReadyForNextToil();
                }
            });
            yield return layDownToil;
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.initAction = delegate
            {
                Log.Message("Starting next toil");
                if(TargetA.Thing is Building_HackingTable)
                    pawn.Position = ((Building_Bed) TargetA.Thing).GetSleepingSlotPos(Building_HackingTable.SLOTINDEX);
                if (TargetA.Thing is Building_HackingTable)
                    pawn.Position = ((Building_MechanoidPlatform)TargetA.Thing).GetSleepingSlotPos(Building_MechanoidPlatform.SLOTINDEX);
                pawn.CurJob.SetTarget(TargetIndex.C, new LocalTargetInfo(new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z - 1)));
                this.rotateToFace = TargetIndex.C;


            };
            yield return toil;

        }
    }
}
