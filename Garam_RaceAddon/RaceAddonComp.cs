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
    public class RaceAddonComp : ThingComp
    {
        public string pawnGeneratedVersion = "0.0.0-000000";

        public Pawn Pawn { private set; get; }

        public float drawSizeDeviation = 1.0f;
        public float cachedDrawLocCorrection = 0f;

        public Color skinColor_Main = Color.clear;
        public Color skinColor_Sub = Color.clear;
        public TorsoDef torsoDef;

        public Color faceColor_Main = Color.clear;
        public Color faceColor_Sub = Color.clear;
        public FaceDef upperFaceDef;
        public FaceDef lowerFaceDef;

        public Color hairColor_Sub = Color.clear;

        public List<AddonData> addonDatas = new List<AddonData>();

        public RaceAddonGraphicSet raceAddonGraphicSet;

        public override void CompTick()
        {
            base.CompTick();
            raceAddonGraphicSet?.Update_Normal();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            raceAddonGraphicSet?.Update_Rare();
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            Pawn = parent as Pawn;
            RaceAddon.OldVersionBugFixes(Pawn);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref pawnGeneratedVersion, "pawnGeneratedVersion");

            Scribe_Values.Look(ref drawSizeDeviation, "drawSizeDeviation");

            Scribe_Values.Look(ref skinColor_Main, "skinColor_Main");
            Scribe_Values.Look(ref skinColor_Sub, "skinColor_Sub");
            Scribe_Defs.Look(ref torsoDef, "torsoDef");

            Scribe_Values.Look(ref faceColor_Main, "faceColor_Main");
            Scribe_Values.Look(ref faceColor_Sub, "faceColor_Sub");
            Scribe_Defs.Look(ref upperFaceDef, "upperFaceDef");
            Scribe_Defs.Look(ref lowerFaceDef, "lowerFaceDef");

            Scribe_Values.Look(ref hairColor_Sub, "hairColor_Sub");

            Scribe_Collections.Look(ref addonDatas, "addonDatas", LookMode.Deep);
        }
    }

    public class AddonData : IExposable
    {
        public Color addonColor_Main = Color.clear;
        public Color addonColor_Sub = Color.clear;
        public AddonDef addonDef;

        public void ExposeData()
        {
            Scribe_Values.Look(ref addonColor_Main, "addonColor_Main");
            Scribe_Values.Look(ref addonColor_Sub, "addonColor_Sub");
            Scribe_Defs.Look(ref addonDef, "addonDef");
        }
    }
}
