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
    class JobDriver_HackDownedMecha : JobDriver
    {
        protected Pawn Target => (Pawn)this.job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            int duration = (int)(3000 + (1f / this.pawn.GetStatValue(WTH_DefOf.WTH_HackingMaintenanceSpeed, true)) * 500f);

            EffecterDef effect = DefDatabase<EffecterDef>.AllDefs.FirstOrDefault((EffecterDef ed) => ed.defName == "Repair");

            yield return Toils_General.Wait(duration, TargetIndex.None).WithProgressBarToilDelay(TargetIndex.A, false, -0.5f).WithEffect(effect, TargetIndex.A);
            Toil finalizeHacking = new Toil
            {
                initAction = delegate
                {
                    BodyPartRecord bodyPart = Target.health.hediffSet.GetNotMissingParts()
                                                                     .FirstOrDefault(bpr => bpr.def.defName == "ArtificialBrain"
                                                                                            || bpr.def.defName == "Brain"
                                                                                            || bpr.def.defName == "MechanicalHead");
                    Target.health.AddHediff(WTH_DefOf.WTH_TargetingHacked, bodyPart);
                    Target.health.AddHediff(WTH_DefOf.WTH_BackupBattery, Target.TryGetReactor());
                    Target.SetFaction(Faction.OfPlayer);
                    if (Target.relations == null)
                        Target.relations = new Pawn_RelationsTracker(Target);
                    if (Target.story == null)
                        Target.story = new Pawn_StoryTracker(Target);
                    if (Target.ownership == null)
                        Target.ownership = new Pawn_Ownership(Target);

                    Target.Name = PawnBioAndNameGenerator.GeneratePawnName(Target, NameStyle.Full);

                    pawn.skills.Learn(SkillDefOf.Crafting, Target.kindDef.combatPower, false);
                    pawn.skills.Learn(SkillDefOf.Intellectual, Target.kindDef.combatPower, false);
                    Messages.Message($"Mechanoid hacked", pawn, MessageTypeDefOf.PositiveEvent);
                },
                defaultCompleteMode = ToilCompleteMode.Instant,
            };
            yield return finalizeHacking;

        }
    }
}
