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
        private static object targetPawn;

        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
            if(store == null || !__instance.IsHacked())
            {
                return;
            }
            
            ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance);
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "test",
                defaultDesc = "test",
                icon = ContentFinder<Texture2D>.Get(("UI/" + "Enable_SD"), true),
                isActive = () => pawnData.isActive,
                toggleAction = () => {
                    pawnData.isActive = !pawnData.isActive;
                    if (pawnData.isActive)
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        if (__instance.GetLord() == null || __instance.GetLord().LordJob == null)
                        {
                            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), __instance.Map, new List<Pawn> { __instance });
                        }

                        if (__instance.equipment.Primary == null)
                        {
                            if (pawnData.carriedThing == null)
                            {
                                PawnWeaponGenerator.TryGenerateWeaponFor(__instance);//when the mod is added to an existing save, handle this properly by generate the missing weapon.
                                pawnData.carriedThing = __instance.equipment.Primary;
                            }
                            if (pawnData.carriedThing != null && pawnData.carriedThing.stackCount == 0)
                            {
                                pawnData.carriedThing.stackCount = 1;
                            }
                            Traverse.Create(__instance.equipment).Property("Primary").SetValue(pawnData.carriedThing);
                            //Traverse.Create(__instance).Method("set_Primary", new object[] { pawnData.carriedThing });
                        }
                    }
                    else
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        Building_MechanoidPlatform closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(__instance, __instance);
                        if(closestAvailablePlatform != null)
                        {
                            Job job = new Job(WTH_DefOf.Mechanoid_Rest, closestAvailablePlatform);
                            __instance.jobs.TryTakeOrderedJob(job);
                        }                      
                    }
                }
            };


            gizmoList.Add(gizmo);
            __result = gizmoList;
        }
    }
}
