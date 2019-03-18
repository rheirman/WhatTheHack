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
                if (pawn.health != null)
                {
                    foreach (Hediff h in pawn.health.hediffSet.hediffs)
                    {
                        if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                        {
                            sb.AppendLine(h.def.label + ": " + modExt.batteryCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
                        }
                    }
                }
            }
            return sb.ToString();
        }
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                if (pawn.health == null)
                {
                    return;
                }
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                    {
                        val += modExt.batteryCapacityOffset;
                    }
                }
            }
        }
    }
}
