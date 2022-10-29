using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace Garam_RaceAddon
{
    public class Synchronization : ColorClass
    {
        public override Color GetColor(Pawn pawn, List<string> options)
        {
            if (options[0] == "hairColor_Main")
            {
                return pawn.story.hairColor;
            }
            return Traverse.Create(pawn.GetComp<RaceAddonComp>()).Field(options[0]).GetValue<Color>();
        }
    }
}
