using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace WhatTheHack.Buildings
{
    class Building_MechanoidBeacon : Building
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo c in base.GetGizmos())
            {
                yield return c;
            }
            foreach (Gizmo c2 in StartupGizmos())
            {
                yield return c2;
            }
        }
        public IEnumerable<Gizmo> StartupGizmos()
        {
            yield return new Command_Action
            {
                action = delegate
                {
                    string text = "HibernateWarning";
                    /*
                    if (building.Map.info.parent.GetComponent<EscapeShipComp>() == null)
                    {
                        text += "Standalone";
                    }
                    */
                    if (!Find.Storyteller.difficulty.allowBigThreats)
                    {
                        text += "Pacifist";
                    }
                    DiaNode diaNode = new DiaNode(text.Translate());
                    DiaOption diaOption = new DiaOption("Confirm".Translate());
                    diaOption.action = delegate
                    {
                        StartupHibernatingParts();
                    };
                    diaOption.resolveTree = true;
                    diaNode.options.Add(diaOption);
                    DiaOption diaOption2 = new DiaOption("GoBack".Translate());
                    diaOption2.resolveTree = true;
                    diaNode.options.Add(diaOption2);
                    Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, false, null));
                },
                defaultLabel = "CommandShipStartup".Translate(),
                defaultDesc = "CommandShipStartupDesc".Translate(),
                hotKey = KeyBindingDefOf.Misc1,
                icon = ContentFinder<Texture2D>.Get("UI/Commands/DesirePower", true)
            };
        }
        public void StartupHibernatingParts()
        {
            CompHibernatable compHibernatable = this.TryGetComp<CompHibernatable>();
            if (compHibernatable != null && compHibernatable.State == HibernatableStateDefOf.Hibernating)
            {
                compHibernatable.Startup();
            }
        }
    }
}
