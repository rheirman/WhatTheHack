using System.Collections.Generic;
using RimWorld;
using Verse;

namespace WhatTheHack.Recipes;

internal class Recipe_ModifyMechanoid_UninstallModule : Recipe_ModifyMechanoid
{
    protected override bool IsValidPawn(Pawn pawn)
    {
        if (recipe.GetModExtension<DefModExtension_Recipe>() is not { requiredHediff: { } } modExt)
        {
            return false;
        }

        var hasRequiredHediff = pawn.health.hediffSet.HasHediff(modExt.requiredHediff);
        return pawn.IsHacked() && hasRequiredHediff;
    }

    protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients,
        Bill bill)
    {
        base.PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
        if (recipe.GetModExtension<DefModExtension_Recipe>() is not { } modExt)
        {
            return;
        }

        var removedHediff = modExt.requiredHediff;
        Uninstallmodule(pawn, removedHediff);
        Cleanup(pawn, removedHediff);
    }

    private static void Cleanup(Pawn pawn, HediffDef removedHediff)
    {
        if (removedHediff == WTH_DefOf.WTH_TurretModule && pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
        {
            pawn.health.RemoveHediff(
                pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == WTH_DefOf.WTH_MountedTurret));
        }

        var pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
        if (removedHediff.GetModExtension<DefModExtension_Hediff_WorkModule>() is { } ext)
        {
            pawnData.workTypes.RemoveAll(def => ext.workTypes.Contains(def));
        }

        if (!pawn.health.hediffSet.hediffs.Exists(h => h.def.HasModExtension<DefModExtension_Hediff_WorkModule>()))
        {
            pawnData.workTypes = null;
            pawn.skills = null;
            pawn.workSettings = null;
        }

        if (removedHediff == WTH_DefOf.WTH_RepairModule)
        {
            Base.RemoveComps(pawn);
        }
    }

    private static void Uninstallmodule(Pawn pawn, HediffDef removedHediff)
    {
        pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == removedHediff));
        var t = ThingMaker.MakeThing(removedHediff.GetModExtension<DefModextension_Hediff>().extraButcherProduct);
        if (t == null)
        {
            return;
        }

        t.stackCount = 1;
        GenPlace.TryPlaceThing(t, pawn.Position, pawn.Map, ThingPlaceMode.Near);
    }
}