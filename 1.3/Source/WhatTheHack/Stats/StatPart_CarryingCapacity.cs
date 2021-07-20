using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Stats
{
    class StatPart_CarryingCapacity : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if (req.Thing is Pawn pawn)
            {
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.carryingCapacityOffset != 0)
                    {
                        sb.AppendLine(h.def.label + ": " + modExt.carryingCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
                    }
                }
            }
            return sb.ToString();
        }
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                float offset = 0;
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.carryingCapacityOffset != 0)
                    {
                        offset += val * modExt.carryingCapacityOffset;
                    }
                }
                val += offset;
            }
        }
    }
}
