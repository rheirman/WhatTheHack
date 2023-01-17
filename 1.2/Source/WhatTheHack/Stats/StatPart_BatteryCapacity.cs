using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Stats
{
    class StatPart_BatteryCapacity : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if (req.Thing is Pawn pawn)
            {
                sb.AppendLine("WTH_Explanation_BodySizeContr".Translate() + ": +" + pawn.BodySize * 100);
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.batteryCapacityOffset != 0)
                    {
                        sb.AppendLine(h.def.label + ": " + modExt.batteryCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
                    }
                }              
            }
            return sb.ToString();
        }
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                val += pawn.BodySize * 100;
                float offset = 0;
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.batteryCapacityOffset != 0)
                    {
                        offset += val * modExt.batteryCapacityOffset;
                    }
                }
                val += offset;
            }
        }
    }
}
