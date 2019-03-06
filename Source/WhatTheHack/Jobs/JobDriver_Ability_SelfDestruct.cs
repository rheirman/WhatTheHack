using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;
using WhatTheHack.Storage;

namespace WhatTheHack.Jobs
{
    class JobDriver_Ability_SelfDestruct : JobDriver_Ability
    {
        protected override IEnumerable<Toil> MakeNewToils()
        {
            IEnumerable<Toil> toils = base.MakeNewToils();
            foreach(Toil toil in toils)
            {
                yield return toil;
            }
            Toil selfDestructToil = new Toil();
            selfDestructToil.defaultCompleteMode = ToilCompleteMode.Instant;
            selfDestructToil.initAction = new Action(delegate {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                pawnData.shouldExplodeNow = true;
            });
            yield return selfDestructToil;

        }
    }
}
