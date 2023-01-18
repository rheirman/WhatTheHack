using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using WhatTheHack.Buildings;
using Random = System.Random;

namespace WhatTheHack.Recipes;

public abstract class Recipe_Hacking : RecipeWorker
{
    protected bool allowMultipleParts = false;

    public virtual bool CanApplyOn(Pawn pawn, out string reason)
    {
        reason = "";
        return true;
    }

    protected virtual bool IsValidPawn(Pawn pawn)
    {
        return true;
    }


    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        var partFound = false;
        if (!IsValidPawn(pawn))
        {
            yield break;
        }

        //Copy from vanilla. Much more complex than needed, but does the trick
        foreach (var part in recipe.appliedOnFixedBodyParts)
        {
            var bpList = pawn.RaceProps.body.AllParts;
            foreach (var record in bpList)
            {
                if ((record.def != part || partFound) && !allowMultipleParts)
                {
                    continue;
                }

                var record1 = record;
                var diffs = from x in pawn.health.hediffSet.hediffs
                    where x.Part == record1
                    select x;
                if (diffs.Count() == 1 && diffs.First().def == recipe.addsHediff)
                {
                    continue;
                }

                if (record.parent != null &&
                    !pawn.health.hediffSet.GetNotMissingParts().Contains(record.parent))
                {
                    continue;
                }

                if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record) &&
                    !pawn.health.hediffSet.HasDirectlyAddedPartFor(record))
                {
                    continue;
                }

                yield return record;
                partFound = true;
            }

            if (recipe.GetModExtension<DefModExtension_Recipe>() is { needsFixedBodyPart: false } && !partFound)
            {
                yield return null;
            }
        }
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        var learnfactor = 1f / recipe.surgerySuccessChanceFactor;

        //We cap the combat power since some mods use extremely high combat power to prevent mechs from being spawned in raids. 
        var combatPowerCapped = pawn.kindDef.combatPower <= 10000 ? pawn.kindDef.combatPower : 300;

        //Let random bad events happen when hacking fails
        if (CheckHackingFail(pawn, billDoer, part))
        {
            learnfactor *= 0.5f;
            if (pawn.Dead)
            {
                return;
            }

            billDoer.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * learnfactor);
            billDoer.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * learnfactor);
            return;
        }

        TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        if (recipe.addsHediff != null)
        {
            pawn.health.AddHediff(recipe.addsHediff, part);
        }

        billDoer.skills.Learn(SkillDefOf.Crafting, combatPowerCapped * learnfactor);
        billDoer.skills.Learn(SkillDefOf.Intellectual, combatPowerCapped * learnfactor);
        PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
    }

    protected abstract void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill);

    private bool CheckHackingFail(Pawn hackee, Pawn hacker, BodyPartRecord part)
    {
        var successChance = 1.0f;
        successChance *= recipe.surgerySuccessChanceFactor;
        successChance *= hacker.GetStatValue(WTH_DefOf.WTH_HackingSuccessChance);
        var r = new Random(DateTime.Now.Millisecond);
        var combatPowerFactorCapped = CalcCombatPowerFactorCapped(hackee);
        successChance *= combatPowerFactorCapped;
        if (recipe.GetModExtension<DefModExtension_Recipe>() is { surgerySuccessCap: > 0 } ext)
        {
            if (successChance > 1.0f)
            {
                successChance = 1.0f;
            }

            successChance *= ext.surgerySuccessCap;
        }

        if (Rand.Chance(successChance))
        {
            return false;
        }

        MoteMaker.ThrowText((hacker.DrawPos + hackee.DrawPos) / 2f, hacker.Map,
            "WTH_TextMote_OperationFailed".Translate(successChance.ToStringPercent()), 8f);
        if (Rand.Chance(recipe.deathOnFailedSurgeryChance))
        {
            HealthUtility.GiveRandomSurgeryInjuries(hackee, 65, part);
            if (!hackee.Dead)
            {
                hackee.Kill(null);
            }

            Messages.Message(
                "MessageMedicalOperationFailureFatal".Translate(hacker.LabelShort, hackee.LabelShort,
                    recipe.LabelCap, hacker.Named("SURGEON"), hackee.Named("PATIENT")), hackee,
                MessageTypeDefOf.NegativeHealthEvent);
        }
        else
        {
            HackingFailEvent(hacker, hackee, part, r);
        }

        return true;
    }

    protected virtual void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, Random r)
    {
        ((Building_HackingTable)hackee.CurrentBed()).TryAddPawnForModification(hackee, recipe);
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