using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public class GraphicSetting
    {
        //Basic
        public Color rottingColor = new Color(0.34f, 0.32f, 0.3f);
        public bool drawWound = true;
        //Animation
        public bool eyeBlink = false;
        public bool fixedUpperFace = false;
        public bool fixedLowerFace = false;
        public bool headAnimation = false;
        public bool headTargeting = false;
        //Draw
        public bool drawHat = true;
        public List<ThingDef> drawHat_Exceptions = new List<ThingDef>();
        public bool drawHair = true;
        public List<DrawHairOption> drawHair_Exceptions = new List<DrawHairOption>();
        public SimpleCurve drawSizeCurve;
        public bool footPositionCorrection_DrawSize = false;
        public bool footPositionCorrection_DrawSizeCurve = true;
    }

    public class DrawHairOption
    {
        public ThingDef thingDef;
        public int hairOption = 0;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            hairOption = int.Parse(xmlRoot.FirstChild.Value);
        }
    }
}