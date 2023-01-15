using HarmonyLib;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(Thing), "DrawExtraSelectionOverlays")]
internal static class Thing_DrawExtraSelectionOverlays
{
    private static void Postfix(Thing __instance)
    {
        if (__instance is Pawn currentPawh)
        {
            if (currentPawh.RemoteControlLink() != null)
            {
                if (currentPawh.IsHacked())
                {
                    GenDraw.DrawRadiusRing(currentPawh.RemoteControlLink().Position,
                        Utilities.GetRemoteControlRadius(currentPawh.RemoteControlLink()));
                }
                else
                {
                    GenDraw.DrawRadiusRing(currentPawh.Position, Utilities.GetRemoteControlRadius(currentPawh));
                }

                GenDraw.DrawLineBetween(currentPawh.Position.ToVector3Shifted(),
                    currentPawh.RemoteControlLink().Position.ToVector3Shifted());
            }

            if (currentPawh.ControllingAI() is { } controller)
            {
                GenDraw.DrawLineBetween(currentPawh.Position.ToVector3Shifted(),
                    currentPawh.ControllingAI().Position.ToVector3Shifted(),
                    controller.controlledMechs.Contains(currentPawh) ? SimpleColor.Blue : SimpleColor.Red);
            }
        }

        if (__instance is not Building_RogueAI rogueAI)
        {
            return;
        }

        foreach (var pawn in rogueAI.controlledMechs)
        {
            GenDraw.DrawLineBetween(rogueAI.Position.ToVector3Shifted(), pawn.Position.ToVector3Shifted(),
                SimpleColor.Blue);
        }

        foreach (var pawn in rogueAI.hackedMechs)
        {
            GenDraw.DrawLineBetween(rogueAI.Position.ToVector3Shifted(), pawn.Position.ToVector3Shifted(),
                SimpleColor.Red);
        }

        foreach (var t in rogueAI.controlledTurrets)
        {
            GenDraw.DrawLineBetween(rogueAI.Position.ToVector3Shifted(), t.Position.ToVector3Shifted(),
                SimpleColor.Green);
        }
    }
}