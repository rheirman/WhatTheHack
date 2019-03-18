using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace WhatTheHack.Recipes
{
    class Recipe_UninstallModule : Recipe_ModifyMechanoid
    {
        protected override bool CanApplyOn(Pawn pawn)
        {
            return base.CanApplyOn(pawn);
        }

        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
            if (recipe.GetModExtension<DefModExtension_Recipe>() is DefModExtension_Recipe modExt)
            {
                pawn.health.RemoveHediff(pawn.health.hediffSet.hediffs.FirstOrDefault((Hediff h) => h.def == modExt.requiredHediff));
                Thing t = ThingMaker.MakeThing(modExt.requiredHediff.GetModExtension<DefModextension_Hediff>().extraButcherProduct);
                if(t != null)
                {
                    t.stackCount = 1;
                    GenPlace.TryPlaceThing(t, pawn.Position, pawn.Map, ThingPlaceMode.Near);
                }
            }
        }
    }

}
