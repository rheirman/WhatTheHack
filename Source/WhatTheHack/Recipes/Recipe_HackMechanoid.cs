using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using WhatTheHack.Buildings;
using Random = System.Random;

namespace WhatTheHack.Recipes;

public class Recipe_HackMechanoid : Recipe_Hacking
{
    protected new bool allowMultipleParts = false;

    protected override bool IsValidPawn(Pawn pawn)
    {
        return pawn.Faction != Faction.OfPlayer;
    }

    protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        pawn.health.AddHediff(WTH_DefOf.WTH_BackupBattery, pawn.TryGetReactor());
        pawn.SetFaction(Faction.OfPlayer);
        if (pawn.relations == null)
        {
            pawn.relations = new Pawn_RelationsTracker(pawn);
        }

        if (pawn.story == null)
        {
            pawn.story = new Pawn_StoryTracker(pawn);
        }

        if (pawn.ownership == null)
        {
            pawn.ownership = new Pawn_Ownership(pawn);
        }

        var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);

        Utilities.InitWorkTypesAndSkills(pawn, pawnData);

        pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
        Find.LetterStack.ReceiveLetter("WTH_Letter_Success_Label".Translate(),
            "WTH_Letter_Success_Label_Description".Translate(billDoer.Name.ToStringShort, pawn.Name),
            LetterDefOf.PositiveEvent, pawn);
        billDoer.jobs.jobQueue.EnqueueFirst(new Job(WTH_DefOf.WTH_ClearHackingTable, pawn, pawn.CurrentBed())
            { count = 1 });
        LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Power, OpportunityType.Important);
        LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Maintenance, OpportunityType.Important);
        LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Concept_MechanoidParts, OpportunityType.Important);
    }


    protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, Random r)
    {
        int[] chances =
        {
            Base.failureChanceHackPoorly, Base.failureChanceCauseRaid, Base.failureChanceShootRandomDirection,
            Base.failureChanceHealToStanding, Base.failureChanceNothing
        };
        var totalChance = chances.Sum();
        var randInt = r.Next(1, totalChance);
        Action<Pawn, BodyPartRecord, RecipeDef>[] functions =
        {
            RecipeUtility.HackPoorly, RecipeUtility.CauseMechanoidRaidByHackingFailure,
            RecipeUtility.ShootRandomDirection, RecipeUtility.HealToStanding, RecipeUtility.Nothing
        };
        var acc = 0;
        for (var i = 0; i < chances.Length; i++)
        {
            if (randInt < acc + chances[i])
            {
                functions[i].Invoke(hackee, part, recipe);
                break;
            }

            acc += chances[i];
        }

        if (hackee.Downed && !hackee.IsHacked())
        {
            ((Building_HackingTable)hackee.CurrentBed()).TryAddPawnForModification(hackee, recipe);
        }
    }

    //Used to make hacking more powerful mechs more difficult. Capped at 1000 points. At this value, hacking is 50% more difficult.  
    private static float CalcCombatPowerFactorCapped(Pawn hackee)
    {
        var combatPowerFactor = Mathf.Min(hackee.kindDef.combatPower / 1000, 1.0f);
        var maxDifficultyPentaly = 0.5f;
        var combatPowerFactorCapped = 1 - (maxDifficultyPentaly * combatPowerFactor);
        return combatPowerFactorCapped;
    }
}