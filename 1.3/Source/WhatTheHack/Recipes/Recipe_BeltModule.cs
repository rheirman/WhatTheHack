using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace WhatTheHack.Recipes
{
    public class Recipe_BeltModule : Recipe_ModifyMechanoid
    {
        protected override void PostSuccessfulApply(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            base.PostSuccessfulApply(pawn, part, billDoer, ingredients, bill);
            if (pawn.outfits == null)
            {
                if(pawn.story == null)
                {
                    pawn.story = new Pawn_StoryTracker(pawn);

                }
                if (pawn.story.bodyType == null)
                {
                    pawn.story.bodyType = BodyTypeDefOf.Fat; //any body type will do, so why not pick "Fat". 
                }
            }
        }
    }
}
