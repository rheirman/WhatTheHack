using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using WhatTheHack.Comps;
using WhatTheHack.Needs;
using WhatTheHack.Storage;
using WhatTheHack.ThinkTree;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "GetGizmos")]
public class Pawn_GetGizmos
{
    public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
    {
        var gizmoList = __result.ToList();
        var store = Base.Instance.GetExtendedDataStorage();
        var isCreatureMine = __instance.Faction is { IsPlayer: true };

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
        var pawnData = store.GetExtendedDataFor(__instance);
        gizmoList.Add(CreateGizmo_SearchAndDestroy(__instance, pawnData));
        var powerNeed = __instance.needs.TryGetNeed<Need_Power>();
        var maintenanceNeed = __instance.needs.TryGetNeed<Need_Maintenance>();
        if (powerNeed != null)
        {
            gizmoList.Add(CreateGizmo_AutoRecharge(powerNeed));
        }

        var hediffSet = __instance.health.hediffSet;

        if (!__instance.IsColonistPlayerControlled)
        {
            if (__instance.apparel != null)
            {
                foreach (var apparelGizmo in __instance.apparel.GetGizmos())
                {
                    gizmoList.Add(apparelGizmo);
                }
            }
        }

        if (__instance.workSettings != null)
        {
            gizmoList.Add(CreateGizmo_Work(__instance, pawnData));
            if (powerNeed != null)
            {
                gizmoList.Add(CreateGizmo_WorkThreshold(powerNeed));
            }
        }

        if (maintenanceNeed != null)
        {
            gizmoList.Add(CreateGizmo_MaintenanceThreshold(maintenanceNeed));
        }

        if (hediffSet.HasHediff(WTH_DefOf.WTH_SelfDestruct))
        {
            gizmoList.Add(CreateGizmo_SelfDestruct(__instance));
        }

        if (hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule))
        {
            gizmoList.Add(CreateGizmo_SelfRepair(__instance));
        }

        if (hediffSet.HasHediff(WTH_DefOf.WTH_RepairModule) && hediffSet.HasHediff(WTH_DefOf.WTH_RepairArm))
        {
            gizmoList.Add(CreateGizmo_Repair(__instance));
        }

        if (hediffSet.HasHediff(WTH_DefOf.WTH_BeltModule))
        {
            gizmoList.Add(CreateGizmo_EquipBelt(__instance));
        }

