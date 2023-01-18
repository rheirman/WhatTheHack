using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace WhatTheHack.TabWindow;

internal class MainTabWindow_Work_Mechanoids : MainTabWindow_Work
{
    public override IEnumerable<Pawn> Pawns =>
        from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
        where p.IsHacked()
        select p;

    public override PawnTableDef PawnTableDef => WTH_DefOf.WTH_Work_Mechanoids;
}