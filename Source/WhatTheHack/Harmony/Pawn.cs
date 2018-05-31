using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Buildings;
using WhatTheHack.Duties;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn), "CurrentlyUsableForBills")]
    static class Pawn_CurrentlyUsableForBills
    {
        static void Postfix(Pawn __instance, ref bool __result)
        {
            Bill bill = __instance.health.surgeryBills.FirstShouldDoNow;
            if (!__instance.RaceProps.IsMechanoid)
            {
                return;
            }

            if (bill != null && bill.recipe == WTH_DefOf.HackMechanoid &&  !__instance.OnHackingTable())
            {
                __result = false;
            }
        }
    }

    
    [HarmonyPatch(typeof(Pawn), "get_IsColonistPlayerControlled")]
    public class Pawn_get_IsColonistPlayerControlled
    {
        public static bool Prefix(Pawn __instance, ref bool __result)
        {
            if (__instance.HasReplacedAI())
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
    
    /*
    [HarmonyPatch]
    public static class Pawn_GetGizmos_Transpiler {
        static MethodBase TargetMethod()
        {
            var predicateClass = typeof(Pawn).GetNestedTypes(AccessTools.all)
                .FirstOrDefault(t => t.FullName.Contains("c__Iterator2"));
            return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext"));
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count - 1; i++)
            {
                CodeInstruction instruction = instructionsList[i];

                if (instructionsList[i].operand == typeof(Pawn).GetMethod("get_IsColonistPlayerControlled"))
                {
                    //yield return new CodeInstruction(OpCodes.Call, typeof(Pawn).GetMethod("CanTakeOrder"));//Injected code     
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Extensions), "CanTakeOrder"));//Injected code     

                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }
    */


    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            bool isCreatureMine = __instance.Faction != null && __instance.Faction.IsPlayer;

            if (store == null || !isCreatureMine)
            {
                return;
            }
            if (__instance.IsHacked())
            {
                AddHackedPawnGizmos(__instance, ref gizmoList, store);
            }

            __result = gizmoList;
        }

        private static void AddHackedPawnGizmos(Pawn __instance, ref List<Gizmo> gizmoList, ExtendedDataStorage store)
        {
            ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance);
            gizmoList.Add(CreateGizmo_SearchAndDestroy(__instance, pawnData));
            gizmoList.Add(CreateGizmo_AutoRecharge(__instance, pawnData));
        }

        private static Gizmo CreateGizmo_SearchAndDestroy(Pawn __instance, ExtendedPawnData pawnData)
        {
            string disabledReason = "";
            bool disabled = false;
            if (__instance.Downed)
            {
                disabled = true;
                disabledReason = "WTH_Reason_MechanoidDowned".Translate();
            }
            else if(pawnData.shouldAutoRecharge)
            {
                Need_Power powerNeed = __instance.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power) as Need_Power;
                if (powerNeed != null && powerNeed.CurCategory >= PowerCategory.LowPower)
                {
                    disabled = true;
                    disabledReason = "WTH_Reason_PowerLow".Translate();
                }
            }
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "WTH_Gizmo_SearchAndDestroy_Label".Translate(),
                defaultDesc = "WTH_Gizmo_SearchAndDestroy_Description".Translate(),
                disabled = disabled,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "Enable_SD"), true),
                isActive = () => pawnData.isActive,
                toggleAction = () =>
                {
                    pawnData.isActive = !pawnData.isActive;
                    if (pawnData.isActive)
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        if (__instance.GetLord() == null || __instance.GetLord().LordJob == null)
                        {
                            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), __instance.Map, new List<Pawn> { __instance });
                        }
                        else
                        {
                            Log.Message("lord was null!");
                        }
                    }
                    else
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        Building_MechanoidPlatform closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(__instance, __instance);
                        if (closestAvailablePlatform != null)
                        {
                            Job job = new Job(WTH_DefOf.Mechanoid_Rest, closestAvailablePlatform);
                            __instance.jobs.TryTakeOrderedJob(job);
                        }
                    }
                }
            };
            return gizmo;
        }
        private static Gizmo CreateGizmo_AutoRecharge(Pawn __instance, ExtendedPawnData pawnData)
        {
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "WTH_Gizmo_AutoRecharge_Label".Translate(),
                defaultDesc = "WTH_Gizmo_AutoRecharge_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("UI/" + "AutoRecharge"), true),
                isActive = () => pawnData.shouldAutoRecharge,
                toggleAction = () =>
                {
                    pawnData.shouldAutoRecharge = !pawnData.shouldAutoRecharge;
                }
            };
            return gizmo;
        }
    }
}
