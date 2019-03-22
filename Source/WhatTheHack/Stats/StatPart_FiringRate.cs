using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Stats
{
    class StatPart_FiringRate :  StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if (req.Thing is Pawn pawn)
            {
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.firingRateOffset != 0)
                    {
                        sb.AppendLine(h.def.label + ": " + modExt.firingRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
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
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.firingRateOffset != 0)
                    {
                        offset += val * modExt.firingRateOffset;
                    }
                }
                val += offset;
            }
        }
    }
}
