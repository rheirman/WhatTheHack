using HarmonyLib;
using RimWorld;
using Verse;

namespace WhatTheHack.Harmony;

[HarmonyPatch(typeof(PawnUIOverlay), "DrawPawnGUIOverlay")]
internal class PawnUIOverlay_DrawPawnGUIOverlay
{
    private static void Postfix(Pawn ___pawn)
    {
        if (!___pawn.IsHacked() || ___pawn.Faction != Faction.OfPlayer || ___pawn.Name == null)
        {
            return;
        }

        var pos = GenMapUI.LabelDrawPosFor(___pawn, -0.6f);
        GenMapUI.DrawPawnLabel(___pawn, pos);
    }
}