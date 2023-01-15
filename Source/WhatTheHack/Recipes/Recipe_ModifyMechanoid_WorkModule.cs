using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WhatTheHack.Recipes;

internal class Recipe_ModifyMechanoid_WorkModule : Recipe_ModifyMechanoid
{
    protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        base.PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
        var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
        if (bill.recipe.addsHediff.GetModExtension<DefModExtension_Hediff_WorkModule>() is not { } modExt)
        {
            return;
        }

        foreach (var workType in modExt.workTypes)
        {
            pawnData.workTypes.Add(workType);
            pawn.workSettings.SetPriority(workType, 3);
            if (modExt.skillLevel <= 0)
            {
                continue;
            }

            foreach (var skillDef in workType.relevantSkills)
            {
                pawn.skills.GetSkill(skillDef).Level = modExt.skillLevel;
            }
        }
    }
}