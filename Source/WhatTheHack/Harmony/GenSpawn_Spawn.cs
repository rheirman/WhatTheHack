using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(GenSpawn), "Spawn")]
[HarmonyPatch(new[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
internal class GenSpawn_Spawn
{
    private static bool Prefix(ref Thing newThing, bool respawningAfterLoad)
    {
        if (newThing is not Building_TurretGun || !respawningAfterLoad)
        {
            return true;
        }

        if (newThing.TryGetComp<CompMountable>() is not { Active: true } comp)
        {
            return true;
        }

        comp.mountedTo.inventory.innerContainer.TryAdd(newThing, 1);
        return false;
    }

    private static void Postfix(ref Thing newThing, bool respawningAfterLoad)
    {
        AddBatteryHediffIfNeeded(newThing);
        RemoveConditionalComps(newThing);
        if (respawningAfterLoad)
        {
            NameUnnamedMechs(newThing);
            AddOwnershipIfNeeded(newThing);
            //Log.Message("aa");
            if (newThing is Pawn p && p.IsHacked() && p.Faction == Faction.OfPlayer)
            {
                var storage = Base.Instance.GetExtendedDataStorage();
                if (storage != null)
                {
                    Utilities.InitWorkTypesAndSkills(p, storage.GetExtendedDataFor(p));
                }
            }
        }

        if (newThing.def == WTH_DefOf.WTH_TableMechanoidWorkshop)
        {
            LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Modification, OpportunityType.Important);
        }

        //disable mechs on map arrival
        if (respawningAfterLoad || newThing is not Pawn pawn || !pawn.IsHacked() || pawn.Faction != Faction.OfPlayer ||
            Base.Instance.GetExtendedDataStorage() is not { } store)
        {
            return;
        }

        var extendedPawnData = store.GetExtendedDataFor(pawn);
        extendedPawnData.isActive = false;
    }

    //Only initialize the refeulcomp of mechanoids that have a repairmodule. 
    private static void RemoveConditionalComps(Thing newThing)
    {
        if (newThing is not ThingWithComps thingWithComps)
        {
            return;
        }

        if (thingWithComps is Pawn pawn && pawn.IsMechanoid() && pawn.def.comps.Any())
        {
            Base.RemoveComps(pawn);
        }
    }

    private static void NameUnnamedMechs(Thing newThing)
    {
        if (newThing is Pawn pawn && pawn.IsHacked() && (pawn.Name == null || !pawn.Name.IsValid))
        {
            pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);
        }
    }

    //For compatbility, add the battery hediff to hacked mechs without one on load. 
    private static void AddBatteryHediffIfNeeded(Thing newThing)
    {
        if (newThing is not Pawn pawn || !pawn.IsHacked())
        {
            return;
        }

        if (pawn.health != null && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_BackupBattery))
        {
            pawn.health.AddHediff(WTH_DefOf.WTH_BackupBattery, pawn.TryGetReactor());
        }
    }

    //For compatbility, add the battery hediff to hacked mechs without one on load. 
    private static void AddOwnershipIfNeeded(Thing newThing)
    {
        if (newThing is not Pawn pawn || !pawn.IsHacked())
        {
            return;
        }

        if (pawn.ownership == null)
        {
            pawn.ownership = new Pawn_Ownership(pawn);
        }
    }


    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        foreach (var instruction in instructionsList)
        {
            if (instruction.operand as MethodInfo == typeof(GenSpawn).GetMethod("WipeExistingThings"))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    typeof(GenSpawn_Spawn).GetMethod("Modified_WipeExistingThings"));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static void Modified_WipeExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map,
        DestroyMode mode, Thing thing)
    {
        if (!(thing.TryGetComp<CompMountable>() is { Active: true }))
        {
            GenSpawn.WipeExistingThings(thingPos, thingRot, thingDef, map, mode);
        }
    }
}