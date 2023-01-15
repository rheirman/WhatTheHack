using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WhatTheHack.Recipes;

internal class Recipe_InduceEmergencySignal : Recipe_Hacking
{
    protected override bool IsValidPawn(Pawn pawn)
    {
        return !pawn.IsHacked();
    }

    protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, Random r)
    {
        int[] chances =
        {
            Base.failureChanceIntRaidTooLarge, Base.failureChanceShootRandomDirection, Base.failureChanceHealToStanding,
            Base.failureChanceNothing
        };
        var totalChance = chances.Sum();
        var randInt = r.Next(1, totalChance);
        Action<Pawn, BodyPartRecord, RecipeDef>[] functions =
        {
            RecipeUtility.CauseIntendedMechanoidRaidTooLarge, RecipeUtility.ShootRandomDirection,
            RecipeUtility.HealToStanding, RecipeUtility.Nothing
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
    }

    protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        RecipeUtility.CauseIntendedMechanoidRaid(pawn, part, recipe);
        var cooldown = new IntRange(GenDate.TicksPerQuadrum / 2, GenDate.TicksPerQuadrum).RandomInRange;
        Base.Instance.GetExtendedDataStorage().lastEmergencySignalCooldown = cooldown;
    }
}