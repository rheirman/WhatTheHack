using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Stats
{
    class StatPart_PartConsumptionRate : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if(req.Thing is Building_MechanoidPlatform platform)
            {
                sb.AppendLine("WTH_Explanation_ConsumptionRate".Translate() + ": " + platform.refuelableComp.Props.fuelConsumptionRate.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Absolute));
                if (Base.repairConsumptionModifier != 1f)
                {
                    sb.AppendLine("WTH_Explanation_RepairConsumptionModifier".Translate() + ": " + (1f - Base.powerFallModifier.Value).ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
                }
            }

            return sb.ToString();
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Building_MechanoidPlatform platform)
            {
                val += platform.refuelableComp.Props.fuelConsumptionRate;
                val *= Base.repairConsumptionModifier;
            }
        }
    }
}