        if (hediffSet.HasHediff(WTH_DefOf.WTH_OverdriveModule))
        {
            gizmoList.Add(CreateGizmo_Overdrive(__instance));
        }
    }

    private static TargetingParameters GetTargetingParametersForTurret()
    {
        return new TargetingParameters
        {
            canTargetPawns = false,
            canTargetBuildings = true,
            mapObjectTargetsMustBeAutoAttackable = false,
            validator = delegate(TargetInfo targ)
            {
                if (!targ.HasThing)
                {
                    return false;
                }

                var building = targ.Thing as Building;
                return building != null && building.TryGetComp<CompMountable>() != null;
            }
        };
    }

    private static Gizmo CreateGizmo_Work(Pawn __instance, ExtendedPawnData pawnData)
    {
        var disabledReason = "";
        Gizmo gizmo = new Command_Toggle
        {
            defaultLabel = "WTH_Gizmo_Work_Label".Translate(),
            defaultDesc = "WTH_Gizmo_Work_Description".Translate(),
            disabled = false,
            disabledReason = disabledReason,
            icon = ContentFinder<Texture2D>.Get("UI/" + "MechanoidWork"),
            isActive = () => pawnData.canWorkNow,
            toggleAction = () =>
            {
                pawnData.canWorkNow = !pawnData.canWorkNow;
                if (__instance.CurJob.def != WTH_DefOf.WTH_Mechanoid_Rest)
                {
                    __instance.jobs.EndCurrentJob(JobCondition.InterruptForced);
                }
            }
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_WorkThreshold(Need_Power powerNeed)
    {
        var disabledReason = "";
        Gizmo gizmo = new Command_SetWorkThreshold
        {
            powerNeed = powerNeed,
            defaultLabel = "WTH_Gizmo_WorkThreshold_Label".Translate(),
            defaultDesc = "WTH_Gizmo_WorkThreshold_Description".Translate(),
            disabled = false,
            disabledReason = disabledReason,
            icon = ContentFinder<Texture2D>.Get("UI/" + "MechanoidWorkThreshold")
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_MaintenanceThreshold(Need_Maintenance maintenanceNeed)
    {
        var disabledReason = "";
        Gizmo gizmo = new Command_SetMaintenanceThreshold
        {
            maintenanceNeed = maintenanceNeed,
            defaultLabel = "WTH_Gizmo_MaintenanceThreshold_Label".Translate(),
            defaultDesc = "WTH_Gizmo_MaintenanceThreshold_Description".Translate(),
            disabled = false,
            disabledReason = disabledReason,
            icon = ContentFinder<Texture2D>.Get("UI/" + "MechMaintenanceThreshold")
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_SearchAndDestroy(Pawn __instance, ExtendedPawnData pawnData)
    {
        var disabledReason = "";
        var disabled = false;
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
            icon = ContentFinder<Texture2D>.Get("UI/" + "Enable_SD"),
            isActive = () => pawnData.isActive,
            toggleAction = () =>
            {
                pawnData.isActive = !pawnData.isActive;
                if (pawnData.isActive)
                {
                    if (__instance.GetLord() == null || __instance.GetLord().LordJob == null)
                    {
                        LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_SearchAndDestroy(), __instance.Map,
                            new List<Pawn> { __instance });
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
                    var closestAvailablePlatform = Utilities.GetAvailableMechanoidPlatform(__instance, __instance);
                    if (closestAvailablePlatform == null)
                    {
                        return;
                    }

                    var job = new Job(WTH_DefOf.WTH_Mechanoid_Rest, closestAvailablePlatform);
                    __instance.jobs.TryTakeOrderedJob(job);
                }
            }
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_AutoRecharge(Need_Power powerNeed)
    {
        Gizmo gizmo = new Command_Toggle
        {
            defaultLabel = "WTH_Gizmo_AutoRecharge_Label".Translate(),
            defaultDesc = "WTH_Gizmo_AutoRecharge_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/" + "AutoRecharge"),
            isActive = () => powerNeed.shouldAutoRecharge,
            toggleAction = () => { powerNeed.shouldAutoRecharge = !powerNeed.shouldAutoRecharge; }
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_SelfDestruct(Pawn pawn)
    {
        var powerNeed = pawn.needs.TryGetNeed<Need_Power>();

        var jobDef = WTH_DefOf.WTH_Ability_SelfDestruct;
        var modExt = jobDef.GetModExtension<DefModExtension_Ability>();

        var needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
        var notActicated = !pawn.IsActivated();
        var isDisabled = needsMorePower || notActicated;
        var disabledReason = "";
        if (isDisabled)
        {
            disabledReason = notActicated
                ? "WTH_Reason_NotActivated".Translate()
                : "WTH_Reason_NeedsMorePower".Translate(modExt.powerDrain);
        }

        Gizmo gizmo = new Command_Action
        {
            defaultLabel = "WTH_Gizmo_SelfDestruct_Label".Translate(),
            defaultDesc = "WTH_Gizmo_SelfDestruct_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/" + "Detonate"),
            disabled = isDisabled,
            disabledReason = disabledReason,
            action = delegate
            {
                var job = new Job(WTH_DefOf.WTH_Ability_SelfDestruct, pawn);
                pawn.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        };
        return gizmo;
    }


    private static Gizmo CreateGizmo_SelfRepair(Pawn __instance)
    {
        var compRefuelable = __instance.GetComp<CompRefuelable>();
        var powerNeed = __instance.needs.TryGetNeed<Need_Power>();
        var jobDef = WTH_DefOf.WTH_Ability_Repair;
        var modExt = jobDef.GetModExtension<DefModExtension_Ability>();

        var alreadyRepairing = __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing);
        var needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
        var needsMoreFuel = compRefuelable.Fuel < modExt.fuelDrain;
        var notActicated = !__instance.IsActivated();
        var noDamage = !__instance.health.hediffSet.HasNaturallyHealingInjury();
        var isDisabled = alreadyRepairing || needsMorePower || needsMoreFuel || notActicated || noDamage;
        var disabledReason = "";
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
                disabledReason = "WTH_Reason_NeedsMorePower".Translate(modExt.powerDrain);
            }
            else
            {
                disabledReason = "WTH_Reason_NeedsMoreFuel".Translate(modExt.fuelDrain);
            }
        }

        Gizmo gizmo = new Command_Action
        {
            defaultLabel = "WTH_Gizmo_SelfRepair_Label".Translate(),
            defaultDesc = "WTH_Gizmo_SelfRepair_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get("Things/" + "Mote_HealingCrossGreen"),
            disabled = isDisabled,
            disabledReason = disabledReason,
            action = delegate
            {
                var job = new Job(WTH_DefOf.WTH_Ability_Repair, __instance);
                __instance.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_Repair(Pawn __instance)
    {
        var compRefuelable = __instance.GetComp<CompRefuelable>();
        var powerNeed = __instance.needs.TryGetNeed<Need_Power>();

        var jobDef = WTH_DefOf.WTH_Ability_Repair;
        var modExt = jobDef.GetModExtension<DefModExtension_Ability>();
        var alreadyRepairing = __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Repairing);
        var needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
        var needsMoreFuel = compRefuelable.Fuel < modExt.fuelDrain;
        var notActicated = !__instance.IsActivated();

        var isDisabled = needsMorePower || needsMoreFuel || notActicated;
        var disabledReason = "";
        if (isDisabled)
        {
            if (needsMorePower)
            {
                disabledReason = "WTH_Reason_NeedsMorePower".Translate(modExt.powerDrain);
            }
            else if (needsMoreFuel)
            {
                disabledReason = "WTH_Reason_NeedsMoreFuel".Translate(modExt.fuelDrain);
            }
            else
            {
                disabledReason = "WTH_Reason_NotActivated".Translate();
            }
        }

        Gizmo gizmo = new Command_Target
        {
            defaultLabel = "WTH_Gizmo_Mech_Repair_Label".Translate(),
            defaultDesc = "WTH_Gizmo_Mech_Repair_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get("Things/" + "Mote_HealingCrossBlue"), //TODO: other icon
            disabled = isDisabled,
            targetingParams = GetTargetingParametersForRepairing(),
            disabledReason = disabledReason,
            action = delegate(LocalTargetInfo target)
            {
                if (target is not { HasThing: true, Thing: Pawn })
                {
                    return;
                }

                var job = new Job(WTH_DefOf.WTH_Ability_Repair, target);
                __instance.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_Overdrive(Pawn __instance)
    {
        var powerNeed = __instance.needs.TryGetNeed<Need_Power>();

        var jobDef = WTH_DefOf.WTH_Ability_Overdrive;
        var modExt = jobDef.GetModExtension<DefModExtension_Ability>();
        var alreadyOverdriving = __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Overdrive) ||
                                 __instance.health.hediffSet.HasHediff(WTH_DefOf.WTH_Overdrive_GoneTooFar);
        var needsMorePower = powerNeed.CurLevel < modExt.powerDrain;
        var notActicated = !__instance.IsActivated();

        var isDisabled = needsMorePower || notActicated || alreadyOverdriving;
        var disabledReason = "";
        if (isDisabled)
        {
            if (alreadyOverdriving)
            {
                disabledReason = "WTH_Reason_AlreadyOverdriving".Translate();
            }
            else if (needsMorePower)
            {
                disabledReason = "WTH_Reason_NeedsMorePower".Translate(modExt.powerDrain);
            }
            else
            {
                disabledReason = "WTH_Reason_NotActivated".Translate();
            }
        }

        Gizmo gizmo = new Command_Action
        {
            defaultLabel = "WTH_Gizmo_Overdrive_Label".Translate(),
            defaultDesc = "WTH_Gizmo_Overdrive_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get("Things/" + "OverdriveModule"),
            disabled = isDisabled,
            disabledReason = disabledReason,
            action = delegate
            {
                var job = new Job(WTH_DefOf.WTH_Ability_Overdrive, __instance);
                __instance.jobs.StartJob(job, JobCondition.InterruptForced);
            }
        };
        return gizmo;
    }

    private static Gizmo CreateGizmo_EquipBelt(Pawn pawn)
    {
        var notActicated = !pawn.IsActivated();
        var disabledReason = "";
        if (notActicated)
        {
            disabledReason = "WTH_Reason_NotActivated".Translate();
        }

        Gizmo gizmo = new Command_Target
        {
            defaultLabel = "WTH_Gizmo_Mech_EquipBelt_Label".Translate(),
            defaultDesc = "WTH_Gizmo_Mech_EquipBelt_Description".Translate(),
            icon = ContentFinder<Texture2D>.Get(
                "Things/Pawn/Humanlike/Apparel/ShieldBelt/ShieldBelt"), //TODO: other icon
            disabled = notActicated,
            targetingParams = GetTargetingParametersForEquipBelt(pawn),
            disabledReason = disabledReason,
            action = delegate(LocalTargetInfo target)
            {
                if (target is not { HasThing: true, Thing: Apparel apparel } || !Utilities.IsBelt(apparel.def.apparel))
                {
                    return;
                }

                apparel.SetForbidden(false);
                var job = new Job(JobDefOf.Wear, apparel);
                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
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

            validator = delegate(TargetInfo targ)
            {
                if (!targ.HasThing)
                {
                    return false;
                }

                var pawn = targ.Thing as Pawn;
                return pawn is { Downed: false }
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
            validator = delegate(TargetInfo targ)
            {
                if (!targ.HasThing)
                {
                    return false;
                }

                if (targ.Thing is not Apparel apparel)
                {
                    return false;
                }

                if (!pawn.HasReplacedAI() && apparel.def == WTH_DefOf.WTH_Apparel_MechControllerBelt)
                {
                    return false;
                }

                return Utilities.IsBelt(apparel.def.apparel) &&
                       pawn.CanReach(apparel, PathEndMode.ClosestTouch, Danger.Deadly);
            }
        };
    }
}