using System.Collections.Generic;
using Verse;

namespace Garam_RaceAddon
{
    public class RestrictionSetting<T>
    {
        public bool allAllow = true;
        public List<T> allAllow_Exceptions = new List<T>();
        public List<T> raceSpecifics = new List<T>();
    }
}