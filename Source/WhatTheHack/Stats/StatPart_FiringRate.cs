using System.Text;
using RimWorld;
using Verse;

namespace WhatTheHack.Stats;

internal class StatPart_FiringRate : StatPart
{
    public override string ExplanationPart(StatRequest req)
    {
        var sb = new StringBuilder();
        if (req.Thing is not Pawn pawn)
        {
            return sb.ToString();
        }

        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.firingRateOffset != 0)
            {
                sb.AppendLine(
                    $"{h.def.label}: {modExt.firingRateOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset)}");
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

        float offset = 0;
        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.firingRateOffset != 0)
            {
                offset += val * modExt.firingRateOffset;
            }
        }

        val += offset;
    }
}