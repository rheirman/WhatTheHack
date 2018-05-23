using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (__instance.RaceProps.IsMechanoid)
            {
                Log.Message("CurrentlyUsableForBills called");
            }

            if (bill != null && bill.recipe == WTH_DefOf.HackMechanoid &&  !__instance.OnHackingTable())
            {
                Log.Message("Cannot do bill now, mechanoid should be on hacking table");
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_DraftController_GetGizmos_Patch
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if (store == null || !__instance.IsHacked())
            {
                return;
            }

            ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance);
            gizmoList.Add(CreateGizmo_SearchAndDestroy(__instance, pawnData));
            gizmoList.Add(CreateGizmo_AutoRecharge(__instance, pawnData));
            __result = gizmoList;
        }

        private static Gizmo CreateGizmo_SearchAndDestroy(Pawn __instance, ExtendedPawnData pawnData)
        {
            string disabledReason = "";
            bool disabled = false;
            if (__instance.Downed)
            {
                disabled = true;
                disabledReason = "WTH_Reason_Mechanoid_Downed".Translate();
            }
            else if(pawnData.shouldAutoRecharge)
            {
                Need_Power powerNeed = __instance.needs.TryGetNeed(WTH_DefOf.Mechanoid_Power) as Need_Power;
                if (powerNeed != null && powerNeed.CurCategory >= PowerCategory.LowPower)
                {
                    disabled = true;
                    disabledReason = "WTH_Reason_Power_Low".Translate();
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
                defaultDesc = "WTH_Gizmo_AutoRecharge_Label".Translate(),
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
