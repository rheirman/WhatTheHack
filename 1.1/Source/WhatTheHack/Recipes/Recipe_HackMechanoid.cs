using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Recipes
{
    public class Recipe_HackMechanoid : Recipe_Hacking
    {
        protected new bool allowMultipleParts = false;
        protected override bool IsValidPawn(Pawn pawn)
        {
            return pawn.Faction != Faction.OfPlayer;
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
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
            if(pawn.ownership == null)
            {
                pawn.ownership = new Pawn_Ownership(pawn);
            }

            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full);
            Find.LetterStack.ReceiveLetter("WTH_Letter_Success_Label".Translate(), "WTH_Letter_Success_Label_Description".Translate(new object[]{billDoer.Name.ToStringShort, pawn.Name}), LetterDefOf.PositiveEvent, pawn);
            billDoer.jobs.jobQueue.EnqueueFirst(new Job(WTH_DefOf.WTH_ClearHackingTable, pawn, pawn.CurrentBed()) {count = 1});
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Power, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Maintenance, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Concept_MechanoidParts, OpportunityType.Important);

        }


        protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, System.Random r)
        {
            int[] chances = { Base.failureChanceHackPoorly, Base.failureChanceCauseRaid, Base.failureChanceShootRandomDirection, Base.failureChanceHealToStanding, Base.failureChanceNothing };
            int totalChance = chances.Sum();
            int randInt = r.Next(1, totalChance);
            Action<Pawn, BodyPartRecord, RecipeDef>[] functions = { RecipeUtility.HackPoorly, RecipeUtility.CauseMechanoidRaidByHackingFailure, RecipeUtility.ShootRandomDirection, RecipeUtility.HealToStanding, RecipeUtility.Nothing };
            int acc = 0;
            for (int i = 0; i < chances.Count(); i++)
            {
                if (randInt < ((acc + chances[i])))
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
            float combatPowerFactor = Mathf.Min(hackee.kindDef.combatPower / 1000, 1.0f);
            float maxDifficultyPentaly = 0.5f;
            float combatPowerFactorCapped = 1 - maxDifficultyPentaly * combatPowerFactor;
            return combatPowerFactorCapped;
        }


    }
    
}
