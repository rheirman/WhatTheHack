using System.Text;
using RimWorld;
using Verse;

namespace WhatTheHack.Stats;

internal class StatPart_BatteryCapacity : StatPart
{
    public override string ExplanationPart(StatRequest req)
    {
        var sb = new StringBuilder();
        if (req.Thing is not Pawn pawn)
        {
            return sb.ToString();
        }

        sb.AppendLine("WTH_Explanation_BodySizeContr".Translate() + ": +" + (pawn.BodySize * 100));
        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.batteryCapacityOffset != 0)
            {
                sb.AppendLine(
                    $"{h.def.label}: {modExt.batteryCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset)}");
            }
        }

        return sb.ToString();
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing is not Pawn pawn)
        {
            return;
        }

        val += pawn.BodySize * 100;
        float offset = 0;
        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.batteryCapacityOffset != 0)
            {
                offset += val * modExt.batteryCapacityOffset;
            }
        }

        val += offset;
    }
}