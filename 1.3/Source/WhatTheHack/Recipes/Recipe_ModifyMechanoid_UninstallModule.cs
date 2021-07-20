using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using WhatTheHack.Storage;

namespace WhatTheHack.Recipes
{
    class Recipe_ModifyMechanoid_UninstallModule : Recipe_ModifyMechanoid
    {
        protected override bool IsValidPawn(Pawn pawn)
        {
            if (recipe.GetModExtension<DefModExtension_Recipe>() is DefModExtension_Recipe modExt && modExt.requiredHediff != null)
            {
                bool hasRequiredHediff = pawn.health.hediffSet.HasHediff(modExt.requiredHediff);
                return pawn.IsHacked() && hasRequiredHediff;
            }
            return false;
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
            if (recipe.GetModExtension<DefModExtension_Recipe>() is DefModExtension_Recipe modExt)
            {
                HediffDef removedHediff = modExt.requiredHediff;
                Uninstallmodule(pawn, removedHediff);
                Cleanup(pawn, removedHediff);
            }
        }

        private static void Cleanup(Pawn pawn, HediffDef removedHediff)
        {
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            if (removedHediff.GetModExtension<DefModExtension_Hediff_WorkModule>() is DefModExtension_Hediff_WorkModule ext)
            {
                pawnData.workTypes.RemoveAll((WorkTypeDef def) => ext.workTypes.Contains(def));
            }
            if (!pawn.health.hediffSet.hediffs.Exists((Hediff h) => h.def.HasModExtension<DefModExtension_Hediff_WorkModule>()))
            {
                pawnData.workTypes = null;
                pawn.skills = null;
                pawn.workSettings = null;
            }
            if(removedHediff == WTH_DefOf.WTH_RepairModule)
            {
                Base.RemoveComps(pawn);
            }
        }

        private static void Uninstallmodule(Pawn pawn, HediffDef removedHediff)
        {
            pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == removedHediff));
            Thing t = ThingMaker.MakeThing(removedHediff.GetModExtension<DefModextension_Hediff>().extraButcherProduct);
            if (t != null)
            {
                t.stackCount = 1;
                GenPlace.TryPlaceThing(t, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
        }
    }

}
