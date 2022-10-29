using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class RelationSetting
    {
        public bool randomRelationAllow = true;
        public List<ThingDef> sameRaces = new List<ThingDef>();
        public Non_TragetRace sameRace = new Non_TragetRace();
        public Non_TragetRace otherRace = new Non_TragetRace();
        public List<TragetRace> specialRaces = new List<TragetRace>();
    }

    public class Non_TragetRace
    {
        public float compatibilityBonus = 0f;
        public LoveChanceFactor loveChanceFactor_Male;
        public LoveChanceFactor loveChanceFactor_Female;
    }

    public class TragetRace
    {
        public ThingDef raceDef;
        public float compatibilityBonus = 0f;
        public LoveChanceFactor loveChanceFactor_Male;
        public LoveChanceFactor loveChanceFactor_Female;
    }

    public class LoveChanceFactor
    {
        public float minAgeValue = 0.15f;
        public int minAgeDifference = -10;
        public int lowerAgeDifference = -4;
        public int upperAgeDifference = 4;
        public int maxAgeDifference = 10;
        public float maxAgeValue = 0.15f;
        public float finalCorrectionValue = 0f;
    }
}