using RimWorld;
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
    public class Melanin : ColorClass
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
                if (num.Count() != 3)
                {
                    RaceAddon.Notify("Parsing failed! => " + option, true);
                }
                Color color = PawnSkinColors.GetSkinColor(Rand.Range(num[0], num[1]));
                list.Add(new Pair<Color, float>(color, num[2]));
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
