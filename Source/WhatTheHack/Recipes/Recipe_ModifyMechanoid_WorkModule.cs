using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Storage;

namespace WhatTheHack.Recipes
{
    class Recipe_ModifyMechanoid_WorkModule : Recipe_ModifyMechanoid
    {
        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
            if (pawn.skills == null)
            {
                pawn.skills = new Pawn_SkillTracker(pawn);
            }
            if (pawn.workSettings == null)
            {
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                pawn.workSettings.EnableAndInitialize();
            }
            if (bill.recipe.GetModExtension<DefModExtension_Recipe_WorkModule>() is DefModExtension_Recipe_WorkModule modExt)
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                if(pawnData.workTypes == null)
                {
                    pawnData.workTypes = new List<WorkTypeDef>();
                }
                pawnData.workTypes.Add(modExt.workType);
                pawn.workSettings.SetPriority(modExt.workType, 3);
            }


        }

    }
}
