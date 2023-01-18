using RimWorld;
using RimWorld.Planet;
using Verse;

namespace WhatTheHack.WorldIncidents;

public class SitePartWorker_RoamingMechanoids : SitePartWorker
{
    public override string GetPostProcessedThreatLabel(Site site, SitePart sitePart)
    {
        return
            $"{base.GetPostProcessedThreatLabel(site, sitePart)} ({GenLabel.BestKindLabel(sitePart.parms.animalKind, Gender.None, true)})";
    }

    public override void PostMapGenerate(Map map)
    {
        var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
        incidentParms.forced = true;
    }
    /*
    public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
    {
        return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, true));
    }
    */
}