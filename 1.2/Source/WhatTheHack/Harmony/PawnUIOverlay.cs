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
        static void Postfix(Pawn ___pawn)
        {
            if(___pawn.IsHacked() && ___pawn.Faction == Faction.OfPlayer && ___pawn.Name != null)
            {
                Vector2 pos = GenMapUI.LabelDrawPosFor(___pawn, -0.6f);
                GenMapUI.DrawPawnLabel(___pawn, pos, 1f, 9999f, null, GameFont.Tiny, true, true);
            }
        }
    }
}
