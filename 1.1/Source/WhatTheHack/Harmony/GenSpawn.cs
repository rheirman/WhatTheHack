using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Comps;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(GenSpawn), "Spawn")]
    [HarmonyPatch(new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool) })]
    class GenSpawn_Spawn
    {
        static bool Prefix(ref Thing newThing, ref WipeMode wipeMode, bool respawningAfterLoad)
        {
            if (newThing is Building_TurretGun && respawningAfterLoad)
            {
                if (newThing.TryGetComp<CompMountable>() is CompMountable comp && comp.Active)
                {
                    comp.mountedTo.inventory.innerContainer.TryAdd(newThing, 1);
                    return false;
                }
            }
            return true;
        }

        static void Postfix(ref Thing newThing, bool respawningAfterLoad)
        {

            AddBatteryHediffIfNeeded(newThing);
            RemoveConditionalComps(newThing);
            if (respawningAfterLoad)
            {
                NameUnnamedMechs(newThing);
                AddOwnershipIfNeeded(newThing);
                //Log.Message("aa");
                if(newThing is Pawn p && p.IsHacked())
                {
                    var storage = Base.Instance.GetExtendedDataStorage();
                    if(storage != null)
                    {
                        
                        Utilities.InitWorkTypesAndSkills(p, storage.GetExtendedDataFor(p));
                    }
                }
            }
            if(newThing.def == WTH_DefOf.WTH_TableMechanoidWorkshop)
            {
                LessonAutoActivator.TeachOpportunity(WTH_DefOf.WTH_Modification, OpportunityType.Important);
            }
            //disable mechs on map arrival
            if (!respawningAfterLoad && newThing is Pawn pawn && pawn.IsHacked() && pawn.Faction == Faction.OfPlayer && Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
            {
                ExtendedPawnData extendedPawnData = store.GetExtendedDataFor(pawn);
                extendedPawnData.isActive = false;
            }
        }

        //Only initialize the refeulcomp of mechanoids that have a repairmodule. 
        private static void RemoveConditionalComps(Thing newThing)
        {
            if (!(newThing is ThingWithComps thingWithComps))
            {
                return;
            }
            if (thingWithComps is Pawn && ((Pawn)thingWithComps).RaceProps.IsMechanoid && thingWithComps.def.comps.Any<CompProperties>())
            {
                Base.RemoveComps(thingWithComps);
            }
        }
        private static void NameUnnamedMechs(Thing newThing)
        {
            if(newThing is Pawn pawn && pawn.IsHacked() && (pawn.Name == null || !pawn.Name.IsValid))
            {
                pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full);
            }
        }

        //For compatbility, add the battery hediff to hacked mechs without one on load. 
        private static void AddBatteryHediffIfNeeded(Thing newThing)
        {
            if (newThing is Pawn pawn && pawn.IsHacked())
            {
                if (pawn.health != null && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_BackupBattery))
                {
                    pawn.health.AddHediff(WTH_DefOf.WTH_BackupBattery, pawn.TryGetReactor());
                }
            }
        }

        //For compatbility, add the battery hediff to hacked mechs without one on load. 
        private static void AddOwnershipIfNeeded(Thing newThing)
        {
            if (newThing is Pawn pawn && pawn.IsHacked())
            {
                if(pawn.ownership == null)
                {
                    pawn.ownership = new Pawn_Ownership(pawn);            
                }
            }
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction instruction in instructionsList)
            {
                if(instruction.operand as MethodInfo == typeof(GenSpawn).GetMethod("WipeExistingThings"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, typeof(GenSpawn_Spawn).GetMethod("Modified_WipeExistingThings"));
                }
                else
                {
                    yield return instruction;
                }

            }
        }

        public static void Modified_WipeExistingThings(IntVec3 thingPos, Rot4 thingRot, BuildableDef thingDef, Map map, DestroyMode mode, Thing thing)
        {
            if (!(thing.TryGetComp<CompMountable>() is CompMountable comp && comp.Active))
            {
                GenSpawn.WipeExistingThings(thingPos, thingRot, thingDef, map, mode);
            }

        }
    }

}