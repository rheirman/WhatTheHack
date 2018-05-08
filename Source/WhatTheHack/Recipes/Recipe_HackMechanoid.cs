using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

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
    }
}
