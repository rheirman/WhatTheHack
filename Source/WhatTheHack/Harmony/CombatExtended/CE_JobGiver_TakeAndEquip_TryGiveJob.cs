using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch]
public class CE_JobGiver_TakeAndEquip_TryGiveJob
{
    public static MethodBase TargetMethod()
    {
        var assemblyCE = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly => assembly.FullName.StartsWith("CombatExtended"));
        var stub = typeof(CE_JobGiver_TakeAndEquip_TryGiveJob).GetMethod("Stub");
        if (assemblyCE == null)
        {
            return stub;
        }

        var type = assemblyCE.GetTypes().FirstOrDefault(t => t.Name == "JobGiver_TakeAndEquip");
        //Type type = assemblyCE.GetType("JobGiver_TakeAndEquip");
        if (type == null)
        {
            return stub;
        }

        var minfo = AccessTools.Method(type, "TryGiveJob");
        return minfo == null ? stub : minfo;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = new List<CodeInstruction>(instructions);
        foreach (var instruction in instructionsList)
        {
            if (instruction.operand as MethodInfo == typeof(Pawn).GetMethod("get_RaceProps"))
            {
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(CE_JobGiver_TakeAndEquip_TryGiveJob), "ShouldReload",
                        new[] { typeof(Pawn) })); //Injected code     
            }
            else if (instruction.operand as MethodInfo == AccessTools.Method(typeof(RaceProperties), "get_Humanlike"))
            {
                //Ommit this instruction
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static void Stub()
    {
        //This is patched when harmony can't find the CE method ShouldReload. 
    }

    public static bool ShouldReload(Pawn p)
    {
        //For mechanoids replace the check of is p.RaceProps.HumanLike by custom logic
        if (!p.IsMechanoid() || !p.IsHacked())
        {
            return p.RaceProps.Humanlike;
        }

        //return true when a mechanoid is hacked and does not have much ammo. 
        var inventory = TryGetCompByTypeName(p, "CompInventory");
        var eq = p.equipment.Primary;
        var shouldTransfer = false;
        if (eq == null)
        {
            eq = p.inventory.GetDirectlyHeldThings().FirstOrDefault() as ThingWithComps;
            shouldTransfer = eq != null;
        }

        if (inventory == null || eq == null)
        {
            return p.RaceProps.Humanlike;
        }

        //Everything is done using reflection, so we don't need to include a dependency
        var ammoUser = TryGetCompByTypeName(eq, "CompAmmoUser");
        if (ammoUser == null)
        {
            return p.RaceProps.Humanlike;
        }

        var currentAmmo = Traverse.Create(ammoUser).Property("CurrentAmmo").GetValue();
        var ammoCount = Traverse.Create(inventory).Method("AmmoCountOfDef", currentAmmo).GetValue<int>();
        var props = Traverse.Create(ammoUser).Property("Props").GetValue();
        var magazineSize = Traverse.Create(props).Field("magazineSize").GetValue<int>();
        var minAmmo = magazineSize == 0 ? 10 : magazineSize; //No magic numbers?
        if (ammoCount >= minAmmo)
        {
            return p.RaceProps.Humanlike;
        }

        if (shouldTransfer)
        {
            p.equipment.AddEquipment(eq.SplitOff(1) as ThingWithComps);
        }

        return true;
    }

    private static ThingComp TryGetCompByTypeName(ThingWithComps thing, string typeName)
    {
        return thing.AllComps.FirstOrDefault(comp => comp.GetType().Name == typeName);
    }
}