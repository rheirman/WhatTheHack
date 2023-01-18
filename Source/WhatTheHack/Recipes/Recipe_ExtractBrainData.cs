using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WhatTheHack.Recipes;

internal class Recipe_ExtractBrainData : Recipe_Surgery
{
    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        var parts = pawn.health.hediffSet.GetNotMissingParts()
            .Where(bpr => bpr.def == recipe.appliedOnFixedBodyParts.First());
        foreach (var part in parts)
        {
            yield return part;
        }
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);

        if (billDoer != null)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            {
                return;
            }

            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            var md = ThingMaker.MakeThing(WTH_DefOf.WTH_ExtractedBrainData);
            md.stackCount = 3 + pawn.skills.GetSkill(SkillDefOf.Intellectual).Level;
            GenPlace.TryPlaceThing(md, pawn.Position, pawn.Map, ThingPlaceMode.Near);
        }

        var surgicalCut = DamageDefOf.SurgicalCut;
        var amount = 99999f;
        var armorPenetration = 999f;
        pawn.TakeDamage(new DamageInfo(surgicalCut, amount, armorPenetration, -1f, null, part));
        if (MedicalRecipesUtility.IsClean(pawn, part))
        {
            if (pawn.Dead)
            {
                ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.OrganHarvesting);
            }

            ThoughtUtility.GiveThoughtsForPawnOrganHarvested(pawn, billDoer);
        }

        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer) || pawn.Faction == null ||
            billDoer is not { Faction: { } })
        {
            return;
        }

        var arg_120_0 = pawn.Faction;
        var faction = billDoer.Faction;
        var goodwillChange = -15;
        arg_120_0.TryAffectGoodwillWith(faction, goodwillChange, true, true,
            HistoryEventDefOf.PerformedHarmfulSurgery, pawn);
    }
}