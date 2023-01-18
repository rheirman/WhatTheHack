using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Recipes;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(HealthCardUtility), "GenerateSurgeryOption")]
internal class HealthCardUtility_GenerateSurgeryOption
{
    private static bool Prefix(Pawn pawn, RecipeDef recipe, BodyPartRecord part, ref FloatMenuOption __result)
    {
        if (recipe.Worker is not Recipe_Hacking worker)
        {
            return true;
        }

        if (worker.CanApplyOn(pawn, out var reason))
        {
            return true;
        }

        if (reason == "")
        {
            return false;
        }

        var text = recipe.Worker.GetLabelWhenUsedOn(pawn, part).CapitalizeFirst();
        if (part != null && !recipe.hideBodyPartNames)
        {
            text = $"{text} ({part.Label})";
        }

        text += $" ({reason})";
        var floatMenuOption = new FloatMenuOption(text, null);
        __result = floatMenuOption;
        return false;
    }
}