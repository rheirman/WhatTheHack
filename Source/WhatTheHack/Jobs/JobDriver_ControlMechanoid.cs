using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace WhatTheHack.Jobs
{
    class JobDriver_ControlMechanoid : JobDriver
    {
        public override bool TryMakePreToilReservations()
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil toil = new Toil();
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return toil;
        }
    }
}
