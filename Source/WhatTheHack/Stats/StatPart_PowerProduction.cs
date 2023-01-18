using System.Text;
using RimWorld;
using Verse;

namespace WhatTheHack.Stats;

internal class StatPart_PowerProduction : StatPart
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
                modExt.powerProduction != 0)
            {
                sb.AppendLine(
                    $"{h.def.label}: {modExt.powerProduction.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset)}");
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

        foreach (var h in pawn.health.hediffSet.hediffs)
        {
            if (h.def.GetModExtension<DefModextension_Hediff>() is { } modExt &&
                modExt.powerProduction != 0)
            {
                val += modExt.powerProduction;
            }
        }
    }
}