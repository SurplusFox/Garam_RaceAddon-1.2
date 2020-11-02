using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class CharacterizationSetting
    {
        public List<WorkTags> workDisables = new List<WorkTags>();

        public List<SkillGain> skillGains = new List<SkillGain>();

        public List<SkillGain> skillLimits = new List<SkillGain>();

        public List<TraitInfo> forcedTraits = new List<TraitInfo>();

        public List<HediffWithChance> forcedHediffs = new List<HediffWithChance>();
        public List<ReplacedHediff> replacedHediffs;

        public List<ReplacedThought> replacedThoughts;
    }

    public class TraitInfo
    {
        public TraitDef traitDef;
        public int degree = 0;
        public float chance = 1.0f;
    }

    public class HediffWithChance
    {
        public HediffDef hediffDef;
        public string targetPart;
        public float severity = 0.0f;
        public float chance = 1.0f;
    }

    public class ReplacedHediff
    {
        public HediffDef originalHediffDef;
        public HediffDef replacedHediffDef;
    }

    public class ReplacedThought
    {
        public ThoughtDef originalThoughtDef;
        public ThoughtDef replacedThoughtDef;
    }
}