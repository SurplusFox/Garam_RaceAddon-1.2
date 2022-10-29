using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class AppearanceDef : Def
    {
        public List<ColorSet> colorList = new List<ColorSet>();
        public List<TorsoSet> torsoList = new List<TorsoSet>();
        public List<FaceSet> faceList = new List<FaceSet>();
        public List<HairSet> hairList = new List<HairSet>();
        public List<AddonSet> addonList = new List<AddonSet>();
    }

    public class ColorSet
    {
        [NoTranslate]
        public string name;
        public Type colorClass;
        public List<string> options = new List<string>();
    }

    public class TorsoSet
    {
        public float weight = 1.0f;
        [NoTranslate]
        public string skinColor_Main;
        [NoTranslate]
        public string skinColor_Sub;
        public TorsoDef torsoDef;
    }

    public class FaceSet
    {
        public float weight = 1.0f;
        [NoTranslate]
        public string faceColor_Main;
        [NoTranslate]
        public string faceColor_Sub;
        public FaceDef upperFaceDef;
        public FaceDef lowerFaceDef;
    }

    public class HairSet
    {
        public float weight = 1.0f;
        [NoTranslate]
        public string hairColor_Main;
        [NoTranslate]
        public string hairColor_Sub;
        [NoTranslate]
        public List<string> hairTags = new List<string>();
    }

    public class AddonSet
    {
        public float weight = 1.0f;
        public List<AddonInfo> addons = new List<AddonInfo>();
    }

    public class AddonInfo
    {
        [NoTranslate]
        public string addonColor_Main;
        [NoTranslate]
        public string addonColor_Sub;
        public AddonDef addonDef;
    }

    public class UniversalPath
    {
        [NoTranslate]
        [GaramTexturePath]
        public string normal;
        [NoTranslate]
        [GaramTexturePath]
        public string damaged;
        public List<CustomPath> customs = new List<CustomPath>();
    }

    public class CustomPath
    {
        public HediffDef hediffDef;
        [NoTranslate]
        [GaramTexturePath]
        public string path;
        public int priority = 0;
    }
}