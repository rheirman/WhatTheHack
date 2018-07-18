using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony
{

    [HarmonyPatch(typeof(Dialog_FormCaravan), "CountToTransferChanged")]
    class Dialog_FormCaravan_CountToTransferChanged
    {
        static void Postfix(Dialog_FormCaravan __instance)
        {
            int mechanoidCount = 0;
            float fuelAmount = 0f;
            float fuelConsumption = 0f;
            int numPlatforms = 0;
            float daysOfFuel = 0;
            StringBuilder daysOfFuelReason = new StringBuilder();

            foreach (TransferableOneWay tow in __instance.transferables)
            {
                Log.Message(tow.ThingDef.defName);
                Log.Message("CountToTransfer: " + tow.CountToTransfer);
                if(tow.ThingDef.race != null && tow.ThingDef.race.IsMechanoid)
                {
                    mechanoidCount += tow.CountToTransfer;
                }
                if(tow.ThingDef == ThingDefOf.Chemfuel)
                {
                    fuelAmount += tow.CountToTransfer;
                }
                if(tow.ThingDef == ThingDefOf.MinifiedThing)
                {
                    if (tow.things[0].GetInnerIfMinified().def == WTH_DefOf.WTH_PortableChargingPlatform)
                    {
                        numPlatforms += tow.CountToTransfer;
                    }

                }
            }
            if (mechanoidCount == 0)
            {
                return;
            }
            daysOfFuelReason.AppendLine("WTH_Explanation_NumMechs".Translate() + ": " + mechanoidCount);
            daysOfFuelReason.AppendLine("WTH_Explanation_NumPlatforms".Translate() + ": " + numPlatforms);
            if (numPlatforms >= mechanoidCount)
            {
                fuelConsumption = mechanoidCount * WTH_DefOf.WTH_PortableChargingPlatform.GetCompProperties<CompProperties_Refuelable>().fuelConsumptionRate;
                daysOfFuel = fuelAmount / fuelConsumption;
                daysOfFuelReason.AppendLine("WTH_Explanation_FuelConsumption".Translate() + ": " + fuelConsumption.ToString("0.#"));
                daysOfFuelReason.AppendLine("WTH_Explanation_TotalFuel".Translate() + ": " + fuelAmount.ToString("0.#"));
                daysOfFuelReason.AppendLine("WTH_Explanation_DaysOfFuel".Translate() + ": " + fuelAmount.ToString("0.#") + " /( " + mechanoidCount + " * " + fuelConsumption + ") = " + daysOfFuel.ToString("0.#"));
            }
            else
            {
                daysOfFuelReason.AppendLine("WTH_Explanation_NotEnoughPlatforms".Translate());
            }
            Base.Instance.daysOfFuel = daysOfFuel;
            Base.Instance.daysOfFuelReason = daysOfFuelReason.ToString();

        }
    }
}
