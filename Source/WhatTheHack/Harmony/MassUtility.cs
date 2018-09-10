using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Comps;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(MassUtility), "Capacity")]
    class MassUtility_Capacity
    {

        static bool Prefix(Pawn p, ref StringBuilder explanation, ref float __result)
        {
            if(p.health != null && p.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret))
            {
                __result = 0f;
                if (explanation != null)
                {
                    explanation.AppendLine();
                    explanation.Append("  - " + p.LabelShortCap + ": " + __result.ToStringMassOffset());
                }
                return false;
            }
            return true;
        }
        static void Postfix(Pawn p, ref StringBuilder explanation, ref float __result)
        {
            float bonus = 0f;
            bool hasMountedTurret = false;

            if (p.health != null && p.health.hediffSet.HasHediff(WTH_DefOf.WTH_MountedTurret)){
                hasMountedTurret = true;
                bonus += MassUtility.GearAndInventoryMass(p);
                if(p.inventory.GetDirectlyHeldThings().FirstOrDefault((Thing t) => t is ThingWithComps twc && twc.TryGetComp<CompMountable>() is CompMountable comp) is Thing mountedTurret)
                {
                    bonus += mountedTurret.def.BaseMass;
                }
            }
            if(p.def.HasModExtension<DefModExtension_PawnMassCapacity>() && !hasMountedTurret)
            {
                bonus += p.def.GetModExtension<DefModExtension_PawnMassCapacity>().bonusMassCapacity;
            }
            __result += bonus;
            if (explanation != null && bonus > 0)
            {
                explanation.AppendLine();
            }
        }
    }
}
