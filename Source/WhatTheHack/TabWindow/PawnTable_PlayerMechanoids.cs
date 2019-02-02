using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.TabWindow
{
    public class PawnTable_PlayerMechanoids : PawnTable
    {
        public PawnTable_PlayerMechanoids(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight) : base(def, pawnsGetter, uiWidth, uiHeight)
        {
        }

        protected override IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
        {
            return PlayerPawnsDisplayOrderUtility.InOrder(input);
        }
    }
}
