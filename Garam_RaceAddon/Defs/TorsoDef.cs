using RimWorld;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public class TorsoDef : Def
    {
        public ShaderTypeDef shaderType;

        public BodyPartGroupDef linkedBodyGroup;
        public BodyPartGroupDef linkedHeadGroup;
        //public BodyPartGroupDef linkedBodyGroup = BodyPartGroupDefOf.Torso;
        //public BodyPartGroupDef linkedHeadGroup = BodyPartGroupDefOf.FullHead;

        public UniversalPath bodyPath = new UniversalPath();
        public BodyTypeDef bodyType;

        public UniversalPath headPath = new UniversalPath();
        [NoTranslate]
        [GaramTexturePath]
        public string skullPath;
        [NoTranslate]
        [GaramTexturePath]
        public string stumpPath;
        public CrownType crownType = CrownType.Average;

        public Rot4ToVector2 headOffsetsCorrection = new Rot4ToVector2();
        public HeadTargetingOffset headTargetingOffsets = new HeadTargetingOffset();

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            if (shaderType == null)
            {
                shaderType = ShaderTypeDefOf.Cutout;
            }
        }
    }

    public class HeadTargetingOffset
    {
        public Rot4ToVector2 body_south = new Rot4ToVector2();
        public Rot4ToVector2 body_north = new Rot4ToVector2();
        public Rot4ToVector2 body_west = new Rot4ToVector2();
        public Rot4ToVector2 body_east = new Rot4ToVector2();

        public void Correction(ref Vector3 headLoc, Rot4 bodyFacing, Rot4 headFacing)
        {
            if (bodyFacing == Rot4.South)
            {
                headLoc.x += headFacing == Rot4.South ? body_south.south.x : headFacing == Rot4.East ? body_south.east.x : headFacing == Rot4.West ? body_south.west.x : body_south.north.x;
                headLoc.z += headFacing == Rot4.South ? body_south.south.y : headFacing == Rot4.East ? body_south.east.y : headFacing == Rot4.West ? body_south.west.y : body_south.north.y;
            }
            else if (bodyFacing == Rot4.East)
            {
                headLoc.x += headFacing == Rot4.South ? body_east.south.x : headFacing == Rot4.East ? body_east.east.x : headFacing == Rot4.West ? body_east.west.x : body_east.north.x;
                headLoc.z += headFacing == Rot4.South ? body_east.south.y : headFacing == Rot4.East ? body_east.east.y : headFacing == Rot4.West ? body_east.west.y : body_east.north.y;
            }
            else if (bodyFacing == Rot4.West)
            {
                headLoc.x += headFacing == Rot4.South ? body_west.south.x : headFacing == Rot4.East ? body_west.east.x : headFacing == Rot4.West ? body_west.west.x : body_west.north.x;
                headLoc.z += headFacing == Rot4.South ? body_west.south.y : headFacing == Rot4.East ? body_west.east.y : headFacing == Rot4.West ? body_west.west.y : body_west.north.y;
            }
            else
            {
                headLoc.x += headFacing == Rot4.South ? body_north.south.x : headFacing == Rot4.East ? body_north.east.x : headFacing == Rot4.West ? body_north.west.x : body_north.north.x;
                headLoc.z += headFacing == Rot4.South ? body_north.south.y : headFacing == Rot4.East ? body_north.east.y : headFacing == Rot4.West ? body_north.west.y : body_north.north.y;
            }
        }
    }

    public class Rot4ToVector2
    {
        public Vector2 north = new Vector3(0f, 0f);
        public Vector2 south = new Vector3(0f, 0f);
        public Vector2 west = new Vector3(0f, 0f);
        public Vector2 east = new Vector3(0f, 0f);

        public Vector3 GetLoc(Rot4 rot)
        {
            if (rot == Rot4.North)
            {
                return new Vector3(north.x, 0f, north.y);
            }
            else if (rot == Rot4.South)
            {
                return new Vector3(south.x, 0f, south.y);
            }
            else if (rot == Rot4.West)
            {
                return new Vector3(west.x, 0f, west.y);
            }
            else
            {
                return new Vector3(east.x, 0f, east.y);
            }
        }
    }
}