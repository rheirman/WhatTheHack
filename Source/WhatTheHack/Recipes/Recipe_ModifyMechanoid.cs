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
    public class Recipe_ModifyMechanoid : Recipe_Hacking
    {
        protected override bool CanApplyOn(Pawn pawn)
        {
            bool hasRequiredHediff = true;
            bool hasRequiredBodySize = true;
            if (recipe.HasModExtension<DefModExtension_Recipe>()) {

                DefModExtension_Recipe ext = recipe.GetModExtension<DefModExtension_Recipe>();
                if(ext.requiredHediff != null && !pawn.health.hediffSet.HasHediff(ext.requiredHediff))
                {
                    hasRequiredHediff = false;
                }
                if(pawn.BodySize < ext.minBodySize)
                {
                    hasRequiredBodySize = false;
                }
            }

            return pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff) && hasRequiredHediff && hasRequiredBodySize;
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            if(pawn.BillStack.Bills.Count <= 1)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, Random r)
        {
            base.HackingFailEvent(hacker, hackee, part, r);
            Messages.Message("MessageMedicalOperationFailureMinor".Translate(new object[]{ hacker.LabelShort, hackee.LabelShort }), hackee, MessageTypeDefOf.NegativeHealthEvent, true);
            foreach (IngredientCount ic in recipe.ingredients)
            {
                Thing t = ThingMaker.MakeThing(ic.filter.AnyAllowedDef, null);
                GenSpawn.Spawn(t, hackee.Position, hackee.Map);
            }
            ((Building_HackingTable)hackee.CurrentBed()).TryAddPawnForModification(hackee, recipe);
        }
    }
}
