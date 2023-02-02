﻿using RimWorld;
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
            if (pawn.skills == null || pawn.workSettings == null || Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn).workTypes == null)
            {
                //for people who became victums of NullReferenceException bug (reinitialize ALL mechanoid skills and worktypes)
                Utilities.InitWorkTypesAndSkills(pawn, Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn));
            }
            if (bill.recipe.addsHediff.GetModExtension<DefModExtension_Hediff_WorkModule>() is DefModExtension_Hediff_WorkModule modExt)
            {
                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
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
