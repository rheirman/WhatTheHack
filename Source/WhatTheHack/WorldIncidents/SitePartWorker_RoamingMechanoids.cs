using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.WorldIncidents
{
    public class SitePartWorker_RoamingMechanoids: SitePartWorker
    {
        public override string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
        {
            return string.Concat(base.GetPostProcessedThreatLabel(site, siteCoreOrPart),
                                     " (",
                                     GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, true),
                                     ")"
                                 );
        }

        public override void PostMapGenerate(Map map)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.Misc, target: map);
            incidentParms.forced = true;
            //this part is forced to bypass CanFireNowSub, to solve issue with scenario-added incident.
            //QueuedIncident queuedIncident = new QueuedIncident(firingInc: new FiringIncident(def: DefDatabase<IncidentDef>.GetNamed(defName: "MFI_HerdMigration_Ambush"), source: null, parms: incidentParms), fireTick: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay / 2, max: GenDate.TicksPerDay));
            //Find.Storyteller.incidentQueue.Add(qi: queuedIncident);
        }

        public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
        {
            return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, true));
        }

    }
}
