using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Stats
{
    class StatPart_RechargeRate : StatPart
    {
        private const float fuelToPowerFactor = 15f;
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if(req.Thing is Building_MechanoidPlatform platform)
            {
                sb.AppendLine("WTH_Explanation_PowerFromNetwork".Translate() + ": " + platform.PowerComp.Props.basePowerConsumption.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset));
            }
            else if(req.Thing is Building_PortableChargingPlatform portablePlatform)
            {
                sb.AppendLine("WTH_Explanation_PowerFromFuel".Translate() + ": " + portablePlatform.refuelableComp.Props.fuelConsumptionRate * fuelToPowerFactor);
            }
            if(Base.powerChargeModifier != 1)
            {
                sb.AppendLine("WTH_Explanation_PowerChargeModifier".Translate() + ": " + (1 - Base.powerChargeModifier).ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
            }
            return sb.ToString();

        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Building_MechanoidPlatform platform)
            {
                val += platform.PowerComp.Props.basePowerConsumption;
            }
            else if (req.Thing is Building_PortableChargingPlatform portablePlatform)
            {
                CompRefuelable refuelableComp = portablePlatform.refuelableComp;
                if (portablePlatform.refuelableComp == null)
                {
                    refuelableComp = portablePlatform.TryGetComp<CompRefuelable>();
                }
                if(refuelableComp != null)
                {
                    val += refuelableComp.Props.fuelConsumptionRate * fuelToPowerFactor;
                }
            }
            if (Base.powerChargeModifier != 1)
            {
                val *= Base.powerChargeModifier;
            }

        }
    }
}
