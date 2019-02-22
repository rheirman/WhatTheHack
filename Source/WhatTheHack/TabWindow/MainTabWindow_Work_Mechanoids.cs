using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.TabWindow
{
    class MainTabWindow_Work_Mechanoids : MainTabWindow_Work
    {
        protected override IEnumerable<Pawn> Pawns
        {
            get
            {
                return from p in Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer)
                       where p.IsHacked()
                       select p;
            }
        }
        
        protected override PawnTableDef PawnTableDef
        {
            get
            {
                return WTH_DefOf.WTH_Work_Mechanoids;
            }
        }
    }
}
