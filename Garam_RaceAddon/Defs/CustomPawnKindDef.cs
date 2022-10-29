using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class CustomPawnKindDef : Def
    {
        public List<PawnKindDefReplaceSetting> pawnKindDefReplaceSettings = new List<PawnKindDefReplaceSetting>();
        public List<PawnKindDef> onlyUsePawnKindDefBackstories = new List<PawnKindDef>();
    }

    public class PawnKindDefReplaceSetting
    {
        public PawnKindDef originalPawnKindDef;
        public float originalWeight = 10f;
        public PawnKindDef replacedPawnKindDef;
        public float replacedWeight = 10f;
    }
}