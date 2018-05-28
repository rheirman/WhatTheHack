using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.Duties;

namespace WhatTheHack.Recipes
{
    class Recipe_ImplantAI : Recipe_Surgery
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
                //Let random bad events happen when hacking fails
                if (base.CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                {
                    return;
                }
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                //Find.LetterStack.ReceiveLetter("LetterSuccess_Label".Translate(), "LetterSuccess_Label_Description".Translate(), LetterDefOf.PositiveEvent, pawn);
            }
            pawn.health.AddHediff(this.recipe.addsHediff, part, null);
        }
    }
}
