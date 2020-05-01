using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(StatWorker), "GetExplanationUnfinalized")]
    public static class StatWorker_GetExplanationUnfinalized
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            int i = 0;
            foreach (CodeInstruction instruction in instructionsList)
            {
                if (instruction.operand as MethodInfo == typeof(Pawn).GetField("skills") && instructionsList[i + 1].opcode == OpCodes.Brfalse)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
                    yield return new CodeInstruction(OpCodes.Call, typeof(Utilities).GetMethod("ShouldGetStatValue"));
                }
                else
                {
                    yield return instruction;
                }
                i++;
            }
        }
    }

    //This makes sure that mechanoids without work modules for certain skills don't use their skill value for those skills, but use the noSkillOffset or noSkillFactor instead.
    //The transpiler below is more correct, but its impact on performance is too high, so I replaced it with a simple postfix that just replaces calculated stat value with the noSkillOfset.
    [HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    public static class StatWorker_GetValueUnfinalized
    {
        static void Postfix(StatWorker __instance, StatRequest req, ref StatDef ___stat, ref float __result)
        {
            Pawn pawn = req.Thing as Pawn;
            if (pawn != null && pawn.IsHacked())
            {
                if (Utilities.ShouldGetStatValue(pawn, ___stat))
                {
                    return;
                }
                else
                {
                    __result = GetBaseValueFor(req, ___stat) + ___stat.noSkillOffset;
                    if (req.HasThing)
                    {
                        __result *= ___stat.noSkillOffset;
                    }
                }
            }
        }
        private static float GetBaseValueFor(StatRequest request, StatDef stat)
        {
            float result = stat.defaultBaseValue;
            if (request.StatBases != null)
            {
                for (int i = 0; i < request.StatBases.Count; i++)
                {
                    if (request.StatBases[i].stat == stat)
                    {
                        result = request.StatBases[i].value;
                        break;
                    }
                }
            }
            return result;
        }
    }

    ////This makes sure that mechanoids without work modules for certain skills don't use their skill value for those skills, but use the noSkillOffset or noSkillFactor instead.
    //[HarmonyPatch(typeof(StatWorker), "GetValueUnfinalized")]
    //public static class StatWorker_GetValueUnfinalized
    //{
    //    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    //    {
    //        var instructionsList = new List<CodeInstruction>(instructions);
    //        foreach (CodeInstruction instruction in instructionsList)
    //        {
    //            if (instruction.operand as MethodInfo == AccessTools.Field(typeof(Pawn), "skills"))
    //            {
    //                yield return new CodeInstruction(OpCodes.Ldarg_0);
    //                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(StatWorker), "stat"));
    //                yield return new CodeInstruction(OpCodes.Call, typeof(Utilities).GetMethod("ShouldGetStatValue"));
    //            }
    //            else
    //            {
    //                yield return instruction;
    //            }
    //        }
    //    }
    //    /*
    //    public static bool ShouldGetStatValue(Pawn pawn)
    //    {
    //        return false;
    //    }

    //    */

    //}

    [HarmonyPatch(typeof(StatWorker), "ShouldShowFor")]
    static class StatWorker_ShouldShowFor
    {
        static bool Prefix(StatWorker __instance, StatRequest req, ref bool __result, ref StatDef ___stat)
        {
            if (___stat.category == WTH_DefOf.WTH_StatCategory_HackedMechanoid && req.Thing is Pawn pawn)
            {
                __result = pawn.IsHacked();
                return false;
            }
            if(___stat.category == WTH_DefOf.WTH_StatCategory_Colonist && req.Thing is Pawn pawn2)
            {
                __result = pawn2.IsColonistPlayerControlled;
                return false;
            }
            if(___stat.category == WTH_DefOf.WTH_StatCategory_Platform && req.Thing is Building_BaseMechanoidPlatform)
            {
                __result = true;
                return false;
            }
            if(___stat.category == WTH_DefOf.WTH_StatCategory_HackedMechanoid || ___stat.category == WTH_DefOf.WTH_StatCategory_Colonist || ___stat.category == WTH_DefOf.WTH_StatCategory_Platform)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
        
    [HarmonyPatch(typeof(StatWorker), "IsDisabledFor")]
    static class StatWorker_IsDisabledFor
    {
        static bool Prefix(Thing thing, ref bool __result)
        {

            if(thing is Pawn && ((Pawn)thing).RaceProps.IsMechanoid)
            {
                Pawn pawn = (Pawn)thing;
                if (pawn.IsHacked())
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }

    }
        

}
