using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using Verse;

namespace Garam_RaceAddon
{
    public class AgeSetting
    {
        public Verse.Gender gender = Verse.Gender.None;
        public int minAge = 0;
        //public SimpleBackstoryDef ageBackstory;
        public DrawSize drawSize = new DrawSize();
        public bool hideHat = false;
        public bool keepAppearance = true;
        public List<AppearanceInfo> appearances = new List<AppearanceInfo>();
        public List<ThingDef> allowedApparels;
    }

    public class DrawSize
    {
        public Vector2 head = new Vector2(1.0f, 1.0f);
        public Vector2 body = new Vector2(1.0f, 1.0f);
        //public Vector2 equipment = new Vector2(1.0f, 1.0f);
    }

    public class AppearanceInfo
    {
        public Verse.Gender gender = Verse.Gender.None;
        public AppearanceDef appearanceDef;
        public float weight = 1.0f;
    }
}