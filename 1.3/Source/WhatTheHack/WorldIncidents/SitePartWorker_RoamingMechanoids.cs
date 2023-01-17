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
        public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
        {
            return string.Concat(base.GetPostProcessedThreatLabel(site, sitePart),
                                     " (",
                                     GenLabel.BestKindLabel(sitePart.parms.animalKind, Gender.None, true),
                                     ")"
                                 );
        }

        public override void PostMapGenerate(Map map)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.Misc, target: map);
            incidentParms.forced = true;
        }
        /*
        public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
        {
            return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, true));
        }
        */
    }
}
