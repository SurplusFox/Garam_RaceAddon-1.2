using RimWorld;
using System.Collections.Generic;
using System.Xml;
using Verse;

namespace Garam_RaceAddon
{
    public class CharacterizationSetting
    {
        public List<WorkTags> workDisables = new List<WorkTags>();

        public List<SkillPair> skillGains = new List<SkillPair>();

        public List<SkillPair> skillLimits = new List<SkillPair>();

        public List<SkillPair> skillFactors = new List<SkillPair>();

        public List<TraitInfo> forcedTraits = new List<TraitInfo>();

        public List<TraitInfo> additionalTraits = new List<TraitInfo>();

        public List<HediffWithChance> forcedHediffs = new List<HediffWithChance>();
        public List<ReplacedHediff> replacedHediffs;

        public List<ReplacedThought> replacedThoughts;
    }

    public class SkillPair
    {
        public SkillDef skill;
        public float value;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured SkillPair: " + xmlRoot.OuterXml);
                return;
            }
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
            value = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
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