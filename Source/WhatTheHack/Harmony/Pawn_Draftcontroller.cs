using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    //Make sure mechanoids are activated automatically when drafted. 
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    static class Pawn_Draftcontroller_set_Drafted
    {
        static void Postfix(Pawn_DraftController __instance)
        {
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                pawnData.isActive = true;
            }

        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    static class Pawn_DraftController_GetGizmos
    {
        static void Postfix(Pawn_DraftController __instance, ref IEnumerable<Gizmo> __result)
        {
            __result = PatchGetGizmos(__instance, __result);
        }

        private static IEnumerable<Gizmo> PatchGetGizmos(Pawn_DraftController __instance, IEnumerable<Gizmo> __result)
        {
            foreach (Gizmo gizmo in __result)
            {
                if (gizmo is Command_Toggle)
                {
                    Command_Toggle toggleCommand = gizmo as Command_Toggle;
                    if (toggleCommand.defaultLabel == "CommandDraftLabel".Translate())
                    {
                        DisableCommandIfMechanoidPowerLow(__instance, toggleCommand);
                        yield return toggleCommand;
                        continue;
                    }
                }
                yield return gizmo;
            }
        }

        private static void DisableCommandIfMechanoidPowerLow(Pawn_DraftController __instance, Command_Toggle toggleCommand)
        {
            Need_Power powerNeed = __instance.pawn.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power) as Need_Power;
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (powerNeed != null && store != null)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                if (pawnData.shouldAutoRecharge && powerNeed.CurCategory >= PowerCategory.LowPower)
                {
                    toggleCommand.Disable("WTH_Reason_PowerLow".Translate());
                }
            }
        }
    }



        //Disabled following code. Somehow the pawn provided as an argument misses all sorts of data that should be assigned. Instead of the transpiler I now used a more ugly postix.
        //Make sure drafting gizmo is disabled when power of mechanoid is low while auto recharge mode is on. 
        /*
        [HarmonyPatch]
        static class Pawn_DraftController_GetGizmos
        {
            static MethodBase TargetMethod()
            {
                var predicateClass = typeof(Pawn_DraftController).GetNestedTypes(AccessTools.all)
                    .FirstOrDefault(t => t.FullName.Contains("c__Iterator0"));
                return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext"));
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Log.Message("applying transpiler patch for get gizmos");
                var instructionsList = new List<CodeInstruction>(instructions);
                for (var i = 0; i < instructionsList.Count; i++)
                {
                    CodeInstruction instruction = instructionsList[i];
                    if (i < instructionsList.Count - 3 &&  instructionsList[i + 2].operand == typeof(Pawn).GetMethod("get_Downed"))
                    {
                        Log.Message("found method get_Downed");
                        //yield return new CodeInstruction(OpCodes.Ldloc_0);
                        Log.Message("after loading args");
                        yield return new CodeInstruction(OpCodes.Call, typeof(Pawn_DraftController_GetGizmos).GetMethod("DeactivateWhenPowerLow", new Type[] { typeof(Pawn_DraftController) }));//Injected code     
                        yield return new CodeInstruction(OpCodes.Ldarg_0);//load Pawns argument

                        Log.Message("after loading method");
                        Log.Message("inserting before: " + instruction.opcode);

                    }
                    yield return instruction;
                }
            }
            public static void DeactivateWhenPowerLow(Pawn_DraftController draftController)
            {
                draftController.pawn.needs.AddOrRemoveNeedsAsAppropriate();

                if (draftController.pawn.needs == null || draftController.pawn.needs.AllNeeds.NullOrEmpty())
                {

                    Log.Message("needs null or empty, what the hack");
                    Log.Message("draftController.pawn" + draftController.pawn);

                    return;
                }


                Need_Power powerNeed = draftController.pawn.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power) as Need_Power;
                ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();

                Log.Message("powerNeed curCategory: " + powerNeed.CurCategory);
                if(powerNeed != null && store != null)
                {

                    ExtendedPawnData pawnData = store.GetExtendedDataFor(draftController.pawn);
                    if(pawnData.shouldAutoRecharge && powerNeed.CurCategory >= PowerCategory.LowPower)
                    {
                        Log.Message("should disable draft gizmo");
                        //draft.Disable("WTH_Reason_PowerLow".Translate());
                    }
                }
            }
        }
        */

    }

