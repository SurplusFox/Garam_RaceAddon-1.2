using RimWorld;
using Verse;

namespace Garam_RaceAddon
{
    public class ImprovedHairDef : HairDef
    {
        public ShaderTypeDef shaderType;

        [NoTranslate]
        [GaramTexturePath]
        public string lowerPath;

        public bool drawnInBed = false;

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
