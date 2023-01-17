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
            ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
            if (pawnData.workTypes == null)
            {
                pawnData.workTypes = new List<WorkTypeDef>();
            }
            if (pawn.skills == null)
            {
                pawn.skills = new Pawn_SkillTracker(pawn);
            }
            if(pawn.workSettings == null)
            {
                pawn.workSettings = new Pawn_WorkSettings(pawn);
                pawn.workSettings.EnableAndInitialize();
            }
            Utilities.InitWorkTypesAndSkills(pawn, pawnData);
            if (bill.recipe.addsHediff.GetModExtension<DefModExtension_Hediff_WorkModule>() is DefModExtension_Hediff_WorkModule modExt)
            {

                foreach (WorkTypeDef workType in modExt.workTypes)
                {
                    pawnData.workTypes.Add(workType);
                    pawn.workSettings.SetPriority(workType, 3);
                    if (modExt.skillLevel > 0)
                    {
                        foreach (SkillDef skillDef in workType.relevantSkills)
                        {
                            pawn.skills.GetSkill(skillDef).Level = modExt.skillLevel;
                        }
                    }
                }

            }


        }
    }
}
