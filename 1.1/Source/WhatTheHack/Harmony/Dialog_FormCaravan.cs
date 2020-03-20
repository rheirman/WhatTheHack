using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Comps;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(Dialog_FormCaravan), "TryReformCaravan")]
    class Dialog_FormCaravan_TryReformCaravan
    {
        //Make sure mounted turrets are brought along with the caravan. 
        static void Prefix(Dialog_FormCaravan __instance)
        {
            List<TransferableOneWay> mountedTurretTows = __instance.transferables.Where((TransferableOneWay tow) => tow.AnyThing is ThingWithComps twc && twc.TryGetComp<CompMountable>() is CompMountable comp && comp.mountedTo != null).ToList();

            foreach (TransferableOneWay tow in mountedTurretTows)
            {
                CompMountable comp = tow.AnyThing.TryGetComp<CompMountable>();
                comp.ToInventory();
                Traverse.Create(tow).Property("CountToTransfer").SetValue(1);
            }
        }
    }


    [HarmonyPatch(typeof(Dialog_FormCaravan), "TryFormAndSendCaravan")]
    class Dialog_FormCaravan_TryFormAndSendCaravan
    {
        static void Postfix(Dialog_FormCaravan __instance, bool __result)
        {
            Utilities.LinkPortablePlatforms(__instance.transferables);
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan), "DoBottomButtons")]
    class Dialog_FormCaravan_DoBottomButtons
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instructionsList = new List<CodeInstruction>(instructions);
            for (var i = 0; i < instructionsList.Count; i++)
            {
                CodeInstruction instruction = instructionsList[i];
                if (i < instructionsList.Count - 2 && instructionsList[i + 1].opcode == OpCodes.Call && instructionsList[i + 1].operand == AccessTools.Method(typeof(Dialog_FormCaravan), "get_DaysWorthOfFood"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 4);
                    yield return new CodeInstruction(OpCodes.Call, typeof(Dialog_FormCaravan_DoBottomButtons).GetMethod("AddWarnings"));
                }
                yield return instruction;
            }
        }

        public static void AddWarnings(Dialog_FormCaravan instance, ref List<string> warnings){
            int numMechanoids = 0;
            int numPlatforms = 0;
            foreach (TransferableOneWay tow in instance.transferables)
            {
                if (tow.ThingDef.race != null && tow.ThingDef.race.IsMechanoid && tow.AnyThing is Pawn pawn && pawn.IsHacked() && !pawn.health.hediffSet.HasHediff(WTH_DefOf.WTH_VanometricModule))
                {
                    numMechanoids += tow.CountToTransfer;
                }
                if (tow.ThingDef == ThingDefOf.MinifiedThing)
                {
                    if (tow.things[0].GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform)
                    {
                        numPlatforms += tow.CountToTransfer;
                    }
                }
                if (tow.ThingDef == WTH_DefOf.WTH_PortableChargingPlatform)
                {
                    numPlatforms += tow.CountToTransfer;
                }
            }
            if(numMechanoids == 0)
            {
                return;
            }
            if(numPlatforms < numMechanoids)
            {
                warnings.Add("WTH_Warning_NotEnoughPlatforms".Translate());
            }
            else if (Base.Instance.daysOfFuel < Traverse.Create(instance).Field("MaxDaysWorthOfFoodToShowWarningDialog").GetValue<float>())
            {
                warnings.Add("WTH_Warning_DaysOfFuel".Translate(new Object[]{Base.Instance.daysOfFuel.ToString("0.#") }));
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_FormCaravan), "CountToTransferChanged")]
    class Dialog_FormCaravan_CountToTransferChanged
    {
        static void Postfix(Dialog_FormCaravan __instance)
        {
            Utilities.CalcDaysOfFuel(__instance.transferables);
        }

       
    }

}
