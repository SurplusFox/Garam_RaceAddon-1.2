using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace Garam_RaceAddon
{
    public class Range : ColorClass
    {
        public override Color GetColor(Pawn pawn, List<string> options)
        {
            List<Pair<Color, float>> list = new List<Pair<Color, float>>();
            foreach (var option in options)
            {
                var arg = option.Replace(" ", "");
                List<float> num = new List<float>();
                foreach (Match match in Regex.Matches(arg, @"(\d{1,}\.\d{1,}|\d{1,})"))
                {
                    num.Add(float.Parse(match.Value));
                }
                if (num.Count() != 7)
                {
                    RaceAddon.Notify("Parsing failed! => " + option, true);
                }
                Color color = new Color(Rand.RangeInclusive((int)num[0], (int)num[3]) / 255f, Rand.RangeInclusive((int)num[1], (int)num[4]) / 255f, Rand.RangeInclusive((int)num[2], (int)num[5]) / 255f);
                list.Add(new Pair<Color, float>(color, num[6]));
            }
            if (list.Count == 0)
            {
                RaceAddon.Notify("Color generation failed! => " + pawn.Name.ToStringFull, true);
                return Color.white;
            }
            return list.RandomElementByWeight(x => x.Second).First;
        }
    }
}
