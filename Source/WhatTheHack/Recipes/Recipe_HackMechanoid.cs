using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Duties;

namespace WhatTheHack.Recipes
{
    class Recipe_HackMechanoid : Recipe_Surgery
    {

        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
            if (brain != null && !pawn.health.hediffSet.HasHediff(recipe.addsHediff))
            {
                yield return brain;
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if (billDoer != null)
            {
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    //TODO actions here when hacking fails!
                    Verb verb = null;

                    foreach (Verb v in pawn.equipment.AllEquipmentVerbs)
                    {
                        if (!v.IsMeleeAttack)
                        {
                            verb = v;
                        }
                    }
                    if(verb == null)
                    {
                        Log.Message("verb was null, miemie");
                        return;
                    }
                    Traverse.Create(verb).Field("currentTarget").SetValue(new LocalTargetInfo(billDoer.Position));
                    Traverse.Create(verb).Method("TryCastNextBurstShot").GetValue();

                    //HealUntilStanding(pawn);
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
            }
            pawn.health.AddHediff(this.recipe.addsHediff, part, null);
            pawn.SetFaction(Faction.OfPlayer);
            if(pawn.playerSettings == null)
            {
                Log.Message("pawn playersettings were null");

            }
            else
            {
                Log.Message("pawn playersettings medcare: " + pawn.playerSettings.medCare.GetLabel());
                Log.Message("pawn playersettings medcare tostring: " + pawn.playerSettings.medCare);

            }

            if (pawn.jobs.curDriver != null)
            {
                pawn.jobs.curDriver.layingDown = LayingDownState.LayingSurface;
            }
            if (pawn.story == null)
            {
                pawn.story = new Pawn_StoryTracker(pawn);
            }

        }

        private static void HealUntilStanding(Pawn pawn)
        {
            bool shouldStop = false;
            while (!shouldStop)
            {
                Hediff_Injury hediff_Injury = pawn.health.hediffSet.GetHediffs<Hediff_Injury>().Where(new Func<Hediff_Injury, bool>(HediffUtility.CanHealNaturally)).RandomElement<Hediff_Injury>();
                if (hediff_Injury == null || !pawn.Downed)
                {
                    shouldStop = true;
                    continue;
                }
                hediff_Injury.Heal(10);
            }
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            if (pawn.GetLord() == null || pawn.GetLord().LordJob == null)
            {
                LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), pawn.Map, new List<Pawn> { pawn });
            }
            else
            {
                Log.Message("lord was not null");
            }
        }
    }
    
}
