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
    public class Recipe_MountTurret : Recipe_Hacking
    {
        protected override bool IsValidPawn(Pawn pawn)
        {
            return pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff) && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TurretModule);
        }

        public override bool CanApplyOn(Pawn pawn, out string reason)
        {
            reason = "";
            if (recipe.HasModExtension<DefModExtension_Recipe>())
            {

                DefModExtension_Recipe ext = recipe.GetModExtension<DefModExtension_Recipe>();
                if (ext.requiredHediff != null && !pawn.health.hediffSet.HasHediff(ext.requiredHediff))
                {
                    reason = "WTH_Reason_MissingHediff".Translate(ext.requiredHediff.label);
                    return false;
                }
            }
            bool isArtillery = recipe.ingredients.FirstOrDefault((IngredientCount ic) => ic.FixedIngredient is ThingDef td && td.placeWorkers != null && td.placeWorkers.FirstOrDefault((Type t) => t == typeof(PlaceWorker_NotUnderRoof)) != null) != null;
            bool mortarResearchCompleted = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault((ResearchProjectDef rp) => rp == WTH_DefOf.WTH_TurretModule_Mortars && rp.IsFinished) != null;
            bool isTurretGun = !isArtillery;
            bool turretGunResearchCompleted = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault((ResearchProjectDef rp) => rp == WTH_DefOf.WTH_TurretModule_GunTurrets && rp.IsFinished) != null;

            if(isArtillery && !mortarResearchCompleted)
            {
                reason = "WTH_Reason_MortarResearch".Translate();
                return false;
            }
            if(isTurretGun && !turretGunResearchCompleted)
            {
                reason = "WTH_Reason_TurretResearch".Translate();
                return false;
            }
            return true;
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            Thing oldThing = ingredients.FirstOrDefault((Thing t) => (t.GetInnerIfMinified().TryGetComp<CompMountable>() != null));
            CompMountable comp = oldThing.GetInnerIfMinified().TryGetComp<CompMountable>();
            comp.MountToPawn(pawn);
        }
    }
}
