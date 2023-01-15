using System.Text;
using RimWorld;
using Verse;

namespace WhatTheHack.Stats;

internal class StatPart_PowerRate : StatPart
{
    public override string ExplanationPart(StatRequest req)
    {
        var sb = new StringBuilder();
        if (req.Thing is Pawn pawn)
        {
            sb.AppendLine("WTH_Explanation_BodySizeContr".Translate() + ": +" + (pawn.BodySize * 150));
            foreach (var h in pawn.health.hediffSet.hediffs)
            {
                if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                    modExt.powerRateOffset != 0)
                {
                    sb.AppendLine(
                        $"{h.def.label}: {modExt.powerRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset)}");
                }
            }

            if (Base.Instance.GetExtendedDataStorage() is { } store)
            {
                var turretMount = store.GetExtendedDataFor(pawn).turretMount;
                if (turretMount != null && turretMount.parent.GetComp<CompPowerTrader>() is { PowerOn: true } powerComp)
                {
                    sb.AppendLine(
                        $"{turretMount.parent.Label}:{powerComp.Props.basePowerConsumption.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset)}");
                }
            }
        }

        if (Base.powerFallModifier.Value != 1f)
        {
            sb.AppendLine("WTH_Explanation_PowerFallModifier".Translate() + ": " +
                          (1f - Base.powerFallModifier.Value).ToStringByStyle(ToStringStyle.PercentZero,
                              ToStringNumberSense.Offset));
        }

        return sb.ToString();
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing is not Pawn pawn)
        {
            return;
        }

        val += pawn.BodySize * 150f;
        float offset = 0;
        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.powerRateOffset != 0)
            {
                offset += val * modExt.powerRateOffset;
            }
        }

        if (Base.Instance.GetExtendedDataStorage() is { } store)
        {
            var turretMount = store.GetExtendedDataFor(pawn).turretMount;
            if (turretMount != null && turretMount.parent.GetComp<CompPowerTrader>() is { PowerOn: true } powerComp)
            {
                val += powerComp.Props.basePowerConsumption;
            }
        }

        val += offset;
        val *= Base.powerFallModifier;
    }
}