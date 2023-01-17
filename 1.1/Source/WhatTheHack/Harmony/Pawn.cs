using HarmonyLib;
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
using WhatTheHack.Comps;
using WhatTheHack.ThinkTree;
using WhatTheHack.Jobs;
using WhatTheHack.Needs;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Pawn), "DropAndForbidEverything")]
    class Pawn_DropAndForbidEverything
    {
        static bool Prefix(Pawn __instance)
        {
            if (__instance.RaceProps.IsMechanoid && !__instance.Dead)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "GetDisabledWorkTypes")]
    class Pawn_GetDisabledWorkTypes
    {
        static bool Prefix(Pawn __instance, ref List<WorkTypeDef> __result)
        {
            if (__instance.IsHacked())
            {
                List<WorkTypeDef> shouldForbid = new List<WorkTypeDef>();
                ExtendedDataStorage store = Base.Instance.GetExtendedDataStorage();
                if (store != null)
                {
                    ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(__instance);
                    foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs)
                    {
                        if (pawnData.workTypes == null || !pawnData.workTypes.Contains(def))
                        {
                            shouldForbid.Add(def);
                        }
                    }
                    __result = shouldForbid;
                    return false;
                }
                
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    static class Pawn_Kill
    {
        static void Prefix(Pawn __instance)
        {

            if (__instance.RaceProps.IsMechanoid)
            {
                if (__instance.relations == null)
                {
                    __instance.relations = new Pawn_RelationsTracker(__instance);
                }
                __instance.RemoveAllLinks();
            }
        }
    }


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

            if(bill != null && bill.recipe.HasModExtension<DefModExtension_Recipe>() && __instance.InteractionCell.IsValid)
            {
                if (bill.recipe.GetModExtension<DefModExtension_Recipe>().requireBed == false || __instance.OnHackingTable())
                {
                    __result = true;
                }
                else
                {
                    JobFailReason.Is("WTH_Reason_NotOnTable".Translate(), null);
                    __result = false;
                }
            }
        }
    }

    
    [HarmonyPatch(typeof(Pawn), "get_IsColonistPlayerControlled")]
    public class Pawn_get_IsColonistPlayerControlled
    {
        public static bool Prefix(Pawn __instance, ref bool __result)
        {
            if (__instance.HasReplacedAI() || IsControlled(__instance))
            {
                if (__instance.Faction == Faction.OfPlayer && __instance.IsHacked() && !__instance.Dead)
                {
                    __result = true;
                    return false;
                }
            }
            int test = Find.TickManager.TicksGame;
            return true;
        }

        private static bool IsControlled(Pawn pawn)
        {
            if (!pawn.RaceProps.IsMechanoid)
            {
                return false;
            }
            if(pawn.RemoteControlLink() != null && !pawn.RemoteControlLink().Drafted)
            {
                float radius = Utilities.GetRemoteControlRadius(pawn.RemoteControlLink());
                return pawn.Position.DistanceToSquared(pawn.RemoteControlLink().Position) <= radius * radius;
            }
            if(pawn.ControllingAI() != null)
            {
                return true;
            }
            return false;
        }
    }

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
            Need_Power powerNeed = __instance.needs.TryGetNeed<Need_Power>();
            Need_Maintenance maintenanceNeed = __instance.needs.TryGetNeed<Need_Maintenance>();
            if (powerNeed != null)
            {
                gizmoList.Add(CreateGizmo_AutoRecharge(__instance, powerNeed));
            }
            HediffSet hediffSet = __instance.health.hediffSet;

            if (!__instance.IsColonistPlayerControlled)
            {
                if (__instance.apparel != null)
                {
                    foreach (Gizmo apparelGizmo in __instance.apparel.GetGizmos())
                    {
                        gizmoList.Add(apparelGizmo);
                    }
                }
            }
            if(__instance.workSettings != null)
            {
                gizmoList.Add(CreateGizmo_Work(__instance, pawnData));
                if(powerNeed != null)
                {
                    gizmoList.Add(CreateGizmo_WorkThreshold(__instance, powerNeed));
                }
            }
            if(maintenanceNeed != null)
            {
                gizmoList.Add(CreateGizmo_MaintenanceThreshold(__instance, maintenanceNeed));
            }
            if (hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestruct))
            {
                gizmoList.Add(CreateGizmo_SelfDestruct(__instance, pawnData));
            }
            if (hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule))
            {
                gizmoList.Add(CreateGizmo_SelfRepair(__instance, pawnData));
            }
            if(hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) && hediffSet.HasHediff(WTH_DefOf.WTH_RepairArm))
            {
                gizmoList.Add(CreateGizmo_Repair(__instance, pawnData));
            }
            if (hediffSet.HasHediff(WTH_DefOf.WTH_BeltModule))
            {
                gizmoList.Add(CreateGizmo_EquipBelt(__instance, pawnData));
            }
            if (hediffSet.HasHediff(WTH_DefOf.WTH_OverdriveModule))
            {
                gizmoList.Add(CreateGizmo_Overdrive(__instance, pawnData));
            }
        }
        private static TargetingParameters GetTargetingParametersForTurret()
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetBuildings = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    Building building = targ.Thing as Building;
                    return building != null && building.TryGetComp<CompMountable>() != null;
                }
            };
        }

        private static Gizmo CreateGizmo_Work(Pawn __instance, ExtendedPawnData pawnData)
        {
            string disabledReason = "";
            bool disabled = false;
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "WTH_Gizmo_Work_Label".Translate(),
                defaultDesc = "WTH_Gizmo_Work_Description".Translate(),
                disabled = disabled,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "MechanoidWork"), true),
                isActive = () => pawnData.canWorkNow,
                toggleAction = () =>
                {
                    pawnData.canWorkNow = !pawnData.canWorkNow;
                    if(__instance.CurJob.def != WTH_DefOf.WTH_Mechanoid_Rest)
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                    }
                }
            };
            return gizmo;        
        }
        private static Gizmo CreateGizmo_WorkThreshold(Pawn __instance, Need_Power powerNeed)
        {
            string disabledReason = "";
            bool disabled = false;
            Gizmo gizmo = new Command_SetWorkThreshold
            {
                powerNeed = powerNeed,
                defaultLabel = "WTH_Gizmo_WorkThreshold_Label".Translate(),
                defaultDesc = "WTH_Gizmo_WorkThreshold_Description".Translate(),
                disabled = disabled,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "MechanoidWorkThreshold"), true),
                
            };
            return gizmo;
        }

        private static Gizmo CreateGizmo_MaintenanceThreshold(Pawn __instance, Need_Maintenance maintenanceNeed)
        {
            string disabledReason = "";
            bool disabled = false;
            Gizmo gizmo = new Command_SetMaintenanceThreshold
            {
                maintenanceNeed = maintenanceNeed,
                defaultLabel = "WTH_Gizmo_MaintenanceThreshold_Label".Translate(),
                defaultDesc = "WTH_Gizmo_MaintenanceThreshold_Description".Translate(),
                disabled = disabled,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "MechMaintenanceThreshold"), true),
            };
            return gizmo;
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
            else if (__instance.ShouldRecharge())
            {
                disabled = true;
                disabledReason = "WTH_Reason_PowerLow".Translate();
            }
            else if (__instance.ShouldBeMaintained())
            {
                disabled = true;
                disabledReason = "WTH_Reason_MaintenanceLow".Translate();
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
                        if (__instance.GetLord() == null || __instance.GetLord().LordJob == null)
                        {
                            LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), __instance.Map, new List<Pawn> { __instance });
                        }
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        if (__instance.relations == null) //Added here to fix existing saves. 
                        {
                            __instance.relations = new Pawn_RelationsTracker(__instance);
                        }
                    }
                    else
                    {
                        __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                        Building_BaseMechanoidPlatform closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(__instance, __instance);
                        if (closestAvailablePlatform != null)
                        {
                            Job job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, closestAvailablePlatform);
                            __instance.jobs.TryTakeOrderedJob(job);
                        }
                    }
                }
            };
            return gizmo;
        }
        private static Gizmo CreateGizmo_AutoRecharge(Pawn __instance, Need_Power powerNeed)
        {

            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "WTH_Gizmo_AutoRecharge_Label".Translate(),
                defaultDesc = "WTH_Gizmo_AutoRecharge_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("UI/" + "AutoRecharge"), true),
                isActive = () => powerNeed.shouldAutoRecharge,
                toggleAction = () =>
                {
                    powerNeed.shouldAutoRecharge = !powerNeed.shouldAutoRecharge;
                }
            };
            return gizmo;
        }

        private static Gizmo CreateGizmo_SelfDestruct(Pawn pawn, ExtendedPawnData pawnData)
        {
            Need_Power powerNeed = pawn.needs.TryGetNeed<Need_Power>();

            JobDef jobDef = WTH_DefOf.WTH_Ability_SelfDestruct;
            DefModExtension_Ability modExt = jobDef.GetModExtension<DefModExtension_Ability>();

            bool needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
            bool notActicated = !pawn.IsActivated();
            bool isDisabled = needsMorePower || notActicated;
            string disabledReason = "";
            if (isDisabled)
            {
                if (notActicated)
                {
                    disabledReason = "WTH_Reason_NotActivated".Translate();
                }
                else if (needsMorePower)
                {
                    disabledReason = "WTH_Reason_NeedsMorePower".Translate(new object[] { modExt.powerDrain });
                }
            }

            Gizmo gizmo = new Command_Action
            {
                defaultLabel = "WTH_Gizmo_SelfDestruct_Label".Translate(),
                defaultDesc = "WTH_Gizmo_SelfDestruct_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("UI/" + "Detonate"), true),
                disabled = isDisabled,
                disabledReason = disabledReason,
                action = delegate
                {
                    Job job = new Job(WTH_DefOf.WTH_Ability_SelfDestruct, pawn);
                    pawn.jobs.StartJob(job, JobCondition.InterruptForced);
                }
            };
            return gizmo;
        }


        private static Gizmo CreateGizmo_SelfRepair(Pawn __instance, ExtendedPawnData pawnData)
        {
            CompRefuelable compRefuelable = __instance.GetComp<CompRefuelable>();
            Need_Power powerNeed = __instance.needs.TryGetNeed<Need_Power>();
            JobDef jobDef = WTH_DefOf.WTH_Ability_Repair;
            DefModExtension_Ability modExt = jobDef.GetModExtension<DefModExtension_Ability>();

            bool alreadyRepairing = __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing);
            bool needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
            bool needsMoreFuel = compRefuelable.Fuel < modExt.fuelDrain;
            bool notActicated = !__instance.IsActivated();
            bool noDamage = !__instance.health.hediffSet.HasNaturallyHealingInjury();
            bool isDisabled = alreadyRepairing || needsMorePower || needsMoreFuel || notActicated || noDamage;
            string disabledReason = "";
            if (isDisabled)
            {
                if (alreadyRepairing)
                {
                    disabledReason = "WTH_Reason_AlreadyRepairing".Translate();
                }
                else if (notActicated)
                {
                    disabledReason = "WTH_Reason_NotActivated".Translate();
                }
                else if (noDamage)
                {
                    disabledReason = "WTH_Reason_NoDamage".Translate();
                }
                else if (needsMorePower)
                {
                    disabledReason = "WTH_Reason_NeedsMorePower".Translate(new object[] { modExt.powerDrain });
                }
                else if (needsMoreFuel)
                {
                    disabledReason = "WTH_Reason_NeedsMoreFuel".Translate(new object[] { modExt.fuelDrain });
                }

            }

            Gizmo gizmo = new Command_Action
            {
                defaultLabel = "WTH_Gizmo_SelfRepair_Label".Translate(),
                defaultDesc = "WTH_Gizmo_SelfRepair_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("Things/" + "Mote_HealingCrossGreen"), true),
                disabled = isDisabled,
                disabledReason = disabledReason,
                action = delegate
                {
                    Job job = new Job(WTH_DefOf.WTH_Ability_Repair, __instance);
                    __instance.jobs.StartJob(job, JobCondition.InterruptForced);
                }
            };
            return gizmo;
        }
        private static Gizmo CreateGizmo_Repair(Pawn __instance, ExtendedPawnData pawnData)
        {
            CompRefuelable compRefuelable = __instance.GetComp<CompRefuelable>();
            Need_Power powerNeed = __instance.needs.TryGetNeed<Need_Power>();

            JobDef jobDef = WTH_DefOf.WTH_Ability_Repair;
            DefModExtension_Ability modExt = jobDef.GetModExtension<DefModExtension_Ability>();
            bool alreadyRepairing = __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing);
            bool needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
            bool needsMoreFuel = compRefuelable.Fuel < modExt.fuelDrain;
            bool notActicated = !__instance.IsActivated();

            bool isDisabled = needsMorePower || needsMoreFuel || notActicated;
            string disabledReason = "";
            if (isDisabled)
            {
                if (needsMorePower)
                {
                    disabledReason = "WTH_Reason_NeedsMorePower".Translate(new object[] { modExt.powerDrain });
                }
                else if (needsMoreFuel)
                {
                    disabledReason = "WTH_Reason_NeedsMoreFuel".Translate(new object[] { modExt.fuelDrain });
                }
                else if (notActicated)
                {
                    disabledReason = "WTH_Reason_NotActivated".Translate();
                }
            }

            Gizmo gizmo = new Command_Target
            {
                defaultLabel = "WTH_Gizmo_Mech_Repair_Label".Translate(),
                defaultDesc = "WTH_Gizmo_Mech_Repair_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("Things/" + "Mote_HealingCrossBlue"), true), //TODO: other icon
                disabled = isDisabled,
                targetingParams = GetTargetingParametersForRepairing(),
                disabledReason = disabledReason,
                action = delegate(Thing target) {
                    if (target is Pawn mech)
                    {
                        Job job = new Job(WTH_DefOf.WTH_Ability_Repair, target);
                        __instance.jobs.StartJob(job, JobCondition.InterruptForced);
                    }

                }
            };
            return gizmo;
        }

        private static Gizmo CreateGizmo_Overdrive(Pawn __instance, ExtendedPawnData pawnData)
        {
            Need_Power powerNeed = __instance.needs.TryGetNeed<Need_Power>();

            JobDef jobDef = WTH_DefOf.WTH_Ability_Overdrive;
            DefModExtension_Ability modExt = jobDef.GetModExtension<DefModExtension_Ability>();
            bool alreadyOverdriving = __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Overdrive) || __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Overdrive_GoneTooFar);
            bool needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
            bool notActicated = !__instance.IsActivated();

            bool isDisabled = needsMorePower || notActicated || alreadyOverdriving;
            string disabledReason = "";
            if (isDisabled)
            {
                if (alreadyOverdriving)
                {
                    disabledReason = "WTH_Reason_AlreadyOverdriving".Translate();
                }
                else if (needsMorePower)
                {
                    disabledReason = "WTH_Reason_NeedsMorePower".Translate(new object[] { modExt.powerDrain });
                }
                else if (notActicated)
                {
                    disabledReason = "WTH_Reason_NotActivated".Translate();
                }
            }

            Gizmo gizmo = new Command_Action
            {
                defaultLabel = "WTH_Gizmo_Overdrive_Label".Translate(),
                defaultDesc = "WTH_Gizmo_Overdrive_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("Things/" + "OverdriveModule"), true),
                disabled = isDisabled,
                disabledReason = disabledReason,
                action = delegate
                {
                    Job job = new Job(WTH_DefOf.WTH_Ability_Overdrive, __instance);
                    __instance.jobs.StartJob(job, JobCondition.InterruptForced);
                }
            };
            return gizmo;
        }

        private static Gizmo CreateGizmo_EquipBelt(Pawn pawn, ExtendedPawnData pawnData)
        {
            bool notActicated = !pawn.IsActivated();
            bool isDisabled = notActicated;
            string disabledReason = "";
            if (isDisabled)
            {
                if (notActicated)
                {
                    disabledReason = "WTH_Reason_NotActivated".Translate();
                }
            }

            Gizmo gizmo = new Command_Target
            {
                defaultLabel = "WTH_Gizmo_Mech_EquipBelt_Label".Translate(),
                defaultDesc = "WTH_Gizmo_Mech_EquipBelt_Description".Translate(),
                icon = ContentFinder<Texture2D>.Get(("Things/Pawn/Humanlike/Apparel/ShieldBelt/ShieldBelt"), true), //TODO: other icon
                disabled = isDisabled,
                targetingParams = GetTargetingParametersForEquipBelt(pawn),
                disabledReason = disabledReason,
                action = delegate (Thing target) {
                    if (target is Apparel apparel && Utilities.IsBelt(apparel.def.apparel))
                    {
                        apparel.SetForbidden(false, true);
                        Job job = new Job(JobDefOf.Wear, apparel);
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }

                }
            };
            return gizmo;
        }



        private static TargetingParameters GetTargetingParametersForRepairing()
        {
            return new TargetingParameters
            {
                canTargetPawns = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }
                    Pawn pawn = targ.Thing as Pawn;
                    return pawn != null 
                    && !pawn.Downed
                    && pawn.IsHacked() 
                    && pawn.health != null
                    && pawn.health.hediffSet.HasNaturallyHealingInjury()
                    && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing);
                }
            };
        }
        private static TargetingParameters GetTargetingParametersForEquipBelt(Pawn pawn)
        {
            return new TargetingParameters
            {
                canTargetPawns = false,
                canTargetItems = true,
                canTargetBuildings = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = delegate (TargetInfo targ)
                {
                    if (!targ.HasThing)
                    {
                        return false;
                    }         
                    Apparel apparel = targ.Thing as Apparel;
                    if(apparel == null)
                    {
                        return false;
                    }
                    if(!pawn.HasReplacedAI() && apparel.def == WTH_DefOf.WTH_Apparel_MechControllerBelt)
                    {
                        return false;
                    }
                    return Utilities.IsBelt(apparel.def.apparel) && pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly);
                }
            };
        }
    }
}
