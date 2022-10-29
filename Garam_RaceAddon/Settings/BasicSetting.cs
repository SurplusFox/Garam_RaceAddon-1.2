using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class BasicSetting
    {
        public float maleChance = 0.5f;
        public int raceSexuality = 0;
        public BodyPartDef raceHeadDef = BodyPartDefOf.Head;
        public bool humanlikeMeat = true;
        public ThingDef recipeImportTarget = null;
        public int maxDamageForSocialfight = 6;
        public string shortDescriptionForRaceStory = null;
        public List<BackstoryCategoryFilter> newbornBackstoryCategoryFilters;
    }
}