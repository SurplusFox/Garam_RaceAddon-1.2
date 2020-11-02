using RimWorld;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Verse;

namespace Garam_RaceAddon
{
    public class RaceAddonSettings
    {
        public List<AgeSetting> ageSettings = new List<AgeSetting>();
        public GraphicSetting graphicSetting = new GraphicSetting();
        public BasicSetting basicSetting = new BasicSetting();
        public CharacterizationSetting characterizationSetting = new CharacterizationSetting();
        public HealthSetting healthSetting = new HealthSetting();
        public RelationSetting relationSetting = new RelationSetting();

        public RestrictionSetting<ThingDef> apparelRestrictionSetting = new RestrictionSetting<ThingDef>();
        public RestrictionSetting<ThingDef> weaponRestrictionSetting = new RestrictionSetting<ThingDef>();
        public RestrictionSetting<WorkGiverDef> workGiverRestrictionSetting = new RestrictionSetting<WorkGiverDef>();
        public RestrictionSetting<TraitInfo> traitRestrictionSetting = new RestrictionSetting<TraitInfo>();
        public RestrictionSetting<HediffDef> hediffRestrictionSetting = new RestrictionSetting<HediffDef>();
        public RestrictionSetting<ThoughtDef> thoughtRestrictionSetting = new RestrictionSetting<ThoughtDef>();
        public RestrictionSetting<ThingDef> buildingRestrictionSetting = new RestrictionSetting<ThingDef>();
        public RestrictionSetting<ThingDef> foodRestrictionSetting = new RestrictionSetting<ThingDef>();
        public RestrictionSetting<ThingDef> plantRestrictionSetting = new RestrictionSetting<ThingDef>();
        public RestrictionSetting<ThingDef> animalRestrictionSetting = new RestrictionSetting<ThingDef>();
        public RestrictionSetting<RecipeDef> recipeRestrictionSetting = new RestrictionSetting<RecipeDef>();
        public RestrictionSetting<ResearchProjectDef> researchRestrictionSetting = new RestrictionSetting<ResearchProjectDef>();
    }
}