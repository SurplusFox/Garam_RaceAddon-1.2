using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("TryGenerateNewPawnInternal")]
    public static class TryGenerateNewPawnInternal
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator il)
        {
            Label end = il.DefineLabel();
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == AccessTools.Method(typeof(RaceProperties), "get_Humanlike"))
                {
                    yield return new CodeInstruction(OpCodes.Brfalse, end);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TryGenerateNewPawnInternal), "RaceAddonGenerator"));
                }
                if (code.opcode == OpCodes.Call && (MethodInfo)code.operand == AccessTools.Method(typeof(PawnGenerator), "GenerateSkills"))
                {
                    yield return new CodeInstruction(opcode: OpCodes.Nop) { labels = new List<Label> { end } };
                }
            }
        }

        public static bool RaceAddonGenerator(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                FactionDef factionDef;
                if (request.Faction != null)
                {
                    factionDef = request.Faction.def;
                }
                else if (Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction_NewTemp(out Faction faction, false, true, TechLevel.Undefined))
                {
                    factionDef = faction.def;
                }
                else
                {
                    factionDef = Faction.OfAncients.def;
                }
                // Race Addon Comp
                var racomp = pawn.GetComp<RaceAddonComp>();
                // Backstory
                /*
                if (request.Newborn && factionDef == Find.FactionManager.OfPlayer.def && thingDef.raceAddonSettings.ageSettings[0].ageBackstory != null)
                {
                    pawn.story.childhood = thingDef.raceAddonSettings.ageSettings[0].ageBackstory.Backstory;
                }
                */
                GiveAppropriateBioAndNameTo.cachedNewborn = request.Newborn;
                PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, request.FixedLastName, factionDef);
                GiveAppropriateBioAndNameTo.cachedNewborn = false;
                SimpleBackstoryDef backstoryDef = null;
                foreach (var backstory in pawn.story.AllBackstories)
                {
                    backstoryDef = backstory.GetDef();
                    if (backstoryDef != null)
                    {
                        // set age
                        bool flag1 = false;
                        bool flag2 = false;
                        if (backstoryDef.bioAgeRange.Average >= 0)
                        {
                            request.FixedBiologicalAge = backstoryDef.bioAgeRange.RandomInRange;
                            flag1 = true;
                        }
                        if (backstoryDef.chronoAgeRange.Average >= 0)
                        {
                            request.FixedChronologicalAge = flag1 ? request.FixedBiologicalAge + backstoryDef.chronoAgeRange.RandomInRange : pawn.ageTracker.AgeBiologicalYearsFloat + backstoryDef.chronoAgeRange.RandomInRange;
                            flag2 = true;
                        }
                        if (flag1 || flag2)
                        {
                            AccessTools.Method(typeof(PawnGenerator), "GenerateRandomAge").Invoke(null, new object[] { pawn, request });
                        }
                        // set gender
                        if (backstoryDef.maleChance >= 0f)
                        {
                            pawn.gender = Rand.Chance(backstoryDef.maleChance) ? Gender.Male : Gender.Female;
                            request.FixedGender = pawn.gender;
                        }
                        else
                        {
                            pawn.gender = Rand.Chance(thingDef.raceAddonSettings.basicSetting.maleChance) ? Gender.Male : Gender.Female;
                            request.FixedGender = pawn.gender;
                        }
                    }
                }
                // Fix Name
                pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn, NameStyle.Full, request.FixedLastName);
                // Choose AppearanceDef
                AppearanceDef appearanceDef = null;
                if (backstoryDef?.appearances?.GetAppearanceDef(pawn) is var def && def != null)
                {
                    appearanceDef = def;
                }
                else
                {
                    var presentAgeSetting = thingDef.raceAddonSettings.ageSettings.GetPresent(pawn);
                    for (int i = thingDef.raceAddonSettings.ageSettings.IndexOf(presentAgeSetting); i < thingDef.raceAddonSettings.ageSettings.Count; i++)
                    {
                        var set = thingDef.raceAddonSettings.ageSettings[i];
                        if (set.appearances?.GetAppearanceDef(pawn, true) is var result && result != null)
                        {
                            appearanceDef = result;
                            break;
                        }
                    }
                }

                RaceAddon.SetAppearance(pawn, racomp, thingDef, appearanceDef);
                racomp.drawSizeDeviation = thingDef.raceAddonSettings.graphicSetting.drawSizeCurve != null ? Rand.ByCurve(thingDef.raceAddonSettings.graphicSetting.drawSizeCurve) : 1.0f;
                AccessTools.Method(typeof(PawnGenerator), "GenerateTraits").Invoke(null, new object[] { pawn, request });
                AccessTools.Method(typeof(PawnGenerator), "GenerateBodyType").Invoke(null, new object[] { pawn });
                AccessTools.Method(typeof(PawnGenerator), "GenerateSkills").Invoke(null, new object[] { pawn });

                if (thingDef.raceAddonSettings.characterizationSetting.skillGains.Count > 0)
                {
                    foreach (var skillGain in thingDef.raceAddonSettings.characterizationSetting.skillGains)
                    {
                        pawn.skills.Learn(skillGain.skill, skillGain.value, true);
                        foreach (var skill in pawn.skills.skills)
                        {
                            if (skill.def == skillGain.skill)
                            {
                                skill.Level += (int)skillGain.value;
                            }
                        }
                    }
                }

                racomp.pawnGeneratedVersion = Mod.ModVersion;

                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawn", new[] { typeof(PawnGenerationRequest) })]
    public static class GeneratePawn
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(ref PawnGenerationRequest request)
        {
            if (!request.Newborn)
            {
                request.KindDef = RaceAddon.GetReplacedPawnKindDef(request.KindDef);
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(ref Pawn __result)
        {
            if (__result.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.characterizationSetting.forcedHediffs.Count > 0)
                {
                    foreach (var set in thingDef.raceAddonSettings.characterizationSetting.forcedHediffs)
                    {
                        if (Rand.Chance(set.chance))
                        {
                            Hediff hediff = HediffMaker.MakeHediff(set.hediffDef, __result, set.targetPart.GetBodyPartRecord(__result.def.race.body));
                            hediff.Severity = set.severity;
                            __result.health.AddHediff(hediff);
                        }
                    }
                }

                foreach (var backstory in __result.story.AllBackstories)
                {
                    if (backstory.GetDef() is var def && def != null && def.forcedHediffs.Count > 0)
                    {
                        foreach (var set in def.forcedHediffs)
                        {
                            if (Rand.Chance(set.chance))
                            {
                                Hediff hediff = HediffMaker.MakeHediff(set.hediffDef, __result, set.targetPart.GetBodyPartRecord(__result.def.race.body));
                                hediff.Severity = set.severity;
                                __result.health.AddHediff(hediff);
                            }
                        }
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(PawnBioAndNameGenerator))]
    [HarmonyPatch("GiveAppropriateBioAndNameTo")]
    public static class GiveAppropriateBioAndNameTo
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Call && (MethodInfo)code.operand == AccessTools.Method(typeof(PawnBioAndNameGenerator), "GetBackstoryCategoryFiltersFor"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GiveAppropriateBioAndNameTo), "GetNewbornBackstoryCategories"));
                }
            }
        }

        public static List<BackstoryCategoryFilter> GetNewbornBackstoryCategories(List<BackstoryCategoryFilter> original, Pawn pawn)
        {
            if (cachedNewborn && pawn.IsColonist && pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.newbornBackstoryCategoryFilters != null)
            {
                return thingDef.raceAddonSettings.basicSetting.newbornBackstoryCategoryFilters;
            }
            return original;
        }

        public static bool cachedNewborn = false;
    }
    
    [HarmonyPatch(typeof(PawnBioAndNameGenerator))]
    [HarmonyPatch("GetBackstoryCategoryFiltersFor")]
    public static class GetBackstoryCategoryFiltersFor
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref List<BackstoryCategoryFilter> __result)
        {
            if (pawn.kindDef.OnlyUsePawnKindDefBackstories())
            {
                if (pawn.kindDef.backstoryFilters?.Count > 0)
                {
                    __result = pawn.kindDef.backstoryFilters;
                }
                else if (pawn.kindDef.backstoryFiltersOverride?.Count > 0)
                {
                    __result = pawn.kindDef.backstoryFiltersOverride;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnBioAndNameGenerator))]
    [HarmonyPatch("FillBackstorySlotShuffled")]
    public static class FillBackstorySlotShuffled
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref Backstory backstory)
        {
            if (pawn.def is RaceAddonThingDef && backstory != null)
            {
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, BackstorySlot slot, ref Backstory backstory)
        {
            if (backstory.GetDef() is var def && def != null && def.backstoryInfo.requiredBackstories != null)
            {
                if (slot == BackstorySlot.Childhood)
                {
                    if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
                    {
                        pawn.story.adulthood = def.backstoryInfo.requiredBackstories.RandomElement().Backstory;
                    }
                }
                else
                {
                    pawn.story.childhood = def.backstoryInfo.requiredBackstories.RandomElement().Backstory;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnBioAndNameGenerator))]
    [HarmonyPatch("TryGiveSolidBioTo")]
    public static class TryGiveSolidBioTo
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref bool __result)
        {
            if (pawn.story.childhood != null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateBodyType_NewTemp")]
    public static class GenerateBodyType_NewTemp
    {
        [HarmonyPriority(-24816)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.story.bodyType != null)
            {
                return false;
            }
            return true;
        }
    }
}