using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Verse;
using WhatTheHack.Buildings;
using WhatTheHack.Storage;

namespace WhatTheHack.Harmony
{
    
    [HarmonyPatch(typeof(Dialog_FormCaravan), "TryFormAndSendCaravan")]
    class Dialog_FormCaravan_TryFormAndSendCaravan
    {
        static void Postfix(Dialog_FormCaravan __instance, bool __result)
        {
            List<Pawn> pawns = TransferableUtility.GetPawnsFromTransferables(__instance.transferables);
            Predicate<Thing> isChargingPlatform = (Thing t) => t != null && t.GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform;
            List<TransferableOneWay> chargingPlatformTows = __instance.transferables.FindAll((TransferableOneWay x) => x.CountToTransfer > 0 && x.HasAnyThing && isChargingPlatform(x.AnyThing));
            List<Building_PortableChargingPlatform> platforms = new List<Building_PortableChargingPlatform>();

            foreach (TransferableOneWay tow in chargingPlatformTows)
            {
                foreach (Thing t in tow.things)
                {
                    Building_PortableChargingPlatform platform = (Building_PortableChargingPlatform)t.GetInnerIfMinified();
                    platform.CaravanPawn = null;
                    //TODO: prevent stacking here so each pawn can have an associated platform
                    //platforms.Add(platform);
                }
            }

            //Find and assign platform for each pawn. 
            foreach (Pawn pawn in pawns)
            {
                if (pawn.IsHacked())
                {
                    bool foundPlatform = false;
                    for(int j = 0; j < chargingPlatformTows.Count && !foundPlatform; j++)
                    {
                        for(int i = 0; i < chargingPlatformTows[j].things.Count && !foundPlatform; i++)
                        {
                            Building_PortableChargingPlatform platform = (Building_PortableChargingPlatform)chargingPlatformTows[j].things[i].GetInnerIfMinified();
                            if (platform != null && platform.CaravanPawn == null)
                            {
                                platform.CaravanPawn = pawn;
                                ExtendedPawnData pawnData = Base.Instance.GetExtendedDataStorage().GetExtendedDataFor(pawn);
                                pawnData.caravanPlatform = platform;
                                foundPlatform = true;
                            }
                        }

                    }
                }
            }
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
                if (tow.ThingDef.race != null && tow.ThingDef.race.IsMechanoid)
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
