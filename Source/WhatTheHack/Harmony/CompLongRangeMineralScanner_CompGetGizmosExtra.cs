using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch]
public static class CompLongRangeMineralScanner_CompGetGizmosExtra
{
    //Code is inside m__0 method inside iterator so TargetMethod is used to access it. 
    private static MethodBase TargetMethod()
    {
        return typeof(CompLongRangeMineralScanner).GetNestedTypes(AccessTools.all).FirstOrDefault(c => c.Name == "<>c")
            ?.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("CompGetGizmosExtra"));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        foreach (var codeInstruction in instructionsList)
        {
            if (codeInstruction.operand as MethodInfo == typeof(WindowStack).GetMethod("Add"))
            {
                //replace call to WindowStack.Add to method that performs WindowStack.Add but also adds a mechanoid part option     
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(CompLongRangeMineralScanner_CompGetGizmosExtra),
                        "AddMechPartsOption")); //Injected code     
            }
            else
            {
                yield return codeInstruction;
            }
        }
    }

    public static void AddMechPartsOption(WindowStack instance, FloatMenu menu)
    {
        var options = menu.options;
        //Traverse.Create(menu).Field("options").GetValue<List<FloatMenuOption>>();
        //options.Add(new FloatMenuOption());
        var researchComplete =
            DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault(
                rp => rp == WTH_DefOf.WTH_LRMSTuning && rp.IsFinished) != null;
        var mechanoidParts = WTH_DefOf.WTH_MineableMechanoidParts;

        if (researchComplete)
        {
            var item = new FloatMenuOption("WTH_MechanoidParts_LabelShort".Translate(), delegate
                {
                    foreach (var selectedObject in Find.Selector.SelectedObjects)
                    {
                        if (selectedObject is not Thing selection)
                        {
                            continue;
                        }

                        var compLongRangeMineralScanner = selection.TryGetComp<CompLongRangeMineralScanner>();
                        if (compLongRangeMineralScanner != null)
                        {
                            compLongRangeMineralScanner.targetMineable = mechanoidParts;
                            //Traverse.Create(compLongRangeMineralScanner).Field("targetMineable").SetValue(mechanoidParts);
                        }
                    }
                }, MenuOptionPriority.Default, null, null, 29f,
                rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2),
                    mechanoidParts.GetConcreteExample()));
            options.Add(item);
        }

        //Traverse.Create(menu).Field("options").SetValue(options);
        instance.Add(menu);
    }
}