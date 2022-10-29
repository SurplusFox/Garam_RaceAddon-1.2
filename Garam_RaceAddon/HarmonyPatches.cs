using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(Pawn_StoryTracker))]
    [HarmonyPatch("SkinColor", MethodType.Getter)]
    public static class SkinColor
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, ref Color __result)
        {
            if (___pawn.def is RaceAddonThingDef)
            {
                RaceAddonComp racomp = ___pawn.GetComp<RaceAddonComp>();
                if (racomp.skinColor_Main != Color.clear)
                {
                    __result = racomp.skinColor_Main;
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(TraitSet))]
    [HarmonyPatch("HasTrait")]
    public static class HasTrait
    {
        [HarmonyPostfix]
        public static void Postfix(TraitDef tDef, Pawn ___pawn, ref bool __result)
        {
            if (tDef == TraitDefOf.Asexual)
            {
                if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.raceSexuality == 1)
                {
                    __result = true;
                    return;
                }
            }
            if (tDef == TraitDefOf.Bisexual)
            {
                if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.raceSexuality == 2)
                {
                    __result = true;
                    return;
                }
            }
            if (tDef == TraitDefOf.Gay)
            {
                if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.basicSetting.raceSexuality == 3)
                {
                    __result = true;
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(HediffSet))]
    [HarmonyPatch("HasHead", MethodType.Getter)]
    public static class HasHead
    {
        [HarmonyPrefix]
        public static bool Prefix(HediffSet __instance, bool? ___cachedHasHead, ref bool __result)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                bool? flag = ___cachedHasHead;
                if (!flag.HasValue)
                {
                    ___cachedHasHead = new bool?(__instance.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).Any((BodyPartRecord x) => x.def == thingDef.raceAddonSettings.basicSetting.raceHeadDef));
                }
                __result = ___cachedHasHead.Value;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn))]    
    [HarmonyPatch("PreApplyDamage")]
    public static class PreApplyDamage
    {
        [HarmonyPrefix]
        public static bool Prefix(ref DamageInfo dinfo, Pawn __instance)
        {
            if (dinfo.Instigator is Pawn from
                && from != null
                && from.def is RaceAddonThingDef fromThingDef
                && from.CurJob?.def == JobDefOf.SocialFight
                && fromThingDef.raceAddonSettings.basicSetting.maxDamageForSocialfight != 0)
            {
                dinfo.SetAmount(Mathf.Min(dinfo.Amount, fromThingDef.raceAddonSettings.basicSetting.maxDamageForSocialfight));
            }
            if (__instance.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.damageFactor != 1f)
            {
                dinfo.SetAmount(dinfo.Amount * thingDef.raceAddonSettings.healthSetting.damageFactor);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch("IsHumanlikeMeat")]
    public static class IsHumanlikeMeat
    {
        [HarmonyPostfix]
        public static void Postfix(ThingDef def, ref bool __result)
        {
            if (__result && def.ingestible.sourceDef is RaceAddonThingDef thingDef)
            {
                __result = thingDef.raceAddonSettings.basicSetting.humanlikeMeat;
            }
        }
    }

    [HarmonyPatch(typeof(FoodUtility))]
    [HarmonyPatch("IsHumanlikeMeatOrHumanlikeCorpse")]
    public static class IsHumanlikeMeatOrHumanlikeCorpse
    {
        [HarmonyPostfix]
        public static void Postfix(Thing thing, ref bool __result)
        {
            if (__result && thing is Corpse corpse && corpse != null && corpse.InnerPawn.def is RaceAddonThingDef thingDef)
            {
                __result = thingDef.raceAddonSettings.basicSetting.humanlikeMeat;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_AgeTracker))]
    [HarmonyPatch("BirthdayBiological")]
    public static class BirthdayBiological
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn)
        {
            if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.antiAging)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AgeInjuryUtility))]
    [HarmonyPatch("GenerateRandomOldAgeInjuries")]
    public static class GenerateRandomOldAgeInjuries
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.antiAging)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(AgeInjuryUtility))]
    [HarmonyPatch("RandomHediffsToGainOnBirthday", new[] { typeof(ThingDef), typeof(int) })]
    public static class RandomHediffsToGainOnBirthday
    {
        [HarmonyPrefix]
        public static bool Prefix(ThingDef raceDef)
        {
            if (raceDef is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.healthSetting.antiAging)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("HealthTick")]
    public static class HealthTick
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Ldc_I4 && (int)code.operand == 600)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Pawn_HealthTracker), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HealthTick), "GetTick"));
                }
            }
        }

        public static int GetTick(int tick, Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                tick = thingDef.raceAddonSettings.healthSetting.healingRate;
            }
            return tick;
        }
    }

    [HarmonyPatch(typeof(Hediff_Injury))]
    [HarmonyPatch("Heal")]
    public static class Heal
    {
        [HarmonyPrefix]
        public static bool Prefix(Hediff_Injury __instance, ref float amount)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                amount *= thingDef.raceAddonSettings.healthSetting.healingFactor;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HediffSet))]
    [HarmonyPatch("CalculatePain")]
    public static class CalculatePain
    {
        [HarmonyPostfix]
        public static void Postfix(HediffSet __instance, ref float __result)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                __result *= thingDef.raceAddonSettings.healthSetting.painFactor;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("DropBloodFilth")]
    public static class DropBloodFilth
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn)
        {
            if (___pawn.def is RaceAddonThingDef thingDef && !thingDef.raceAddonSettings.healthSetting.dropBloodFilth)
            {
                return false;
            }
            return true;
        }
    }

    public static class DrawCharacterCard
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            var type1 = AccessTools.FirstInner(typeof(CharacterCardUtility), x => x.Name.Contains("14_1"));
            var type2 = AccessTools.FirstInner(typeof(CharacterCardUtility), x => x.Name.Contains("14_0"));
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Call && code.operand.ToString().Contains("Font"))
                {
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 1); // sectionRect
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0); // num12
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(type1, "leftRect")); // leftRect
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type1, "CS$<>8__locals1"));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type2, "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawCharacterCard), "GetRaceStory"));
                }
            }
        }

        public static void GetRaceStory(ref Rect sectionRect, ref float num12, ref Rect leftRect, Pawn pawn)
        {
            Rect rect13 = new Rect(sectionRect.x, num12, leftRect.width, 22f);
            if (Mouse.IsOver(rect13))
            {
                Widgets.DrawHighlight(rect13);
            }
            if (Mouse.IsOver(rect13))
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (pawn.def is RaceAddonThingDef thingDef)
                {
                    if (thingDef.raceAddonSettings.basicSetting.shortDescriptionForRaceStory.NullOrEmpty())
                    {
                        stringBuilder.AppendLine(pawn.def.description.Translate());
                    }
                    else
                    {
                        string description = pawn.def.description.Translate().ToString();
                        int index = description.IndexOf(thingDef.raceAddonSettings.basicSetting.shortDescriptionForRaceStory.Translate());
                        if (index > 0)
                        {
                            stringBuilder.AppendLine(description.Remove(description.IndexOf(thingDef.raceAddonSettings.basicSetting.shortDescriptionForRaceStory)));
                        }
                        else
                        {
                            stringBuilder.AppendLine(pawn.def.description.Translate());
                        }
                    }
                    if (thingDef.raceAddonSettings.characterizationSetting.skillGains.Count > 0)
                    {
                        stringBuilder.AppendLine();
                        List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                        for (int i = 0; i < allDefsListForReading.Count; i++)
                        {
                            SkillDef skillDef = allDefsListForReading[i];
                            if (thingDef.raceAddonSettings.characterizationSetting.skillGains.Find(x => x.skill == skillDef) is var skill && skill != null)
                            {
                                stringBuilder.AppendLine(skillDef.skillLabel.CapitalizeFirst() + ":   " + skill.value.ToString("+##;-##"));
                            }
                        }
                    }
                    foreach (WorkTypeDef disabledWorkType in thingDef.DisabledWorkTypes)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine(disabledWorkType.gerundLabel.CapitalizeFirst() + " " + "DisabledLower".Translate());
                    }
                    foreach (WorkGiverDef disabledWorkGiver in thingDef.DisabledWorkGivers)
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine(disabledWorkGiver.workType.gerundLabel.CapitalizeFirst() + ": " + disabledWorkGiver.LabelCap + " " + "DisabledLower".Translate());
                    }
                }
                string str = stringBuilder.ToString().TrimEndNewlines();
                TooltipHandler.TipRegion(rect13, Find.ActiveLanguageWorker.PostProcessed(str));
            }
            Text.Anchor = TextAnchor.MiddleLeft;
            string str2 = "RaceAddon_Race".Translate();
            Widgets.Label(rect13, str2 + ":");
            Text.Anchor = TextAnchor.UpperLeft;
            Rect rect14 = new Rect(rect13);
            rect14.x += 90f;
            rect14.width -= 90f;
            string str3 = pawn.def.label.Translate();
            Widgets.Label(rect14, str3.Truncate(rect14.width));
            num12 += rect13.height;
        }
    }

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GeneratePawnRelations")]
    public static class GeneratePawnRelations
    {
        private static readonly PawnRelationDef[] blood = DefDatabase<PawnRelationDef>.AllDefsListForReading.FindAll
            ((PawnRelationDef x) => x.familyByBloodRelation && x.generationChanceFactor > 0f).ToArray();
        private static readonly PawnRelationDef[] nonBlood = DefDatabase<PawnRelationDef>.AllDefsListForReading.FindAll
            ((PawnRelationDef x) => !x.familyByBloodRelation && x.generationChanceFactor > 0f).ToArray();
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref PawnGenerationRequest request)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                if (!thingDef.raceAddonSettings.relationSetting.randomRelationAllow)
                {
                    return false;
                }
                Pawn[] targets = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
                                  where x.def == pawn.def
                                  select x).ToArray();
                if (targets.Length == 0)
                {
                    return false;
                }
                float num = 45f;
                num += targets.ToList().FindAll((Pawn x) => !x.Discarded).Count * 2.7f;

                List<Pair<Pawn, PawnRelationDef>> pairs = new List<Pair<Pawn, PawnRelationDef>>();
                foreach (Pawn target in targets)
                {
                    pairs.Add(new Pair<Pawn, PawnRelationDef>(target, blood.RandomElement()));
                }
                PawnGenerationRequest localReq = request;
                Pair<Pawn, PawnRelationDef> pair = pairs.RandomElementByWeightWithDefault
                    ((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num * 40f / (targets.Count() * blood.Count()));
                if (pair.First != null)
                {
                    pair.Second.Worker.CreateRelation(pawn, pair.First, ref request);
                }
                Pair<Pawn, PawnRelationDef> pair2 = pairs.RandomElementByWeightWithDefault
                    ((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num * 40f / (targets.Count() * nonBlood.Count()));
                if (pair2.First != null)
                {
                    pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
                }
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker))]
    [HarmonyPatch("CompatibilityWith")]
    public static class CompatibilityWith
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn_RelationsTracker __instance, Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            if (!___pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || ___pawn == otherPawn)
            {
                __result = 0f;
                return false;
            }
            float num1 = Mathf.Abs(___pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num2 = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, num1);
            num2 = Mathf.Clamp(num2, -0.45f, 0.45f);
            float num3 = __instance.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);

            float num4 = 0f;

            if (___pawn.def is RaceAddonThingDef thingDef1)
            {
                if (thingDef1.raceAddonSettings.relationSetting.specialRaces.Find(x => x.raceDef == otherPawn.def) is var set && set != null)
                {
                    num4 = set.compatibilityBonus;
                }
                else if (___pawn.def == otherPawn.def || thingDef1.raceAddonSettings.relationSetting.sameRaces.Contains(otherPawn.def))
                {
                    num4 = thingDef1.raceAddonSettings.relationSetting.sameRace.compatibilityBonus;
                }
                else
                {
                    num4 = thingDef1.raceAddonSettings.relationSetting.otherRace.compatibilityBonus;
                }
            }

            if (otherPawn.def is RaceAddonThingDef thingDef2)
            {
                if (thingDef2.raceAddonSettings.relationSetting.specialRaces.Find(x => x.raceDef == otherPawn.def) is var set && set != null)
                {
                    num4 = set.compatibilityBonus;
                }
                else if (___pawn.def == otherPawn.def || thingDef2.raceAddonSettings.relationSetting.sameRaces.Contains(otherPawn.def))
                {
                    num4 = thingDef2.raceAddonSettings.relationSetting.sameRace.compatibilityBonus;
                }
                else
                {
                    num4 = thingDef2.raceAddonSettings.relationSetting.otherRace.compatibilityBonus;
                }
            }

            __result = num2 + num3 + num4;
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn_RelationsTracker))]
    [HarmonyPatch("SecondaryLovinChanceFactor")]
    public static class SecondaryLovinChanceFactor
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, Pawn otherPawn, ref float __result)
        {
            Pawn pawn = ___pawn;
            if (!pawn.RaceProps.Humanlike || !otherPawn.RaceProps.Humanlike || ___pawn == otherPawn)
            {
                __result = 0f;
                return false;
            }
            if (pawn.story != null && pawn.story.traits != null)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Asexual)) //무성애 = 0f
                {
                    __result = 0f;
                    return false;
                }
                if (!pawn.story.traits.HasTrait(TraitDefOf.Bisexual)) //!양성애
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (otherPawn.gender != pawn.gender) //동성애 && 이성 = 0f
                        {
                            __result = 0f;
                            return false;
                        }
                    }
                    else if (otherPawn.gender == pawn.gender) //이성애 && 동성 = 0f
                    {
                        __result = 0f;
                        return false;
                    }
                }
            }
            float ageBiologicalYearsFloat = pawn.ageTracker.AgeBiologicalYearsFloat;
            float ageBiologicalYearsFloat2 = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if (ageBiologicalYearsFloat < 16f || ageBiologicalYearsFloat2 < 16f)
            {
                __result = 0f;
                return false;
            }
            float defaultValue = 1f;
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                LoveChanceFactor factor = null;
                if (thingDef.raceAddonSettings.relationSetting.specialRaces.Find(x => x.raceDef == otherPawn.def) is var set && set != null) // 특별한 종족
                {
                    if (otherPawn.gender == Gender.Male) // 특별한 종족 - 남성
                    {
                        factor = set.loveChanceFactor_Male;
                    }
                    else if (otherPawn.gender == Gender.Female) // 특별한 종족 - 여성
                    {
                        factor = set.loveChanceFactor_Female;
                    }
                }
                else if (pawn.def == otherPawn.def || thingDef.raceAddonSettings.relationSetting.sameRaces.Contains(otherPawn.def)) // 같은 종족
                {
                    if (otherPawn.gender == Gender.Male) // 같은 종족 - 남성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.sameRace.loveChanceFactor_Male;
                    }
                    else if (otherPawn.gender == Gender.Female) // 같은 종족 - 여성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.sameRace.loveChanceFactor_Female;
                    }
                }
                else // 다른 종족
                {
                    if (otherPawn.gender == Gender.Male) // 다른 종족 - 남성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.otherRace.loveChanceFactor_Male;
                    }
                    else if (otherPawn.gender == Gender.Female) // 다른 종족 - 여성
                    {
                        factor = thingDef.raceAddonSettings.relationSetting.otherRace.loveChanceFactor_Female;
                    }
                }
                if (factor != null) // 계산 시작
                {
                    float 최소나이값 = factor.minAgeValue;
                    float 상대최소나이 = ageBiologicalYearsFloat - factor.minAgeDifference;
                    float 상대적은나이 = ageBiologicalYearsFloat - factor.lowerAgeDifference;
                    float 상대많은나이 = ageBiologicalYearsFloat + factor.upperAgeDifference;
                    float 상대최대나이 = ageBiologicalYearsFloat + factor.maxAgeDifference;
                    float 최대나이값 = factor.maxAgeValue;
                    defaultValue = 평탄한꼭대기(최소나이값, 상대최소나이, 상대적은나이, 상대많은나이, 상대최대나이, 최대나이값, ageBiologicalYearsFloat2);
                }
            }
            else // 바닐라
            {
                if (pawn.gender == Gender.Male)
                {
                    float min = ageBiologicalYearsFloat - 30f;
                    float lower = ageBiologicalYearsFloat - 10f;
                    float upper = ageBiologicalYearsFloat + 3f;
                    float max = ageBiologicalYearsFloat + 10f;
                    defaultValue = GenMath.FlatHill(0.2f, min, lower, upper, max, 0.2f, ageBiologicalYearsFloat2);
                }
                else if (pawn.gender == Gender.Female)
                {
                    float min2 = ageBiologicalYearsFloat - 10f;
                    float lower2 = ageBiologicalYearsFloat - 3f;
                    float upper2 = ageBiologicalYearsFloat + 10f;
                    float max2 = ageBiologicalYearsFloat + 30f;
                    defaultValue = GenMath.FlatHill(0.2f, min2, lower2, upper2, max2, 0.2f, ageBiologicalYearsFloat2);
                }
            }
            float ageFactor_Pawn = Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat);
            float ageFactor_OtherPawn = Mathf.InverseLerp(16f, 18f, ageBiologicalYearsFloat2);
            float beauty = otherPawn.GetStatValue(StatDefOf.PawnBeauty, true);
            float beautyFactor = 1f;
            if (beauty < 0f)
            {
                beautyFactor = 0.3f;
            }
            else if (beauty > 0f)
            {
                beautyFactor = 2.3f;
            }
            __result = defaultValue * ageFactor_Pawn * ageFactor_OtherPawn * beautyFactor;
            return false;
        }
        private static float 평탄한꼭대기(float 최소나이값, float 상대최소나이, float 상대적은나이, float 상대많은나이, float 상대최대나이, float 최대나이값, float 상대나이)
        {
            if (상대나이 < 상대최소나이)
            {
                return 최소나이값;
            }
            if (상대나이 < 상대적은나이)
            {
                return GenMath.LerpDouble(상대최소나이, 상대적은나이, 최소나이값, 1f, 상대나이);
            }
            if (상대나이 < 상대많은나이)
            {
                return 1f;
            }
            if (상대나이 < 상대최대나이)
            {
                return GenMath.LerpDouble(상대많은나이, 상대최대나이, 1f, 최대나이값, 상대나이);
            }
            return 최대나이값;
        }
    }

    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("GetFather")]
    public static class GetFather
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref Pawn __result)
        {
            if (pawn.RaceProps.IsFlesh && __result == null)
            {
                foreach (var relation in pawn.relations.DirectRelations)
                {
                    if (relation.def == PawnRelationDefOf.Parent && relation.otherPawn != pawn.GetMother())
                    {
                        __result = relation.otherPawn;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("GetMother")]
    public static class GetMother
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref Pawn __result)
        {
            if (pawn.RaceProps.IsFlesh && __result == null)
            {
                foreach (var relation in pawn.relations.DirectRelations)
                {
                    if (relation.def == PawnRelationDefOf.Parent && relation.otherPawn != pawn.GetFather())
                    {
                        __result = relation.otherPawn;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("SetFather")]
    public static class SetFather
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Pawn newFather)
        {
            Pawn father = pawn.GetFather();
            if (father != newFather)
            {
                if (father != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, father);
                }
                if (newFather != null)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newFather);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ParentRelationUtility))]
    [HarmonyPatch("SetMother")]
    public static class SetMother
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Pawn newMother)
        {
            Pawn mother = pawn.GetMother();
            if (mother != newMother)
            {
                if (mother != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Parent, mother);
                }
                if (newMother != null)
                {
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.Parent, newMother);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnApparelGenerator))]
    [HarmonyPatch("GenerateStartingApparelFor")]
    public static class GenerateStartingApparelFor
    {
        private static List<ThingStuffPair> savedAllApparelPairs;

        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref List<ThingStuffPair> ___allApparelPairs)
        {
            if (savedAllApparelPairs == null)
            {
                savedAllApparelPairs = ___allApparelPairs.ListFullCopy();
            }
            ___allApparelPairs = savedAllApparelPairs.FindAll((ThingStuffPair x) => RaceAddon.CheckApparel(pawn, x.thing));
            return true;
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel))]
    [HarmonyPatch("ApparelScoreGain_NewTmp")]
    public static class ApparelScoreGain
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, Apparel ap, ref float __result)
        {
            if (!RaceAddon.CheckApparel(pawn, ap.def))
            {
                __result = int.MinValue;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(JobGiver_PrisonerGetDressed))]
    [HarmonyPatch("FindGarmentCoveringPart")]
    public static class FindGarmentCoveringPart
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, BodyPartGroupDef bodyPartGroupDef, ref Apparel __result)
        {
            Room room = pawn.GetRoom(RegionType.Set_Passable);
            if (room.isPrisonCell)
            {
                foreach (IntVec3 current in room.Cells)
                {
                    List<Thing> thingList = current.GetThingList(pawn.Map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        if (thingList[i] is Apparel apparel &&
                            RaceAddon.CheckApparel(pawn, apparel.def) &&
                            apparel.def.apparel.bodyPartGroups.Contains(bodyPartGroupDef) &&
                            pawn.CanReserve(apparel, 1, -1, null, false) && !apparel.IsBurning() &&
                            ApparelUtility.HasPartsToWear(pawn, apparel.def))
                        {
                            __result = apparel;
                            return false;
                        }
                    }
                }
            }
            __result = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(PawnWeaponGenerator))]
    [HarmonyPatch("TryGenerateWeaponFor")]
    public static class TryGenerateWeaponFor
    {
        private static List<ThingStuffPair> savedAllWeaponPairs;

        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, ref List<ThingStuffPair> ___allWeaponPairs)
        {
            if (savedAllWeaponPairs == null)
            {
                savedAllWeaponPairs = ___allWeaponPairs.ListFullCopy();
            }
            ___allWeaponPairs = savedAllWeaponPairs.FindAll((ThingStuffPair x) => RaceAddon.CheckWeapon(pawn.def, x.thing));
            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("CombinedDisabledWorkTags", MethodType.Getter)]
    public static class CombinedDisabledWorkTags
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, ref WorkTags __result)
        {
            if (__instance.def is RaceAddonThingDef thingDef)
            {
                __result |= thingDef.DisabledWorkTags;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_StoryTracker))]
    [HarmonyPatch("DisabledWorkTagsBackstoryAndTraits", MethodType.Getter)]
    public static class DisabledWorkTagsBackstoryAndTraits
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn, ref WorkTags __result)
        {
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                __result |= thingDef.DisabledWorkTags;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("GetDisabledWorkTypes")]
    public static class GetDisabledWorkTypes
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, ref IEnumerable<WorkTypeDef> __result)
        {
            if (__instance.def is RaceAddonThingDef thingDef)
            {
                foreach (WorkTypeDef def in thingDef.DisabledWorkTypes)
                {
                    if (!__result.Contains(def))
                    {
                        __result.AddItem(def);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(WorkGiver))]
    [HarmonyPatch("ShouldSkip")]
    public static class ShouldSkip
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, WorkGiver __instance, ref bool __result)
        {
            if (!__result && !RaceAddon.CheckWorkGiver(pawn.def, __instance.def))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator))]
    [HarmonyPatch("GenerateTraits")]
    public static class GenerateTraits
    {
        /*
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.story != null)
            {

            }
            return true;
        }
        */
        [HarmonyPriority(int.MaxValue)]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator il)
        {
            var target_GenerateForcedTraits = codes.ToList().FindAll(x => x.opcode == OpCodes.Ldarga_S && (byte)x.operand == 1)[0];
            var target_Gay = codes.ToList().FindAll(x => x.opcode == OpCodes.Ldsfld && (FieldInfo)x.operand == AccessTools.Field(typeof(TraitDefOf), "Gay"))[0];
            var target_End = codes.ToList().FindAll(x => x.opcode == OpCodes.Callvirt && (MethodInfo)x.operand == AccessTools.Method(typeof(TraitSet), "GainTrait"))[4];
            Label end = il.DefineLabel();
            foreach (var code in codes)
            {
                if (code == target_Gay)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.FirstInner(typeof(PawnGenerator), x => x.Name.Contains("25_0")).GetField("pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenerateTraits), nameof(GenerateTraits.CheckGay)));
                    yield return new CodeInstruction(OpCodes.Brfalse, end);
                }
                yield return code;
                if (code.opcode == OpCodes.Stfld && (FieldInfo)code.operand == AccessTools.FirstInner(typeof(PawnGenerator), x => x.Name.Contains("25_1")).GetField("newTraitDef"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.FirstInner(typeof(PawnGenerator), x => x.Name.Contains("25_0")).GetField("pawn"));
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 11);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.FirstInner(typeof(PawnGenerator), x => x.Name.Contains("25_1")).GetField("newTraitDef"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenerateTraits), nameof(GenerateTraits.FinalCheck)));
                    yield return new CodeInstruction(OpCodes.Brfalse, end);
                }
                //if (code.opcode == OpCodes.Call && (MethodInfo)code.operand == AccessTools.Method(typeof(Rand), "RangeInclusive"))
                if (code == target_GenerateForcedTraits)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.FirstInner(typeof(PawnGenerator), x => x.Name.Contains("25_0")).GetField("pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenerateTraits), nameof(GenerateTraits.GenerateForcedTraits)));
                }
                if (code.opcode == OpCodes.Call && (MethodInfo)code.operand == AccessTools.Method(typeof(Rand), "RangeInclusive"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.FirstInner(typeof(PawnGenerator), x => x.Name.Contains("25_0")).GetField("pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenerateTraits), nameof(GenerateTraits.GenerateAdditionalTraits)));
                }
                if (code == target_End)
                {
                    yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { end } };
                }
            }
        }

        public static void GenerateForcedTraits(Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                foreach (var set in thingDef.raceAddonSettings.characterizationSetting.forcedTraits)
                {
                    if (Rand.Chance(set.chance))
                    {
                        if (set.traitDef == null)
                        {
                            Log.Error("Null forced trait def on " + thingDef.defName, false);
                        }
                        else if (!pawn.story.traits.HasTrait(set.traitDef) && RaceAddon.CheckTrait(pawn.def, new Trait(set.traitDef, set.degree, false)))
                        {
                            pawn.story.traits.GainTrait(new Trait(set.traitDef, set.degree, false));
                        }
                    }
                }
            }
        }

        public static int GenerateAdditionalTraits(int num, Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                foreach (var set in thingDef.raceAddonSettings.characterizationSetting.additionalTraits)
                {
                    if (pawn.story.traits.allTraits.Count < num && Rand.Chance(set.chance))
                    {
                        if (set.traitDef == null)
                        {
                            Log.Error("Null forced trait def on " + thingDef.defName, false);
                        }
                        else if (!pawn.story.traits.HasTrait(set.traitDef) && RaceAddon.CheckTrait(pawn.def, new Trait(set.traitDef, set.degree, false)))
                        {
                            pawn.story.traits.GainTrait(new Trait(set.traitDef, set.degree, false));
                        }
                    }
                }
            }
            return num;
        }

        public static bool CheckGay(Pawn pawn)
        {
            return RaceAddon.CheckTrait(pawn.def, new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay)));
        }

        public static bool FinalCheck(Pawn pawn, Trait trait)
        {
            return RaceAddon.CheckTrait(pawn.def, trait);
        }

        /*
        [HarmonyPrefix]
        public static bool Prefix(Pawn pawn, PawnGenerationRequest request)
        {
            if (pawn.story == null)
            {
                return false;
            }
            if (request.ForcedTraits != null)
            {
                foreach (TraitDef forcedTrait in request.ForcedTraits)
                {
                    Trait trait = new Trait(forcedTrait, 0, true);
                    if (forcedTrait != null && RaceAddon.CheckTrait(pawn.def, trait))
                    {
                        pawn.story.traits.GainTrait(trait);
                    }
                }
            }
            foreach (var backstory in pawn.story.AllBackstories)
            {
                List<TraitEntry> forcedTraits = backstory.forcedTraits;
                if (forcedTraits != null)
                {
                    foreach (var traitEntry in forcedTraits)
                    {
                        if (traitEntry.def == null)
                        {
                            Log.Error("Null forced trait def on " + backstory);
                        }
                        else if ((request.KindDef.disallowedTraits == null || !request.KindDef.disallowedTraits.Contains(traitEntry.def)) && !pawn.story.traits.HasTrait(traitEntry.def) && (request.ProhibitedTraits == null || !request.ProhibitedTraits.Contains(traitEntry.def)))
                        {
                            Trait trait = new Trait(traitEntry.def, traitEntry.degree, false);
                            if (RaceAddon.CheckTrait(pawn.def, trait))
                            {
                                pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree, false));
                            }
                        }
                    }
                }
            }
            RaceAddonThingDef thingDef = pawn.def as RaceAddonThingDef;
            if (thingDef != null)
            {
                foreach (var set in thingDef.raceAddonSettings.characterizationSetting.forcedTraits)
                {
                    if (Rand.Chance(set.chance))
                    {
                        if (set.traitDef == null)
                        {
                            Log.Error("Null forced trait def on " + thingDef.defName, false);
                        }
                        else if (!pawn.story.traits.HasTrait(set.traitDef) && RaceAddon.CheckTrait(pawn.def, new Trait(set.traitDef, set.degree, false)))
                        {
                            pawn.story.traits.GainTrait(new Trait(set.traitDef, set.degree, false));
                        }
                    }
                }
            }
            int traitCount = thingDef != null ? thingDef.raceAddonSettings.characterizationSetting.traitCount.RandomInRange : Rand.RangeInclusive(2, 3);
            if (request.AllowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
            {
                Trait trait = new Trait(TraitDefOf.Gay, PawnGenerator.RandomTraitDegree(TraitDefOf.Gay));
                if (RaceAddon.CheckTrait(pawn.def, trait))
                {
                    pawn.story.traits.GainTrait(trait);
                }
            }
            while (pawn.story.traits.allTraits.Count < traitCount)
            {
                TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
                int degree = PawnGenerator.RandomTraitDegree(newTraitDef);
                Trait result = new Trait(newTraitDef, degree);
                if (RaceAddon.CheckTrait(pawn.def, result))
                {
                    if (pawn.story.traits.HasTrait(newTraitDef) ||
                        (request.KindDef.disallowedTraits != null && request.KindDef.disallowedTraits.Contains(newTraitDef)) ||
                        (request.KindDef.requiredWorkTags != 0 && (newTraitDef.disabledWorkTags & request.KindDef.requiredWorkTags) != 0) ||
                        (newTraitDef == TraitDefOf.Gay && (!request.AllowGay || LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) ||
                        LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))) ||
                        (request.ProhibitedTraits != null && request.ProhibitedTraits.Contains(newTraitDef)) ||
                        (request.Faction != null && Faction.OfPlayerSilentFail != null && request.Faction.HostileTo(Faction.OfPlayer) && !newTraitDef.allowOnHostileSpawn) ||
                        pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) ||
                        (newTraitDef.requiredWorkTypes != null && pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes)) ||
                        pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags) ||
                        (newTraitDef.forcedPassions != null && pawn.workSettings != null && newTraitDef.forcedPassions.Any((SkillDef p) => p.IsDisabled(pawn.story.DisabledWorkTagsBackstoryAndTraits, pawn.GetDisabledWorkTypes(permanentOnly: true)))))
                        continue;
                    if (!pawn.story.AllBackstories.Any(x => x.DisallowsTrait(newTraitDef, degree)))
                    {
                        if (pawn.mindState == null ||
                            pawn.mindState.mentalBreaker == null ||
                            !((pawn.mindState.mentalBreaker.BreakThresholdMinor +
                            result.OffsetOfStat(StatDefOf.MentalBreakThreshold)) *
                            result.MultiplierOfStat(StatDefOf.MentalBreakThreshold) > 0.5f))
                            pawn.story.traits.GainTrait(result);
                    }
                }
            }
            return false;
        }
        */
    }

    [HarmonyPatch(typeof(TraitSet))]
    [HarmonyPatch("GainTrait")]
    public static class GainTrait
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, Trait trait)
        {
            return RaceAddon.CheckTrait(___pawn.def, trait);
        }
    }

    [HarmonyPatch(typeof(Pawn_HealthTracker))]
    [HarmonyPatch("AddHediff", new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) })]
    public static class AddHediff
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn ___pawn, Hediff hediff)
        {
            if (!RaceAddon.CheckHediff(___pawn.def, hediff.def))
            {
                return false;
            }
            if (___pawn.def is RaceAddonThingDef thingDef)
            {
                if (thingDef.raceAddonSettings.characterizationSetting.replacedHediffs != null)
                {
                    var replacedHediff = thingDef.raceAddonSettings.characterizationSetting.replacedHediffs.Find(x => x.originalHediffDef == hediff.def);
                    if (replacedHediff != null)
                    {
                        hediff.def = replacedHediff.replacedHediffDef;
                    }
                }
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(Pawn ___pawn, Hediff hediff)
        {
            if (___pawn.def is RaceAddonThingDef thingDef && ___pawn.GetComp<RaceAddonComp>() is var racomp && racomp.raceAddonGraphicSet != null)
            {
                if (!racomp.raceAddonGraphicSet.bodyDamaged && (hediff.Part != null && hediff.Part.IsInGroup(racomp.torsoDef.linkedBodyGroup)))
                {
                    ResolveAllGraphics.RenewalBodyGraphic(___pawn.Drawer.renderer.graphics, racomp.torsoDef.bodyPath.damaged, racomp.torsoDef.shaderType.Shader, racomp, (___pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.rottingColor);
                    racomp.raceAddonGraphicSet.bodyDamaged = true;
                    //Log.Error("[body] 상처가 생성됨, 몸통 그래픽 damaged로 변경, 현재 bodyDamaged : " + racomp.raceAddonGraphicSet.bodyDamaged);
                }
                if (!racomp.raceAddonGraphicSet.headDamaged && (hediff.Part != null && hediff.Part.IsInGroup(racomp.torsoDef.linkedHeadGroup)))
                {
                    ResolveAllGraphics.RenewalHeadGraphic(___pawn.Drawer.renderer.graphics, racomp.torsoDef.headPath.damaged, racomp.torsoDef.shaderType.Shader, racomp, (___pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.rottingColor);
                    racomp.raceAddonGraphicSet.headDamaged = true;
                    //Log.Error("[head] 상처가 생성됨, 머리 그래픽 damaged로 변경, 현재 headDamaged : " + racomp.raceAddonGraphicSet.headDamaged);
                }
                racomp.raceAddonGraphicSet.upperFaceGraphic?.Update(___pawn.health.hediffSet);
                racomp.raceAddonGraphicSet.lowerFaceGraphic?.Update(___pawn.health.hediffSet);
                racomp.raceAddonGraphicSet.addonGraphics.ForEach(x => x.Update(___pawn.health.hediffSet));
            }
        }
    }

    [HarmonyPatch(typeof(ThoughtUtility))]
    [HarmonyPatch("CanGetThought_NewTemp")]
    public static class CanGetThought_NewTemp
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ThoughtDef def, ref bool __result)
        {
            if (__result)
            {
                __result = RaceAddon.CheckThought(pawn.def, def);
            }
        }
    }

    [HarmonyPatch(typeof(MemoryThoughtHandler))]
    [HarmonyPatch("TryGainMemory", new[] { typeof(Thought_Memory), typeof(Pawn) })]
    public static class TryGainMemory
    {
        [HarmonyPrefix]
        public static bool Prefix(Thought_Memory newThought, Pawn ___pawn)
        {
            if (___pawn.def is RaceAddonThingDef thingDef && thingDef.raceAddonSettings.characterizationSetting.replacedThoughts != null)
            {
                if (thingDef.raceAddonSettings.characterizationSetting.replacedThoughts.Find(x => x.originalThoughtDef == newThought.def) is var replace && replace != null)
                {
                    newThought.def = replace.replacedThoughtDef;
                }
            }
            return true;
        }
    }

    [StaticConstructorOnStartup]
    public static class BuildingDescriptionEditor
    {
        static BuildingDescriptionEditor()
        {
            foreach (var def in RaceAddon.BuildingRestrictions)
            {
                StringBuilder stringBuilder = new StringBuilder("\n\n");
                stringBuilder.AppendLine("RaceAddon_Building".Translate());
                foreach (var thingDef in RaceAddon.AllRaceAddonThingDefs)
                {
                    stringBuilder.AppendLine(thingDef.label);
                }
                def.description += stringBuilder.ToString();
            }
        }
    }

    [HarmonyPatch(typeof(GameRules))]
    [HarmonyPatch("DesignatorAllowed")]
    public static class DesignatorAllowed
    {
        [HarmonyPostfix]
        public static void Postfix(ref Designator d, ref bool __result)
        {
            if (__result && d is Designator_Build target && target.PlacingDef is ThingDef def && RaceAddon.BuildingRestrictions.Contains(def))
            {
                if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddon.CheckBuilding(x.def, def)))
                {
                    d.disabled = true;
                    d.disabledReason = "RaceAddon_Designator".Translate();
                }
            }
        }
    }

    [HarmonyPatch(typeof(BuildCopyCommandUtility))]
    [HarmonyPatch("BuildCopyCommand")]
    public static class BuildCopyCommand
    {
        [HarmonyPostfix]
        public static void Postfix(BuildableDef buildable, ref Command __result)
        {
            if (__result != null && buildable is ThingDef def)
            {
                if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddon.CheckBuilding(x.def, def)))
                {
                    __result.disabled = true;
                    __result.disabledReason = "RaceAddon_Designator".Translate();
                }
            }
        }
    }

    [HarmonyPatch(typeof(GenConstruct))]
    [HarmonyPatch("CanConstruct")]
    public static class HarmonyPatches_CanConstruct
    {
        [HarmonyPostfix]
        public static void Postfix(Thing t, Pawn p, ref bool __result)
        {
            if (__result && t.def.entityDefToBuild is ThingDef def)
            {
                if (!RaceAddon.CheckBuilding(p.def, def))
                {
                    __result = false;
                    JobFailReason.Is("RaceAddon_FloatMenu".Translate(), null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(RaceProperties))]
    [HarmonyPatch("CanEverEat", new[] { typeof(ThingDef) })]
    public static class CanEverEat
    {
        [HarmonyPostfix]
        public static void Postfix(ThingDef t, RaceProperties __instance, ref bool __result)
        {
            if (__result && __instance.Humanlike && DefDatabase<ThingDef>.AllDefsListForReading.First((ThingDef x) => x.race == __instance) is ThingDef thingDef)
            {
                __result = RaceAddon.CheckFood(thingDef, t);
            }
        }
    }

    [HarmonyPatch(typeof(WorkGiver_GrowerSow))]
    [HarmonyPatch("ExtraRequirements")]
    public static class ExtraRequirements
    {
        [HarmonyPostfix]
        public static void Postfix(IPlantToGrowSettable settable, Pawn pawn, ref bool __result)
        {
            if (__result)
            {
                ThingDef plant = WorkGiver_Grower.CalculateWantedPlantDef((settable as Zone_Growing)?.Cells[0] ?? ((Thing)settable).Position, pawn.Map);
                __result = RaceAddon.CheckPlant(pawn.def, plant);
            }
        }
    }

    [HarmonyPatch(typeof(WorkGiver_GrowerHarvest))]
    [HarmonyPatch("HasJobOnCell")]
    public static class HasJobOnCell
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, IntVec3 c, ref bool __result)
        {
            if (__result)
            {
                ThingDef plant = c.GetPlant(map: pawn.Map).def;
                __result = RaceAddon.CheckPlant(pawn.def, plant);
            }
        }
    }

    [HarmonyPatch(typeof(Command_SetPlantToGrow))]
    [HarmonyPatch("ProcessInput")]
    public static class ProcessInput
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var info = AccessTools.Method(typeof(List<FloatMenuOption>), "Add");
            var type = AccessTools.FirstInner(typeof(Command_SetPlantToGrow), x => x.Name.Contains("5_0"));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Callvirt && (MethodInfo)instruction.operand == info)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type, "plantDef"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ProcessInput), "Check"));
                }
                yield return instruction;
            }
        }
        public static FloatMenuOption Check(FloatMenuOption menu, ThingDef plantDef)
        {
            if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddon.CheckPlant(x.def, plantDef)))
            {
                menu.Label = plantDef.LabelCap + " (" + "RaceAddon_FloatMenu".Translate() + ")";
                menu.Disabled = true;
            }
            return menu;
        }
    }

    [HarmonyPatch(typeof(WorkGiver_Tame))]
    [HarmonyPatch("JobOnThing")]
    public static class JobOnThing_A
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing t, ref Job __result)
        {
            if (__result != null && !RaceAddon.CheckAnimal(pawn.def, t.def))
            {
                __result = null;
                JobFailReason.Is("RaceAddon_FloatMenu".Translate(), null);
            }
        }
    }

    [HarmonyPatch(typeof(WorkGiver_Train))]
    [HarmonyPatch("JobOnThing")]
    public static class JobOnThing_B
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, Thing t, ref Job __result)
        {
            if (__result != null && !RaceAddon.CheckAnimal(pawn.def, t.def))
            {
                __result = null;
                JobFailReason.Is("RaceAddon_FloatMenu".Translate(), null);
            }
        }
    }

    [HarmonyPatch(typeof(Bill))]
    [HarmonyPatch("PawnAllowedToStartAnew")]
    public static class HarmonyPatches_PawnAllowedToStartAnew
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, RecipeDef ___recipe, ref bool __result)
        {
            if (__result && !RaceAddon.CheckRecipe(p.def, ___recipe))
            {
                __result = false;
                JobFailReason.Is("RaceAddon_FloatMenu".Translate(), null);
            }
        }
    }

    [HarmonyPatch(typeof(BillUtility))]
    [HarmonyPatch("MakeNewBill")]
    public static class MakeNewBill
    {
        [HarmonyPostfix]
        public static void Postfix(RecipeDef recipe)
        {
            if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => RaceAddon.CheckRecipe(x.def, recipe)))
            {
                string text = "RaceAddon_Recipe".Translate(recipe.LabelCap);
                text += "\n\n";
                foreach (RaceAddonThingDef thingDef in RaceAddon.AllRaceAddonThingDefs.FindAll(x => RaceAddon.CheckRecipe(x, recipe)))
                {
                    text += thingDef.label + "\n";
                }
                Find.WindowStack.Add(new Dialog_MessageBox(text, null, null, null, null, null, false, null, null));
            }
        }
    }

    [HarmonyPatch(typeof(WorkGiver_Researcher))]
    [HarmonyPatch("ShouldSkip")]
    public static class ShouldSkip_Br
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result)
            {
                ResearchProjectDef project = Find.ResearchManager.currentProj;
                if (!RaceAddon.CheckResearch(pawn.def, project))
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(MainTabWindow_Research))]
    [HarmonyPatch("DrawLeftRect")]
    public static class DrawLeftRect
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            var info1 = AccessTools.Field(typeof(SoundDefOf), "ResearchStart");
            foreach (CodeInstruction code in codes)
            {
                if (code.opcode == OpCodes.Ldsfld && (FieldInfo)code.operand == info1)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(MainTabWindow_Research), "selectedProject"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawLeftRect), nameof(DrawLeftRect.NoRaceWarning)));
                }
                yield return code;
            }
        }

        public static void NoRaceWarning(ResearchProjectDef def)
        {
            if (!PawnsFinder.AllMaps_FreeColonistsSpawned.Any((Pawn x) => RaceAddon.CheckResearch(x.def, def)))
            {
                string text = "RaceAddon_Research".Translate(def.label);
                text += "\n\n";
                foreach (var thingDef in RaceAddon.AllRaceAddonThingDefs.FindAll(x => RaceAddon.CheckResearch(x, def)))
                {
                    text += thingDef.label + "\n";
                }
                Find.WindowStack.Add(new Dialog_MessageBox(text, null, null, null, null, null, false, null, null));
            }
        }
    }

    [HarmonyPatch(typeof(WITab_Caravan_Gear))]
    [HarmonyPatch("TryEquipDraggedItem")]
    public static class TryEquipDraggedItem
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn p, ref Thing ___draggedItem, ref bool ___droppedDraggedItem)
        {
            ___droppedDraggedItem = false;
            if (___draggedItem.def.IsApparel && !RaceAddon.CheckApparel(p, ___draggedItem.def))
            {
                Messages.Message("RaceAddon_Caravan".Translate(p.LabelShort), p, MessageTypeDefOf.RejectInput, false);
                ___draggedItem = null;
                return false;
            }
            if (___draggedItem.def.IsWeapon && !RaceAddon.CheckWeapon(p.def, ___draggedItem.def))
            {
                Messages.Message("RaceAddon_Caravan".Translate(p.LabelShort), p, MessageTypeDefOf.RejectInput, false);
                ___draggedItem = null;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FloatMenuUtility))]
    [HarmonyPatch("DecoratePrioritizedTask")]
    public static class DecoratePrioritizedTask
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, LocalTargetInfo target, ref FloatMenuOption __result)
        {
            if (pawn != null && target.Thing != null)
            {
                var thing = target.Thing;
                //if (thing is Apparel apparel && !RaceAddon.CheckApparel(pawn, apparel.def) && __result.Label == "ForceWear".Translate(apparel.LabelShort, apparel))
                if (thing is Apparel apparel && !RaceAddon.CheckApparel(pawn, apparel.def))
                {
                    __result = new FloatMenuOption("CannotWear".Translate(apparel.Label, apparel) + " (" + "RaceAddon_FloatMenu".Translate() + ")", null);
                }
                //if (thing.TryGetComp<CompEquippable>() != null && !RaceAddon.CheckWeapon(pawn.def, thing.def) && __result.Label.Contains("Equip".Translate(thing.LabelShort)))
                if (thing.TryGetComp<CompEquippable>() != null && !RaceAddon.CheckWeapon(pawn.def, thing.def))
                {
                    __result = new FloatMenuOption("CannotEquip".Translate(thing.LabelShort) + " (" + "RaceAddon_FloatMenu".Translate() + ")", null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Learn")]
    public static class Learn
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Ldc_I4_S && (sbyte)code.operand == 20)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "def"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Learn), "GetMaxLevel"));
                }
            }
        }

        public static int GetMaxLevel(int level, SkillDef def, Pawn pawn)
        {
            if ((pawn.def as RaceAddonThingDef)?.raceAddonSettings.characterizationSetting.skillLimits.GetSkillPair(def) is var set && set != null)
            {
                return (int)set.value;
            }
            return level;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("Level", MethodType.Setter)]
    public static class Set_Level
    {
        [HarmonyPrefix]
        public static bool Prefix(int value, Pawn ___pawn, SkillDef ___def, ref int ___levelInt)
        {
            if ((___pawn.def as RaceAddonThingDef)?.raceAddonSettings.characterizationSetting.skillLimits.GetSkillPair(___def) is var set && set != null)
            {
                ___levelInt = Mathf.Clamp(value, 0, (int)set.value);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SkillUI))]
    [HarmonyPatch("DrawSkill", new[] { typeof(SkillRecord), typeof(Rect), typeof(SkillUI.SkillDrawMode), typeof(string) })]
    public static class DrawSkill
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Ldc_R4 && (float)code.operand == 20f)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "def"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DrawSkill), "GetMaxLevel"));
                }
            }
        }

        public static float GetMaxLevel(float level, SkillDef def, Pawn pawn)
        {
            if ((pawn.def as RaceAddonThingDef)?.raceAddonSettings.characterizationSetting.skillLimits.GetSkillPair(def) is var set && set != null)
            {
                return set.value;
            }
            return level;
        }
    }

    [HarmonyPatch(typeof(SkillRecord))]
    [HarmonyPatch("LearnRateFactor")]
    public static class LearnRateFactor
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                yield return code;
                if (code.opcode == OpCodes.Brtrue_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "def"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LearnRateFactor), "GetSkillFactor"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Stloc_0);
                }
            }
        }

        public static float GetSkillFactor(SkillDef def, Pawn pawn)
        {
            if ((pawn.def as RaceAddonThingDef)?.raceAddonSettings.characterizationSetting.skillFactors.GetSkillPair(def, true) is var set && set != null)
            {
                return set.value;
            }
            return 0f;
        }
    }

    [HarmonyPatch(typeof(SkillUI))]
    [HarmonyPatch("GetSkillDescription")]
    public static class GetSkillDescription
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes)
        {
            foreach (var code in codes)
            {
                if (code.opcode == OpCodes.Callvirt && (MethodInfo)code.operand == AccessTools.Method(typeof(SkillRecord), "get_LearningSaturatedToday"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SkillRecord), "pawn"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetSkillDescription), "SetSkillDescription"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                }
                yield return code;
            }
        }

        public static void SetSkillDescription(SkillRecord record, StringBuilder stringBuilder, Pawn pawn)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                string value1 = "20";
                string value2 = "0%";
                if (thingDef.raceAddonSettings.characterizationSetting.skillLimits.GetSkillPair(record.def, true) is var set1 && set1 != null)
                {
                    value1 = set1.value.ToString();
                }
                if (thingDef.raceAddonSettings.characterizationSetting.skillFactors.GetSkillPair(record.def, true) is var set2 && set2 != null)
                {
                    value2 = set2.value.ToStringPercent("F0");
                    if (set2.value >= 0)
                    {
                        value2 = "+" + value2;
                    }
                }
                stringBuilder.AppendLine("\n" + "RaceAddon_Race".Translate() + ": " + pawn.def.label.Translate());
                stringBuilder.AppendLine("RaceAddon_SkillLimit".Translate(value1));
                stringBuilder.Append("RaceAddon_Skill".Translate(value2));
            }
        }
    }
}