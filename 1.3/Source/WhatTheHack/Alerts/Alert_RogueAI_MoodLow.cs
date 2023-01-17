using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using WhatTheHack.Buildings;

namespace WhatTheHack.Alerts
{
    class Alert_RogueAI_MoodLow : Alert
    {
        private IEnumerable<Building_RogueAI> RogueAIs
        {
            get
            {
                foreach (Map map in Find.Maps)
                {
                    foreach (Building_RogueAI rAI in map.listerBuildings.AllBuildingsColonistOfDef(WTH_DefOf.WTH_RogueAI).Cast<Building_RogueAI>())
                    {
                        if (rAI.CurMoodCategory == Building_RogueAI.Mood.Annoyed && !rAI.goingRogue)
                        {
                            yield return rAI;
                        }
                    }
                }
            }
        }

        public Alert_RogueAI_MoodLow()
        {
            this.defaultLabel = "WTH_Alert_RogueAI_MoodLow_Label".Translate();
            this.defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            return string.Format("WTH_Alert_RogueAI_MoodLow_Description".Translate(), stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritIs(RogueAIs.FirstOrDefault<Building_RogueAI>());
        }
    }
}
