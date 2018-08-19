using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using WhatTheHack.Comps;

namespace WhatTheHack.Recipes
{
    class Recipe_MountTurret : Recipe_Hacking
    {
        protected override bool CanApplyOn(Pawn pawn)
        {
            bool hasRequiredHediff = true;
            if (recipe.HasModExtension<DefModExtension_Recipe>())
            {

                DefModExtension_Recipe ext = recipe.GetModExtension<DefModExtension_Recipe>();
                if (ext.requiredHediff != null && !pawn.health.hediffSet.HasHediff(ext.requiredHediff))
                {
                    hasRequiredHediff = false;
                }
            }
            return pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff) && hasRequiredHediff;
        }
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            Thing oldThing = ingredients.FirstOrDefault((Thing t) => (t.GetInnerIfMinified().TryGetComp<CompMountable>() != null));
            CompMountable comp = oldThing.GetInnerIfMinified().TryGetComp<CompMountable>();
            comp.MountToPawn(pawn);
            //GenSpawn.Spawn(oldThing.GetInnerIfMinified(), pawn.Position, pawn.Map);
        }

    }
}
