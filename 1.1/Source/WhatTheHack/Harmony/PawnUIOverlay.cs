using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Harmony
{
    [HarmonyPatch(typeof(PawnUIOverlay), "DrawPawnGUIOverlay")]
    class PawnUIOverlay_DrawPawnGUIOverlay
    {
        static void Postfix(PawnUIOverlay __instance)
        {
            Pawn pawn = Traverse.Create(__instance).Field("pawn").GetValue<Pawn>();
            if(pawn.IsHacked() && pawn.Faction == Faction.OfPlayer && pawn.Name != null)
            {
                Vector2 pos = GenMapUI.LabelDrawPosFor(pawn, -0.6f);
                GenMapUI.DrawPawnLabel(pawn, pos, 1f, 9999f, null, GameFont.Tiny, true, true);
            }
        }
    }
}
