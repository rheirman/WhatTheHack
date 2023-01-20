using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Dialog_FormCaravan), "TrySend")]
internal class Dialog_FormCaravan_TrySend
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        for (var i = 0; i < instructionsList.Count; i++)
        {
            var instruction = instructionsList[i];
            if (instruction.opcode == OpCodes.Stloc_0)
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(Dialog_FormCaravan_TrySend).GetMethod("AddWarnings"));
            }

            yield return instruction;
        }
    }

    public static List<string> AddWarnings(List<string> warnings, Dialog_FormCaravan instance)
    {
        var numMechanoids = 0;
        var numPlatforms = 0;
        foreach (var tow in instance.transferables)
        {
            if (tow.ThingDef.race is { IsMechanoid: true } && tow.AnyThing is Pawn pawn &&
                pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
            {
                numMechanoids += tow.CountToTransfer;
            }

            if (tow.ThingDef == ThingDefOf.MinifiedThing)
            {
                if (tow.things[0].GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform)
                {
                    numPlatforms += tow.CountToTransfer;
                }
            }

            if (tow.ThingDef == WTH_DefOf.WTH_PortableChargingPlatform)
            {
                numPlatforms += tow.CountToTransfer;
            }
        }

        if (numMechanoids == 0)
        {
            return warnings;
        }

        if (numPlatforms < numMechanoids)
        {
            warnings.Add("WTH_Warning_NotEnoughPlatforms".Translate());
        }
        else if (Base.Instance.daysOfFuel <
                 Traverse.Create(instance).Field("MaxDaysWorthOfFoodToShowWarningDialog").GetValue<float>())
        {
            warnings.Add("WTH_Warning_DaysOfFuel".Translate(Base.Instance.daysOfFuel.ToString("0.#")));
        }

        return warnings;
    }
}