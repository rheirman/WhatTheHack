using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.WorldIncidents
{
    public class IncidentWorker_RoamingMechanoids : IncidentWorker
    {
        private const int MinDistance = 2;
        private const int MaxDistance = 15;

        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 25, max: 50);


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && Find.AnyPlayerHomeMap != null
                                                    && TryFindTile(tile: out int num);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {

            if (!TryFindTile(tile: out int tile))
                return false;

            Site site = SiteMaker.MakeSite(WTH_DefOf.WTH_RoamingMechanoidsCore,
                                           WTH_DefOf.WTH_RoamingMechanoidsPart,
                                           tile, Faction.OfMechanoids, ifHostileThenMustRemainHostile: true);

            if (site == null)
                return false;

            int randomInRange = TimeoutDaysRange.RandomInRange;

            site.Tile = tile;
            site.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
            site.SetFaction(Faction.OfMechanoids);

            site.customLabel = "TODO";
            Find.WorldObjects.Add(o: site);

            Find.LetterStack.ReceiveLetter(label: this.def.letterLabel, text: "TODO", textLetterDef: this.def.letterDef, lookTargets: site);
            return true;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, true, false);
        }
    }
}
