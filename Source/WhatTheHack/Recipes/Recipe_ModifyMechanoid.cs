using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace WhatTheHack.Recipes;

public class Recipe_ModifyMechanoid : Recipe_Hacking
{
    public override bool CanApplyOn(Pawn pawn, out string reason)
    {
        reason = "";
        if (!recipe.HasModExtension<DefModExtension_Recipe>())
        {
            return true;
        }

        var ext = recipe.GetModExtension<DefModExtension_Recipe>();
        if (ext.requiredHediff != null && !pawn.health.hediffSet.HasHediff(ext.requiredHediff))
        {
            reason = "WTH_Reason_MissingHediff".Translate(ext.requiredHediff.label);
            return false;
        }

        var ignoreBodySize = recipe.addsHediff == WTH_DefOf.WTH_TurretModule &&
                             pawn.def.GetModExtension<DefModExtension_TurretModule>() is { ignoreMinBodySize: true };
        if (ignoreBodySize || !(pawn.BodySize < ext.minBodySize))
        {
            return true;
        }

        reason = "WTH_Reason_BodySize".Translate(ext.minBodySize);
        return false;
    }

    protected override bool IsValidPawn(Pawn pawn)
    {
        return pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(recipe.addsHediff);
    }

    protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        if (recipe.GetModExtension<DefModExtension_Recipe>() is
            {
                addsAdditionalHediff: { }
            } extension)
        {
            BodyPartRecord additionalHediffBodyPart = null;
            if (extension.additionalHediffBodyPart != null)
            {
                additionalHediffBodyPart = pawn.health.hediffSet.GetNotMissingParts()
                    .FirstOrDefault(bpr => bpr.def == extension.additionalHediffBodyPart);
            }

            pawn.health.AddHediff(extension.addsAdditionalHediff, additionalHediffBodyPart);
        }

        if (pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) && pawn.GetComp<CompRefuelable>() == null)
        {
            pawn.InitializeComps();
        }

        if (pawn.BillStack.Bills.Count <= 1)
        {
            pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }
    }

    protected override void HackingFailEvent(Pawn hacker, Pawn hackee, BodyPartRecord part, Random r)
    {
        base.HackingFailEvent(hacker, hackee, part, r);
        Messages.Message(
            "MessageMedicalOperationFailureMinor".Translate(hacker.LabelShort, hackee.def.label,
                hacker.Named("SURGEON"), hackee.Named("PATIENT")), hackee, MessageTypeDefOf.NegativeHealthEvent);
        HealthUtility.GiveRandomSurgeryInjuries(hackee, 20, part);
        foreach (var ic in recipe.ingredients)
        {
            var t = ThingMaker.MakeThing(ic.filter.AnyAllowedDef);
            GenSpawn.Spawn(t, hackee.Position, hackee.Map);
        }
    }
}