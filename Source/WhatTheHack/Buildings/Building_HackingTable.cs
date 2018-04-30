using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Buildings
{
    class Building_HackingTable : Building_Bed
    {
        private Pawn occupiedByInt = null;
        public Pawn OccupiedBy { get => occupiedByInt; set => occupiedByInt = value; }
    }
}
