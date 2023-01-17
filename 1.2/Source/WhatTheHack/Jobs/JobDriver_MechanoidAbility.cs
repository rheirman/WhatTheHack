using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Needs;

namespace WhatTheHack.Jobs
{
    public class JobDriver_MechanoidAbility : JobDriver
    {
        public bool finished = false;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        private Pawn TargetPawn
        {
            get
            {
                return (Pawn)TargetA;
            }
        }
        
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if(TargetA != null && job.def.GetModExtension<DefModExtension_Ability>() is DefModExtension_Ability modExt)
            {
                this.FailOnDespawnedOrNull(TargetIndex.A);
                //this.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
                this.FailOn(() => pawn.Dead);
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
                if (TargetA != pawn && !TargetPawn.Faction.HostileTo(pawn.Faction))
                {
                    yield return Toils_General.WaitWith(TargetIndex.A, modExt.warmupTicks, true, true);
                }
                else
                {
                    yield return Toils_General.Wait(modExt.warmupTicks).WithProgressBarToilDelay(TargetIndex.A);
                }
                yield return Toils_General.Do(delegate
                {
                    PerformAbility(modExt);
                    finished = true;
                });
            }
        }

        protected virtual void PerformAbility(DefModExtension_Ability modExt)
        {
            if (modExt.fuelDrain > 0 && pawn.TryGetComp<CompRefuelable>() is CompRefuelable refuelableComp)
            {
                refuelableComp.ConsumeFuel(modExt.fuelDrain);
            }
            if (modExt.powerDrain > 0 && pawn.needs.TryGetNeed<Need_Power>() is Need_Power powerNeed)
            {
                powerNeed.CurLevel -= modExt.powerDrain;
            }
            if (Rand.Chance(modExt.failChance))
            {
                FailAbility(modExt);
                return;
            }
            if (modExt.hediffSelf != null)
            {
                pawn.health.AddHediff(modExt.hediffSelf);
            }
            if (modExt.hediffTarget != null && TargetPawn != null)
            {
                TargetPawn.health.AddHediff(modExt.hediffTarget);
            }
        }
        protected virtual void FailAbility(DefModExtension_Ability modExt)
        {

        }
    }
}
