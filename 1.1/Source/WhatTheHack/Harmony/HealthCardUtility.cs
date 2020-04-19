using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using WhatTheHack.Recipes;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(HealthCardUtility), "GenerateSurgeryOption")]
    class HealthCardUtility_GenerateSurgeryOption
    {
        static bool Prefix(Pawn pawn, RecipeDef recipe, BodyPartRecord part, ref FloatMenuOption __result)
        {
            if(recipe.Worker is Recipe_Hacking worker)
            {
                if (!worker.CanApplyOn(pawn, out string reason)){
                    if(reason == "")
                    {
                        return false;
                    }
                    string text = recipe.Worker.GetLabelWhenUsedOn(pawn, part).CapitalizeFirst();
                    if (part != null && !recipe.hideBodyPartNames)
                    {
                        text = text + " (" + part.Label + ")";
                    }
                    FloatMenuOption floatMenuOption;
                
                    text += " (" + reason + ")";
                    floatMenuOption = new FloatMenuOption(text, null);
                    __result = floatMenuOption;
                    return false;
                }
            }
            return true;
        }
    }
}
