using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(MassUtility), "Capacity")]
internal class MassUtility_Capacity
{
    private static bool Prefix(Pawn p, ref StringBuilder explanation, ref float __result)
    {
        if (p.health == null || !p.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
        {
            return true;
        }

        __result = 0f;
        if (explanation == null)
        {
            return false;
        }

        explanation.AppendLine();
        explanation.Append($"  - {p.LabelShortCap}: {__result.ToStringMassOffset()}");

        return false;
    }

    private static void Postfix(Pawn p, ref StringBuilder explanation, ref float __result)
    {
        var bonus = 0f;
        var hasMountedTurret = false;

        if (p.health != null && p.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
        {
            hasMountedTurret = true;
            bonus += MassUtility.GearAndInventoryMass(p);
            if (p.inventory.GetDirectlyHeldThings().FirstOrDefault(t =>
                    t is ThingWithComps twc && twc.TryGetComp<CompMountable>() is { }) is { } mountedTurret)
            {
                bonus += mountedTurret.def.BaseMass;
            }
        }

        if (p.def.HasModExtension<DefModExtension_PawnMassCapacity>() && !hasMountedTurret)
        {
            bonus += p.def.GetModExtension<DefModExtension_PawnMassCapacity>().bonusMassCapacity;
        }

        __result += bonus;
        float offset = 0;
        if (explanation == null)
        {
            explanation = new StringBuilder();
        }

        if (explanation != null && bonus > 0)
        {
            explanation.AppendLine(
                $"- {p.def.label}: {bonus.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset)}");
        }

        if (p.health is { hediffSet: { } })
        {
            foreach (var h in p.health.hediffSet.hediffs)
            {
                if (h.def.GetModExtension<DefModextension_Hediff>() is not { } modExt ||
                    modExt.carryingCapacityOffset == 0)
                {
                    continue;
                }

                explanation.AppendLine();
                explanation.AppendLine(
                    $"- {h.def.label}: {modExt.carryingCapacityOffset.ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Offset)}");
                offset += modExt.carryingCapacityOffset;
            }
        }

        __result += offset * __result;
    }
}