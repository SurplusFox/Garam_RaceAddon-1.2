using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Garam_RaceAddon
{
    public class SimpleBackstoryDef : Def
    {
        public BackstoryInfo backstoryInfo;
        public List<string> spawnCategories = new List<string>();

        public List<WorkTags> workDisables = new List<WorkTags>();
        public List<WorkTags> requiredWorkTags = new List<WorkTags>();

        public List<SkillGain> skillGains = new List<SkillGain>();

        public List<TraitInfo> forcedTraits = new List<TraitInfo>();
        public List<TraitInfo> disallowedTraits = new List<TraitInfo>();

        public List<HediffWithChance> forcedHediffs = new List<HediffWithChance>();

        public IntRange bioAgeRange = new IntRange(-1, -1);
        public IntRange chronoAgeRange = new IntRange(-1, -1);

        public float maleChance = -1f;
        public List<AppearanceInfo> appearances;

        public Backstory Backstory { get; private set; }

        public override void ResolveReferences()
        {
            base.ResolveReferences();

            WorkTags workDisables = WorkTags.None;
            if (this.workDisables != null)
            {
                this.workDisables.ForEach(x => workDisables |= x);
            }

            WorkTags requiredWorkTags = WorkTags.None;
            if (this.requiredWorkTags != null)
            {
                this.requiredWorkTags.ForEach(x => requiredWorkTags |= x);
            }

            List<TraitEntry> forcedTraits = new List<TraitEntry>();
            if (this.forcedTraits != null)
            {
                this.forcedTraits.ForEach(x => forcedTraits.Add(new TraitEntry(x.traitDef, x.degree)));
            }

            List<TraitEntry> disallowedTraits = new List<TraitEntry>();
            if (this.disallowedTraits != null)
            {
                this.disallowedTraits.ForEach(x => disallowedTraits.Add(new TraitEntry(x.traitDef, x.degree)));
            }

            Dictionary<SkillDef, int> skillGainsResolved = new Dictionary<SkillDef, int>();
            if (skillGains != null)
            {
                skillGains.ForEach(x => skillGainsResolved.Add(x.skill, x.xp));
            }

            Backstory = new Backstory
            {
                identifier = defName,

                slot = backstoryInfo.slot,
                title = backstoryInfo.title,
                titleShort = backstoryInfo.titleShort,
                baseDesc = backstoryInfo.description,
                spawnCategories = spawnCategories,

                workDisables = workDisables,
                requiredWorkTags = requiredWorkTags,
                forcedTraits = forcedTraits,
                disallowedTraits = disallowedTraits,
                skillGainsResolved = skillGainsResolved
            };
            Traverse.Create(Backstory).Field("bodyTypeMaleResolved").SetValue(BodyTypeDefOf.Male);
            Traverse.Create(Backstory).Field("bodyTypeFemaleResolved").SetValue(BodyTypeDefOf.Female);
            Backstory.ResolveReferences();
            Backstory.PostLoad();
            Backstory.identifier = defName;

            IEnumerable<string> errors = Backstory.ConfigErrors(false);
            if (errors.Any())
            {
                RaceAddon.Notify("BackstoryDef has errors! => " + defName + string.Join("\n", errors.ToArray()), true);
            }
            else
            {
                BackstoryDatabase.AddBackstory(Backstory);
            }
        }
    }

    public class BackstoryInfo
    {
        public BackstorySlot slot;
        public string title;
        public string titleShort;
        public string description;
        public List<SimpleBackstoryDef> requiredBackstories;
    }
}