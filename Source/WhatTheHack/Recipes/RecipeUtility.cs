using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Recipes
{
    class RecipeUtility
    {
        public static IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            for (int i = 0; i < recipe.appliedOnFixedBodyParts.Count; i++)
            {
                BodyPartDef part = recipe.appliedOnFixedBodyParts[i];
                Log.Message("testing for: " + part.defName);
                foreach(BodyPartRecord br in pawn.health.hediffSet.GetNotMissingParts())
                {
                    Log.Message("Record defname: " + br.def.defName); 
                }

                BodyPartRecord r = pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault((BodyPartRecord x) => x.def == part);
                if(r != null)
                {
                    yield return r;
                }
            }
        }

    }
}
