using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_Ability : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            if(TargetA != null)
            {
                this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
                this.FailOnNotCasualInterruptible(TargetIndex.A);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                yield return Toils_General.WaitWith(TargetIndex.A, job.expiryInterval, true, true);
            }
        }
    }
}
