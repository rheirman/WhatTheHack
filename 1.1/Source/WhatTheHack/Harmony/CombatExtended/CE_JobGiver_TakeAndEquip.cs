using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch]
    public class CE_JobGiver_TakeAndEquip_TryGiveJob
    {
        public static MethodBase TargetMethod()
        {
            Assembly assemblyCE = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly assembly) => assembly.FullName.StartsWith("CombatExtended"));
            MethodInfo stub = typeof(CE_JobGiver_TakeAndEquip_TryGiveJob).GetMethod("Stub");
            if (assemblyCE == null)
            {
                return stub;
            }
            Type type = assemblyCE.GetTypes().FirstOrDefault((Type t) => t.Name == "JobGiver_TakeAndEquip");
            //Type type = assemblyCE.GetType("JobGiver_TakeAndEquip");
            if(type == null)
            {
                return stub;
            }
            MethodInfo minfo = AccessTools.Method(type, "TryGiveJob");
            if(minfo == null)
            {
                return stub;
            }
            return minfo;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (instruction.operand as MethodInfo == typeof(Pawn).GetMethod("get_RaceProps"))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CE_JobGiver_TakeAndEquip_TryGiveJob), "ShouldReload", new Type[] { typeof(Pawn) }));//Injected code     
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
            if (p.RaceProps.IsMechanoid && p.IsHacked())
            {
                //return true when a mechanoid is hacked and does not have much ammo. 
                ThingComp inventory = TryGetCompByTypeName(p, "CompInventory", "CombatExtended");
                ThingWithComps eq = p.equipment.Primary;
                bool shouldTransfer = false;
                if(eq == null)
                {
                    eq = p.inventory.GetDirectlyHeldThings().FirstOrDefault() as ThingWithComps;
                    shouldTransfer = eq == null ? false : true;
                }
                if (inventory != null && eq != null)
                {
                    //Everything is done using reflection, so we don't need to include a dependency
                    ThingComp ammoUser = TryGetCompByTypeName(eq, "CompAmmoUser", "CombatExtended");
                    if (ammoUser != null)
                    {
                        var currentAmmo = Traverse.Create(ammoUser).Property("CurrentAmmo").GetValue();
                        int ammoCount = Traverse.Create(inventory).Method("AmmoCountOfDef", new object[] { currentAmmo }).GetValue<int>();
                        var props = Traverse.Create(ammoUser).Property("Props").GetValue();
                        int magazineSize = Traverse.Create(props).Field("magazineSize").GetValue<int>();
                        int minAmmo = magazineSize == 0 ? 10 : magazineSize; //No magic numbers?
                        if (ammoCount < minAmmo)
                        {
                            if (shouldTransfer)
                            {
                                p.equipment.AddEquipment(eq.SplitOff(1) as ThingWithComps);
                            }
                            return true;
                        }
                    }
                }
            }
            return p.RaceProps.Humanlike;
        }
        private static ThingComp TryGetCompByTypeName(ThingWithComps thing, string typeName, string assemblyName = "")
        {
            return thing.AllComps.FirstOrDefault((ThingComp comp) => comp.GetType().Name == typeName);
        }
    }

}
