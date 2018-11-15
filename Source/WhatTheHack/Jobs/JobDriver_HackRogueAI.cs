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
    class JobDriver_HackRogueAI : JobDriver
    {
        protected Building_RogueAI RogueAI
        {
            get
            {
                return (Building_RogueAI)this.job.targetA.Thing;
            }
        }
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(RogueAI, job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => RogueAI.goingRogue == false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
            int duration = (int)(3000 + (1f / this.pawn.GetStatValue(WTH_DefOf.WTH_HackingMaintenanceSpeed, true)) * 500f);
            EffecterDef effect = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault((EffecterDef ed) => ed.defName == "Repair");
            yield return Toils_General.Wait(duration, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).WithEffect(effect, TargetIndex.A);
            Toil finalizeHacking = new Toil() {
                defaultCompleteMode = ToilCompleteMode.Instant,
                initAction = new Action(delegate {
                    Messages.Message("WTH_Message_HackedRogueAI".Translate(this.pawn.Name.ToStringShort), new RimWorld.Planet.GlobalTargetInfo(this.pawn.Position, this.Map), MessageTypeDefOf.PositiveEvent);
                    RogueAI.StopGoingRogue();
                })
            };
            yield return finalizeHacking;

        }
    }
}
