using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;
using WhatTheHack.Storage;

namespace WhatTheHack.Stats
{
    class StatPart_PowerRate : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            StringBuilder sb = new StringBuilder();
            if (req.Thing is Pawn pawn)
            {
                sb.AppendLine("WTH_Explanation_BodySizeContr".Translate() + ": +" + pawn.BodySize * 150);
                foreach (Hediff h in pawn.health.hediffSet.hediffs)
                {
                    if (h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.powerRateOffset != 0)
                    {
                        sb.AppendLine(h.def.label + ": " +  modExt.powerRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
                    }
                }
                if(Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
                {
                    CompMountable turretMount = store.GetExtendedDataFor(pawn).turretMount;
                    if(turretMount != null && turretMount.parent.GetComp<CompPowerTrader>() is CompPowerTrader powerComp && powerComp.PowerOn)
                    {
                        sb.AppendLine(turretMount.parent.Label + ":" + powerComp.Props.basePowerConsumption.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset));
                    }
                }
            }
            
            if(Base.powerFallModifier.Value != 1f)
            { 
                sb.AppendLine("WTH_Explanation_PowerFallModifier".Translate() + ": " + (1f - Base.powerFallModifier.Value).ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset));
            }
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
                    if(h.def.GetModExtension<DefModextension_Hediff>() is DefModextension_Hediff modExt && modExt.powerRateOffset != 0)
                    {
                        offset += val * modExt.powerRateOffset;
                    }
                }
                if (Base.Instance.GetExtendedDataStorage() is ExtendedDataStorage store)
                {
                    CompMountable turretMount = store.GetExtendedDataFor(pawn).turretMount;
                    if (turretMount != null && turretMount.parent.GetComp<CompPowerTrader>() is CompPowerTrader powerComp && powerComp.PowerOn)
                    {
                        val += powerComp.Props.basePowerConsumption;
                    }
                }
                val += offset;
                val *= Base.powerFallModifier;
            }
        }
    }
}
