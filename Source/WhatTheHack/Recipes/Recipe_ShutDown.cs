using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WhatTheHack.Recipes;

internal class Recipe_ShutDown : RecipeWorker
{
    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        var brain = pawn.health.hediffSet.GetBrain();
        if (brain != null)
        {
            yield return brain;
        }
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        pawn.health.AddHediff(recipe.addsHediff, part);
        ThoughtUtility.GiveThoughtsForPawnExecuted(pawn, billDoer, PawnExecutionKind.GenericHumane);
    }
}