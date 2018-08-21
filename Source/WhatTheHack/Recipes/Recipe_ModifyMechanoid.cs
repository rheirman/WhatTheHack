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
    class Recipe_ModifyMechanoid : Recipe_Hacking
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
            if (this.recipe.addsHediff == WTH_DefOf.WTH_RepairModule)
            {
                pawn.InitializeComps();
            }
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }
}
