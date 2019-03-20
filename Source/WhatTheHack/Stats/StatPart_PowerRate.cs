using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Stats
{
    class StatPart_PowerRate : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if (req.Thing is Pawn pawn)
            {
                sb.AppendLine("Body size contribution: +" + pawn.BodySize * 150);
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                    {
                        sb.AppendLine(h.def.label + ": " +  modExt.powerRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
                    }
                }
                
            }
            sb.AppendLine("power fall modifier (set in options): " + (1f - Base.powerFallModifier.Value).ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
            return sb.ToString();
        }
        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                val += pawn.BodySize * 150f;
                float offset = 0;
                foreach(Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if(h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt)
                    {
                        offset += val * modExt.powerRateOffset;
                    }
                }
                val += offset;
                val *= Base.powerFallModifier;
            }
        }
    }
}
