using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Recipes
{
    class Recipe_InduceEmergencySignal : Recipe_Hacking
    {
        protected override bool IsValidPawn(Pawn pawn)
        {
            return !pawn.IsHacked();
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.ApplyOnPawn(pawn, part, billDoer, ingredients, bill);
        }
        protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, System.Random r)
        {
            int[] chances = { Base.failureChanceIntRaidTooLarge,  Base.failureChanceShootRandomDirection, Base.failureChanceHealToStanding, Base.failureChanceNothing };
            int totalChance = chances.Sum();
            int randInt = r.Next(1, totalChance);
            Action<Pawn, BodyPartRecord, RecipeDef>[] functions = { RecipeUtility.CauseIntendedMechanoidRaidTooLarge, RecipeUtility.ShootRandomDirection, RecipeUtility.HealToStanding, RecipeUtility.Nothing };
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
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            RecipeUtility.CauseIntendedMechanoidRaid(pawn, part, recipe);
            int cooldown = new IntRange(GenDate.TicksPerQuadrum/2, GenDate.TicksPerQuadrum).RandomInRange;
            Base.Instance.GetExtendedDataStorage().lastEmergencySignalCooldown = cooldown;
        }
    }
}
