using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Garam_RaceAddon
{
    public class RaceAddonThingDef : ThingDef
    {
        public RaceAddonSettings raceAddonSettings = new RaceAddonSettings();

        public WorkTags DisabledWorkTags { private set; get; } = WorkTags.None;
        public List<WorkTypeDef> DisabledWorkTypes = new List<WorkTypeDef>();
        public List<WorkGiverDef> DisabledWorkGivers = new List<WorkGiverDef>();

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            RaceAddon.AllRaceAddonThingDefs.Add(this);

            if (!raceAddonSettings.healthSetting.rottingCorpse)
            {
                race.corpseDef.comps.RemoveAll((CompProperties x) => x is CompProperties_Rottable);
                race.corpseDef.comps.RemoveAll((CompProperties x) => x is CompProperties_SpawnerFilth);
            }

            if (raceAddonSettings.workGiverRestrictionSetting.allAllow)
            {
                foreach (WorkGiverDef def in raceAddonSettings.workGiverRestrictionSetting.allAllow_Exceptions)
                {
                    if (!DisabledWorkGivers.Contains(def))
                    {
                        DisabledWorkGivers.Add(def);
                    }
                }
            }
            else
            {
                foreach (WorkGiverDef def in DefDatabase<WorkGiverDef>.AllDefs)
                {
                    if (!raceAddonSettings.workGiverRestrictionSetting.allAllow_Exceptions.Contains(def))
                    {
                        if (!DisabledWorkGivers.Contains(def))
                        {
                            DisabledWorkGivers.Add(def);
                        }
                    }
                }
            }
            foreach (WorkGiverDef def in raceAddonSettings.workGiverRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.WorkGiverRestrictions.Contains(def))
                {
                    RaceAddon.WorkGiverRestrictions.Add(def);
                }
            }
            if (raceAddonSettings.characterizationSetting.workDisables != null)
            {
                WorkTags workTags = WorkTags.None;
                foreach (WorkTags tag in raceAddonSettings.characterizationSetting.workDisables)
                {
                    workTags |= tag;
                }
                DisabledWorkTags = workTags;


                foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefsListForReading.FindAll(x => (DisabledWorkTags & x.workTags) != 0))
                {
                    if (!DisabledWorkTypes.Contains(def))
                    {
                        DisabledWorkTypes.Add(def);
                    }
                }
            }

            foreach (var obj in raceAddonSettings.apparelRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.ApparelRestrictions.Contains(obj))
                {
                    RaceAddon.ApparelRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.weaponRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.WeaponRestrictions.Contains(obj))
                {
                    RaceAddon.WeaponRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.workGiverRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.WorkGiverRestrictions.Contains(obj))
                {
                    RaceAddon.WorkGiverRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.traitRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.TraitRestrictions.Contains(new Trait(obj.traitDef, obj.degree)))
                {
                    RaceAddon.TraitRestrictions.Add(new Trait(obj.traitDef, obj.degree));
                }
            }
            foreach (var obj in raceAddonSettings.hediffRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.HediffRestrictions.Contains(obj))
                {
                    RaceAddon.HediffRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.thoughtRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.ThoughtRestrictions.Contains(obj))
                {
                    RaceAddon.ThoughtRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.buildingRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.BuildingRestrictions.Contains(obj))
                {
                    RaceAddon.BuildingRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.foodRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.FoodRestrictions.Contains(obj))
                {
                    RaceAddon.FoodRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.plantRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.PlantRestrictions.Contains(obj))
                {
                    RaceAddon.PlantRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.animalRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.AnimalRestrictions.Contains(obj))
                {
                    RaceAddon.AnimalRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.recipeRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.RecipeRestrictions.Contains(obj))
                {
                    RaceAddon.RecipeRestrictions.Add(obj);
                }
            }
            foreach (var obj in raceAddonSettings.researchRestrictionSetting.raceSpecifics)
            {
                if (!RaceAddon.ResearchRestrictions.Contains(obj))
                {
                    RaceAddon.ResearchRestrictions.Add(obj);
                }
            }

            if (comps.Any(x => x.compClass == typeof(RaceAddonComp)))
            {
                RaceAddon.Notify(defName + " has a duplicate RaceAddonComp!");
            }
            else
            {
                comps.Add(new CompProperties(typeof(RaceAddonComp)));
            }
        }
    }
}
