using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Comps;

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
                Base.RemoveComps(ref thingWithComps);
            }
        }
        //For backwards compatbility, add the battery hediff to hacked mechs without one on load. 
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

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            foreach (CodeInstruction instruction in instructionsList)
            {
                if(instruction.operand == typeof(GenSpawn).GetMethod("WipeExistingThings"))
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