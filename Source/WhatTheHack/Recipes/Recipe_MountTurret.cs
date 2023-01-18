using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using WhatTheHack.Comps;

namespace WhatTheHack.Recipes;

public class Recipe_MountTurret : Recipe_Hacking
{
    protected override bool IsValidPawn(Pawn pawn)
    {
        return pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff) &&
               pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_TurretModule);
    }

    public override bool CanApplyOn(Pawn pawn, out string reason)
    {
        reason = "";
        if (recipe.HasModExtension<DefModExtension_Recipe>())
        {
            var ext = recipe.GetModExtension<DefModExtension_Recipe>();
            if (ext.requiredHediff != null && !pawn.health.hediffSet.HasHediff(ext.requiredHediff))
            {
                reason = "WTH_Reason_MissingHediff".Translate(ext.requiredHediff.label);
                return false;
            }
        }

        var isArtillery = recipe.ingredients.FirstOrDefault(ic =>
            ic.FixedIngredient is { placeWorkers: { } } td &&
            td.placeWorkers.FirstOrDefault(t => t == typeof(PlaceWorker_NotUnderRoof)) != null) != null;
        var mortarResearchCompleted =
            DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(rp =>
                rp == WTH_DefOf.WTH_TurretModule_Mortars && rp.IsFinished) != null;
        var isTurretGun = !isArtillery;
        var turretGunResearchCompleted =
            DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(rp =>
                rp == WTH_DefOf.WTH_TurretModule_GunTurrets && rp.IsFinished) != null;

        if (isArtillery && !mortarResearchCompleted)
        {
            reason = "WTH_Reason_MortarResearch".Translate();
            return false;
        }

        if (!isTurretGun || turretGunResearchCompleted)
        {
            return true;
        }

        reason = "WTH_Reason_TurretResearch".Translate();
        return false;
    }

    protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        var oldThing = ingredients.FirstOrDefault(t => t.GetInnerIfMinified().TryGetComp<CompMountable>() != null);
        var comp = oldThing.GetInnerIfMinified().TryGetComp<CompMountable>();
        comp.MountToPawn(pawn);
    }
}