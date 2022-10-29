using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public static class RaceAddon
    {
        public static void OldVersionBugFixes(Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef && pawn.GetComp<RaceAddonComp>() is var racomp && racomp.pawnGeneratedVersion != Mod.ModVersion)
            {
                //Notify("The version in which [" + pawn.Name.ToStringFull + "] was created and the version in the mod do not match. Attempts to fix known bugs.");
                Notify("({0}) The generated version({1}) and the current version({2}) do not match.".Formatted(pawn.Name.ToStringFull, racomp.pawnGeneratedVersion, Mod.ModVersion));
                //4.3.0-201208
                if (racomp.drawSizeDeviation <= 0f)
                {
                    racomp.drawSizeDeviation = 1.0f;
                }

                //Wrap-up
                racomp.pawnGeneratedVersion = Mod.ModVersion;
            }
        }

        public static Dictionary<PawnKindDef, List<Pair<PawnKindDef, float>>> PawnKindDefReplaceSettings { get; set; } = new Dictionary<PawnKindDef, List<Pair<PawnKindDef, float>>>();

        public static PawnKindDef GetReplacedPawnKindDef(PawnKindDef key)
        {
            if (PawnKindDefReplaceSettings.TryGetValue(key, out var list) && list.Count > 0)
            {
                return list.RandomElementByWeight(x => x.Second).First;
            }
            return key;
        }

        public static bool IsWildMan(CustomPawnKindDef spkd)
        {
            if (PawnKindDefReplaceSettings.TryGetValue(PawnKindDefOf.WildMan, out var pair) && pair.Any(x => spkd.pawnKindDefReplaceSettings.Any(y => y.replacedPawnKindDef == x.First)))
            {
                return true;
            }
            return false;
        }

        public static bool IsSlave(CustomPawnKindDef spkd)
        {
            if (PawnKindDefReplaceSettings.TryGetValue(PawnKindDefOf.Slave, out var pair) && pair.Any(x => spkd.pawnKindDefReplaceSettings.Any(y => y.replacedPawnKindDef == x.First)))
            {
                return true;
            }
            return false;
        }

        public static List<RaceAddonThingDef> AllRaceAddonThingDefs { get; set; } = new List<RaceAddonThingDef>();
        public static List<ThingDef> ApparelRestrictions { get; set; } = new List<ThingDef>();
        public static List<ThingDef> WeaponRestrictions { get; set; } = new List<ThingDef>();
        public static List<WorkGiverDef> WorkGiverRestrictions { get; set; } = new List<WorkGiverDef>();
        public static List<Trait> TraitRestrictions { get; set; } = new List<Trait>();
        public static List<HediffDef> HediffRestrictions { get; set; } = new List<HediffDef>();
        public static List<ThoughtDef> ThoughtRestrictions { get; set; } = new List<ThoughtDef>();
        public static List<ThingDef> BuildingRestrictions { get; set; } = new List<ThingDef>();
        public static List<ThingDef> FoodRestrictions { get; set; } = new List<ThingDef>();
        public static List<ThingDef> PlantRestrictions { get; set; } = new List<ThingDef>();
        public static List<ThingDef> AnimalRestrictions { get; set; } = new List<ThingDef>();
        public static List<RecipeDef> RecipeRestrictions { get; set; } = new List<RecipeDef>();
        public static List<ResearchProjectDef> ResearchRestrictions { get; set; } = new List<ResearchProjectDef>();

        public static bool CheckApparel(Pawn pawn, ThingDef apparel)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                if (pawn.GetComp<RaceAddonComp>()?.raceAddonGraphicSet?.presentAgeSetting?.allowedApparels is var list && list != null)
                {
                    if (list.Contains(apparel))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                var set = thingDef.raceAddonSettings.apparelRestrictionSetting;
                if (ApparelRestrictions.Contains(apparel))
                {
                    return set.raceSpecifics.Contains(apparel);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(apparel);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(apparel);
                    }
                }
            }
            else
            {
                return !ApparelRestrictions.Contains(apparel);
            }
        }
        public static bool CheckWeapon(ThingDef pawn, ThingDef weapon)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.weaponRestrictionSetting;
                if (WeaponRestrictions.Contains(weapon))
                {
                    return set.raceSpecifics.Contains(weapon);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(weapon);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(weapon);
                    }
                }
            }
            else
            {
                return !WeaponRestrictions.Contains(weapon);
            }
        }
        public static bool CheckWorkGiver(ThingDef pawn, WorkGiverDef workGiver)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.workGiverRestrictionSetting;
                if (WorkGiverRestrictions.Contains(workGiver))
                {
                    return set.raceSpecifics.Contains(workGiver);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(workGiver);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(workGiver);
                    }
                }
            }
            else
            {
                return !WorkGiverRestrictions.Contains(workGiver);
            }
        }
        public static bool CheckTrait(ThingDef pawn, Trait trait)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.basicSetting.raceSexuality != 0 && (trait.def == TraitDefOf.Asexual || trait.def == TraitDefOf.Bisexual || trait.def == TraitDefOf.Gay))
                {
                    return false;
                }
                var set = thingDef.raceAddonSettings.traitRestrictionSetting;
                if (TraitRestrictions.Contains(trait))
                {
                    return set.raceSpecifics.Any(x => x.traitDef == trait.def && x.degree == trait.Degree);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Any(x => x.traitDef == trait.def && x.degree == trait.Degree);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Any(x => x.traitDef == trait.def && x.degree == trait.Degree);
                    }
                }
            }
            else
            {
                return !TraitRestrictions.Contains(trait);
            }
        }
        public static bool CheckHediff(ThingDef pawn, HediffDef hediff)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.hediffRestrictionSetting;
                if (HediffRestrictions.Contains(hediff))
                {
                    return set.raceSpecifics.Contains(hediff);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(hediff);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(hediff);
                    }
                }
            }
            else
            {
                return !HediffRestrictions.Contains(hediff);
            }
        }
        public static bool CheckThought(ThingDef pawn, ThoughtDef thought)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.thoughtRestrictionSetting;
                if (ThoughtRestrictions.Contains(thought))
                {
                    return set.raceSpecifics.Contains(thought);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(thought);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(thought);
                    }
                }
            }
            else
            {
                return !ThoughtRestrictions.Contains(thought);
            }
        }
        public static bool CheckBuilding(ThingDef pawn, ThingDef building)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.buildingRestrictionSetting;
                if (BuildingRestrictions.Contains(building))
                {
                    return set.raceSpecifics.Contains(building);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(building);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(building);
                    }
                }
            }
            else
            {
                return !BuildingRestrictions.Contains(building);
            }
        }
        public static bool CheckFood(ThingDef pawn, ThingDef food)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.foodRestrictionSetting;
                if (FoodRestrictions.Contains(food))
                {
                    return set.raceSpecifics.Contains(food);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(food);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(food);
                    }
                }
            }
            else
            {
                return !FoodRestrictions.Contains(food);
            }
        }
        public static bool CheckPlant(ThingDef pawn, ThingDef plant)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.plantRestrictionSetting;
                if (PlantRestrictions.Contains(plant))
                {
                    return set.raceSpecifics.Contains(plant);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(plant);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(plant);
                    }
                }
            }
            else
            {
                return !PlantRestrictions.Contains(plant);
            }
        }
        public static bool CheckAnimal(ThingDef pawn, ThingDef animal)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.animalRestrictionSetting;
                if (AnimalRestrictions.Contains(animal))
                {
                    return set.raceSpecifics.Contains(animal);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(animal);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(animal);
                    }
                }
            }
            else
            {
                return !AnimalRestrictions.Contains(animal);
            }
        }
        public static bool CheckRecipe(ThingDef pawn, RecipeDef recipe)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.recipeRestrictionSetting;
                if (RecipeRestrictions.Contains(recipe))
                {
                    return set.raceSpecifics.Contains(recipe);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(recipe);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(recipe);
                    }
                }
            }
            else
            {
                return !RecipeRestrictions.Contains(recipe);
            }
        }
        public static bool CheckResearch(ThingDef pawn, ResearchProjectDef research)
        {
            if (pawn is RaceAddonThingDef thingDef)
            {
                var set = thingDef.raceAddonSettings.researchRestrictionSetting;
                if (ResearchRestrictions.Contains(research))
                {
                    return set.raceSpecifics.Contains(research);
                }
                else
                {
                    if (set.allAllow)
                    {
                        return !set.allAllow_Exceptions.Contains(research);
                    }
                    else
                    {
                        return set.allAllow_Exceptions.Contains(research);
                    }
                }
            }
            else
            {
                return !ResearchRestrictions.Contains(research);
            }
        }

        public static AgeSetting GetPresent(this List<AgeSetting> ageSettings, Pawn pawn)
        {
            AgeSetting result = null;
            foreach (var ageSetting in ageSettings.FindAll(x => x.gender == Gender.None || x.gender == pawn.gender))
            {
                if (ageSetting.minAge <= pawn.ageTracker.AgeBiologicalYearsFloat)
                {
                    result = ageSetting;
                }
            }
            return result;
        }

        public static AppearanceDef GetAppearanceDef(this List<AppearanceInfo> appearances, Pawn pawn, bool nullable = false)
        {
            if (appearances.FindAll(x => x.gender == Gender.None || x.gender == pawn.gender) is var list && list.Count > 0)
            {
                return list.RandomElementByWeight(x => x.weight).appearanceDef;
            }
            else if (!nullable)
            {
                Notify("Can't find a suitable AppearanceDef! => " + pawn.Name.ToStringFull, true);
                if (DefDatabase<AppearanceDef>.AllDefsListForReading.FindAll(x => x.modContentPack == pawn.def.modContentPack) is var all && all.Count > 0)
                {
                    return all.RandomElement();
                }
            }
            return null;
        }

        public static Dictionary<string, Color> GetPairs(this List<ColorSet> colorList, Pawn pawn)
        {
            Dictionary<string, Color> result = new Dictionary<string, Color>();
            foreach (var colorSet in colorList)
            {
                if ((ColorClass)Activator.CreateInstance(colorSet.colorClass) is var colorClass && colorClass != null)
                {
                    Color color = colorClass.GetColor(pawn, colorSet.options);
                    result.Add(colorSet.name, color);
                }
                else
                {
                    Notify("Can't make the color! => " + colorSet.name, true);
                }
            }
            return result;
        }

        public static Color GetColor(this Dictionary<string, Color> pairs, string name)
        {
            if (pairs.TryGetValue(name, out Color result))
            {
                return result;
            }
            else
            {
                Notify("Can't find the color! => " + name, true);
                return Color.white;
            }
        }

        public static void SetAppearance(Pawn pawn, RaceAddonComp racomp, RaceAddonThingDef thingDef, AppearanceDef appearanceDef)
        {
            Dictionary<string, Color> colorList = appearanceDef.colorList.GetPairs(pawn);

            racomp.skinColor_Main = Color.clear;
            racomp.skinColor_Sub = Color.clear;
            racomp.torsoDef = null;
            racomp.faceColor_Main = Color.clear;
            racomp.faceColor_Sub = Color.clear;
            racomp.upperFaceDef = null;
            racomp.lowerFaceDef = null;
            racomp.addonDatas = new List<AddonData>();

            var torsoSet = appearanceDef.torsoList?.Count > 0 ? appearanceDef.torsoList.RandomElementByWeight(x => x.weight) : null;
            var faceSet = appearanceDef.faceList?.Count > 0 ? appearanceDef.faceList.RandomElementByWeight(x => x.weight) : null;
            var hairSet = appearanceDef.hairList?.Count > 0 ? appearanceDef.hairList.RandomElementByWeight(x => x.weight) : null;
            var addonSet = appearanceDef.addonList?.Count > 0 ? appearanceDef.addonList.RandomElementByWeight(x => x.weight) : null;

            if (torsoSet != null)
            {
                racomp.skinColor_Main = colorList.GetColor(torsoSet.skinColor_Main);
                racomp.skinColor_Sub = colorList.GetColor(torsoSet.skinColor_Sub);
                racomp.torsoDef = torsoSet.torsoDef;
                pawn.story.bodyType = torsoSet.torsoDef.bodyType;
                Traverse.Create(pawn.story).Field("headGraphicPath").SetValue(torsoSet.torsoDef.headPath.normal);
                pawn.story.crownType = torsoSet.torsoDef.crownType;
            }
            if (faceSet != null)
            {
                racomp.faceColor_Main = colorList.GetColor(faceSet.faceColor_Main);
                racomp.faceColor_Sub = colorList.GetColor(faceSet.faceColor_Sub);
                racomp.upperFaceDef = faceSet.upperFaceDef;
                racomp.lowerFaceDef = faceSet.lowerFaceDef;
            }
            if (hairSet != null)
            {
                pawn.story.hairColor = colorList.GetColor(hairSet.hairColor_Main);
                racomp.hairColor_Sub = colorList.GetColor(hairSet.hairColor_Sub);
                List<HairDef> hairs = DefDatabase<HairDef>.AllDefsListForReading.FindAll((HairDef x) => x.hairTags.SharesElementWith(hairSet.hairTags));
                pawn.story.hairDef = hairs.RandomElementByWeight((HairDef x) =>
                Traverse.Create(typeof(PawnHairChooser)).Method("HairChoiceLikelihoodFor", new[] { typeof(HairDef), typeof(Pawn) }).GetValue<float>(x, pawn));
                if (thingDef.raceAddonSettings.healthSetting.greyHairAt != 0)
                {
                    int greyHairAt = thingDef.raceAddonSettings.healthSetting.greyHairAt;
                    if (pawn.ageTracker.AgeBiologicalYears > greyHairAt)
                    {
                        float num = GenMath.SmootherStep(greyHairAt, greyHairAt * 2, pawn.ageTracker.AgeBiologicalYears);
                        if (Rand.Value < num)
                        {
                            float num2 = Rand.Range(0.65f, 0.85f);
                            pawn.story.hairColor = new Color(num2, num2, num2);
                            racomp.hairColor_Sub = new Color(num2, num2, num2);
                        }
                    }
                }
            }
            if (addonSet != null)
            {
                foreach (var addon in addonSet.addons)
                {
                    var data = new AddonData
                    {
                        addonColor_Main = colorList.GetColor(addon.addonColor_Main),
                        addonColor_Sub = colorList.GetColor(addon.addonColor_Sub),
                        addonDef = addon.addonDef
                    };
                    racomp.addonDatas.Add(data);
                }
            }
        }

        public static Graphic GetGraphic(this string path, Shader shader, Color main, Color sub)
        {
            return path.NullOrEmpty() ? null : GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, main, sub);
        }

        public static BodyPartRecord GetBodyPartRecord(this string linkedBodyPart, BodyDef bodyDef)
        {
            return linkedBodyPart.NullOrEmpty() ? null : bodyDef.AllParts.Find(x => x.untranslatedCustomLabel.NullOrEmpty() && x.def.defName == linkedBodyPart || x.untranslatedCustomLabel == linkedBodyPart);
        }

        public static void Notify(string reason, bool error = false)
        {
            if (error)
            {
                Log.Error("[Garam, RaceAddon] " + reason);
            }
            else
            {
                Log.Warning("[Garam, RaceAddon] " + reason);
            }
        }

        public static SimpleBackstoryDef GetDef(this Backstory backstory)
        {
            return DefDatabase<SimpleBackstoryDef>.AllDefs.FirstOrDefault(x => x.Backstory == backstory);
            //return DefDatabase<SimpleBackstoryDef>.AllDefsListForReading.Find(x => x.Backstory == backstory);
        }

        public static bool OnlyUsePawnKindDefBackstories(this PawnKindDef pawnKindDef)
        {
            return DefDatabase<CustomPawnKindDef>.AllDefs.Any(x => x.onlyUsePawnKindDefBackstories.Contains(pawnKindDef));
        }

        public static SkillPair GetSkillPair(this List<SkillPair> skillLimits, SkillDef def, bool unlimit = false)
        {
            if (skillLimits.Find(x => x.skill == def) is var set && set != null && (unlimit || set.value <= 20f))
            {
                return set;
            }
            return null;
        }
    }
}
