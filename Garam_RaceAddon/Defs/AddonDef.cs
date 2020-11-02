using Verse;

namespace Garam_RaceAddon
{
    public class AddonDef : Def
    {
        public ShaderTypeDef shaderType;

        public UniversalPath addonPath;

        [NoTranslate]
        public string linkedBodyPart = "None";
        public bool drawnInBed = false;
        public bool rotting = true;

        public bool drawingToBody = true;
        public DrawPriority drawPriority = new DrawPriority();
        public Rot4ToVector2 offsets = new Rot4ToVector2();
    }

    public class DrawPriority
    {
        public int north = 0;
        public int south = 0;
        public int west = 0;
        public int east = 0;

        public int GetPriority(Rot4 rot)
        {
            if (rot == Rot4.North)
            {
                return north;
            }
            else if (rot == Rot4.South)
            {
                return south;
            }
            else if (rot == Rot4.West)
            {
                return west;
            }
            else
            {
                return east;
            }
        }
    }
}