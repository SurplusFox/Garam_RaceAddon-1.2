using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    [StaticConstructorOnStartup]
    public static class Mod
    {
        public const string ModVersion = "4.3.2-210101";

        static Mod()
        {
            Harmony harmony = new Harmony("com.rimworld.Dalrae.Garam_RaceAddon");
            harmony.PatchAll();
            harmony.Patch(AccessTools.FirstMethod(AccessTools.FirstInner(typeof(CharacterCardUtility), x => x.Name.Contains("14_1")), x => x.Name.Contains("b__21")), null, null, new HarmonyMethod(typeof(DrawCharacterCard), "Transpiler"));
            foreach (var type in typeof(WorkGiver).AllSubclasses())
            {
                if (AccessTools.DeclaredMethod(type, "ShouldSkip") is var methodInfo && methodInfo != null)
                {
                    harmony.Patch(methodInfo, null, new HarmonyMethod(typeof(ShouldSkip), "Postfix"), null, null);
                }
            }
            foreach (var thingDef in DefDatabase<RaceAddonThingDef>.AllDefsListForReading)
            {
                if (thingDef.raceAddonSettings.basicSetting.recipeImportTarget != null)
                {
                    // Recipe Import
                    ThingDef targetThingDef = DefDatabase<ThingDef>.AllDefs.First((ThingDef def) => def == thingDef.raceAddonSettings.basicSetting.recipeImportTarget);
                    thingDef.recipes = thingDef.recipes ?? new List<RecipeDef>();
                    foreach (RecipeDef recipe in targetThingDef.AllRecipes)
                    {
                        if (RecipeImport(recipe, thingDef.race.body))
                        {
                            thingDef.recipes.AddDistinct(recipe);
                        }
                    }
                }
            }

            foreach (var customPawnKindDef in DefDatabase<CustomPawnKindDef>.AllDefsListForReading)
            {
                foreach (var setting in customPawnKindDef.pawnKindDefReplaceSettings)
                {
                    if (!RaceAddon.PawnKindDefReplaceSettings.ContainsKey(setting.originalPawnKindDef))
                    {
                        RaceAddon.PawnKindDefReplaceSettings.Add(setting.originalPawnKindDef, new List<Pair<PawnKindDef, float>>());
                    }
                    if (RaceAddon.PawnKindDefReplaceSettings[setting.originalPawnKindDef].Count > 0)
                    {
                        for (int i = 0; i < RaceAddon.PawnKindDefReplaceSettings[setting.originalPawnKindDef].Count; i++)
                        {
                            if (setting.originalWeight < RaceAddon.PawnKindDefReplaceSettings[setting.originalPawnKindDef][i].Second)
                            {
                                RaceAddon.PawnKindDefReplaceSettings[setting.originalPawnKindDef][i] = new Pair<PawnKindDef, float>(setting.originalPawnKindDef, setting.originalWeight);
                            }
                        }
                    }
                    else
                    {
                        RaceAddon.PawnKindDefReplaceSettings[setting.originalPawnKindDef].Add(new Pair<PawnKindDef, float>(setting.originalPawnKindDef, setting.originalWeight));
                    }
                    RaceAddon.PawnKindDefReplaceSettings[setting.originalPawnKindDef].Add(new Pair<PawnKindDef, float>(setting.replacedPawnKindDef, setting.replacedWeight));
                }
            }
        }

        private static bool RecipeImport(RecipeDef recipe, BodyDef body)
        {
            return !recipe.targetsBodyPart || recipe.appliedOnFixedBodyParts.NullOrEmpty() || recipe.appliedOnFixedBodyParts.Any(def => body.AllParts.Any(bpr => bpr.def == def));
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class GaramTexturePath : Attribute
    {

    }
}
