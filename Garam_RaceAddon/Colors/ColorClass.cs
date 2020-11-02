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
    public class ColorClass
    {
        public virtual Color GetColor(Pawn pawn, List<string> options)
        {
            /* 
             * Caution!
             * When finally outputting the color, either specify the transparency value in the range 0-255, or set the color values in the range 0-1.
             * There may be bugs when loading after saving.
             */
            return Color.white;
        }
    }
}
