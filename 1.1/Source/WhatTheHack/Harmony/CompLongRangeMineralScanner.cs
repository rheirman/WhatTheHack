using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony
{

    //Make sure mechanoid temple quest is started when minerals are found when the scanner is tuned to mech parts.  
    [HarmonyPatch(typeof(CompLongRangeMineralScanner), "DoFind")]
    public static class CompLongRangeMineralScanner_Foundminerals
    {
        private const int MinDistance = 6;
        private const int MaxDistance = 22;
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 25, max: 50);

        static bool Prefix(CompLongRangeMineralScanner __instance, Pawn worker, ref ThingDef ___targetMineable)
        {
            if (__instance!=null)
            {
                if (___targetMineable == null)
                {
                    return true;
                }
                if (___targetMineable == WTH_DefOf.WTH_MineableMechanoidParts)
                {
                //    Traverse.Create(__instance).Field("daysWorkingSinceLastMinerals").SetValue(0f);
                    if (!TileFinder.TryFindNewSiteTile(out int tile, MinDistance, MaxDistance, true, false))
                        return false;

                    Slate slate = new Slate();
                    slate.Set<Map>("map", worker.Map, false);
                    slate.Set<ThingDef>("targetMineable", ___targetMineable, false);
                    slate.Set<Pawn>("worker", worker, false);
                    if (!WTH_DefOf.WTH_LongRangeMineralScannerMechParts.CanRun(slate))
                    {
                        return true;
                    }
                    Quest quest = QuestUtility.GenerateQuestAndMakeAvailable(WTH_DefOf.WTH_LongRangeMineralScannerMechParts, slate);
                    Find.LetterStack.ReceiveLetter(quest.name, quest.description, LetterDefOf.PositiveEvent, null, null, quest, null, null);
                    return false;
                }
            }
            return true;

        }


    }


   [HarmonyPatch]
   public static class CompLongRangeMineralScanner_CompGetGizmosExtra
   {
        //Code is inside m__0 method inside iterator so TargetMethod is used to access it. 
        static MethodBase TargetMethod()
        {
            return typeof(CompLongRangeMineralScanner).GetNestedTypes(AccessTools.all).FirstOrDefault((c) => c.Name == "<>c").GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("b__7_0"));
        }
        
       static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
       {
           var instructionsList = new List<CodeInstruction>(instructions);
           for (var i = 0; i < instructionsList.Count; i++)
           {
               CodeInstruction instruction = instructionsList[i];

               if (instructionsList[i].operand as MethodInfo == typeof(WindowStack).GetMethod("Add"))
               {
                    //replace call to WindowStack.Add to method that performs WindowStack.Add but also adds a mechanoid part option     
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CompLongRangeMineralScanner_CompGetGizmosExtra), "AddMechPartsOption"));//Injected code     

               }
               else
               {
                   yield return instruction;
               }
           }
        }
       
        public static void AddMechPartsOption(WindowStack instance, FloatMenu menu)
        {
            List<FloatMenuOption> options = Traverse.Create(menu).Field("options").GetValue<List<FloatMenuOption>>();
            //options.Add(new FloatMenuOption());
            bool researchComplete = DefDatabase<ResearchProjectDef>.AllDefs.FirstOrDefault((ResearchProjectDef rp) => rp == WTH_DefOf.WTH_LRMSTuning && rp.IsFinished) != null;
            ThingDef mechanoidParts = WTH_DefOf.WTH_MineableMechanoidParts;

            if (researchComplete)
            {
                FloatMenuOption item = new FloatMenuOption("WTH_MechanoidParts_LabelShort".Translate(), delegate
                {
                    foreach (object selectedObject in Find.Selector.SelectedObjects)
                    {
                        Thing selection = selectedObject as Thing;
                        if (selection != null)
                        {
                            CompLongRangeMineralScanner compLongRangeMineralScanner = selection.TryGetComp<CompLongRangeMineralScanner>();
                            if (compLongRangeMineralScanner != null)
                            {
                                Traverse.Create(compLongRangeMineralScanner).Field("targetMineable").SetValue(mechanoidParts);
                            }
                        }
                    }
                }, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2, mechanoidParts.GetConcreteExample()), null);
                options.Add(item);
            }

            Traverse.Create(menu).Field("options").SetValue(options);
            instance.Add(menu);
        }
   }
}
