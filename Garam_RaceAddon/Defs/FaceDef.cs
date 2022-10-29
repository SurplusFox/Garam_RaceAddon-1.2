using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class FaceDef : Def
    {
        public ShaderTypeDef shaderType;

        [NoTranslate]
        [GaramTexturePath]
        public string mentalBreakPath;
        [NoTranslate]
        [GaramTexturePath]
        public string aboutToBreakPath;
        [NoTranslate]
        [GaramTexturePath]
        public string onEdgePath;
        [NoTranslate]
        [GaramTexturePath]
        public string stressedPath;
        [NoTranslate]
        [GaramTexturePath]
        public string neutralPath;
        [NoTranslate]
        [GaramTexturePath]
        public string contentPath;
        [NoTranslate]
        [GaramTexturePath]
        public string happyPath;

        [NoTranslate]
        [GaramTexturePath]
        public string sleepingPath;
        [NoTranslate]
        [GaramTexturePath]
        public string painShockPath;
        [NoTranslate]
        [GaramTexturePath]
        public string deadPath;
        [NoTranslate]
        [GaramTexturePath]
        public string blinkPath;
        [NoTranslate]
        [GaramTexturePath]
        public string winkPath;
        [NoTranslate]
        [GaramTexturePath]
        public string draftedPath;
        [NoTranslate]
        [GaramTexturePath]
        public string damagedPath;
        [NoTranslate]
        [GaramTexturePath]
        public string attackingPath;

        public List<CustomPath> customs = new List<CustomPath>();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (shaderType == null)
            {
                shaderType = ShaderTypeDefOf.Cutout;
            }
        }
    }
}