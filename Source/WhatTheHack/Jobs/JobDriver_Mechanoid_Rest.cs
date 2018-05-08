using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

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
            Toil toil = Toils_LayDown.LayDown(TargetIndex.A, true, false, false, false);
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}
