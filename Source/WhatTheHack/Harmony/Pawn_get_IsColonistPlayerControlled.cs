using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Pawn), "get_IsColonistPlayerControlled")]
public class Pawn_get_IsColonistPlayerControlled
{
    public static bool Prefix(Pawn __instance, ref bool __result)
    {
        if (!__instance.HasReplacedAI() && !IsControlled(__instance))
        {
            return true;
        }

        if (__instance.Faction != Faction.OfPlayer || !__instance.IsHacked() || __instance.Dead)
        {
            return true;
        }

        __result = true;
        return false;
    }

    private static bool IsControlled(Pawn pawn)
    {
        if (!pawn.IsMechanoid())
        {
            return false;
        }

        if (pawn.RemoteControlLink() == null || pawn.RemoteControlLink().Drafted)
        {
            return pawn.ControllingAI() != null;
        }

        float radius = Utilities.GetRemoteControlRadius(pawn.RemoteControlLink());
        return pawn.Position.DistanceToSquared(pawn.RemoteControlLink().Position) <= radius * radius;
    }
}