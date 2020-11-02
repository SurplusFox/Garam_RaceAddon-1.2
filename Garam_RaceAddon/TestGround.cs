using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(Selector))]
    [HarmonyPatch("Select")]
    public static class TestGround
    {
        [HarmonyPrefix]
        public static bool Prefix(object obj)
        {
            if (obj is Pawn pawn && pawn.RaceProps.Humanlike && pawn.def is RaceAddonThingDef thingDef && pawn.GetComp<RaceAddonComp>() is var racomp)
            {
                //pawn.health.AddHediff(HediffDefOf.MissingBodyPart, "tail".GetBodyPartRecord(pawn.RaceProps.body));
                //pawn.GetComp<RaceAddonComp>().raceAddonGraphicSet.eyeBlinker.ForcedExecution();
            }
            return true;
        }

        [HarmonyPostfix]
        public static void Postfix(object obj)
        {
            if (obj is Pawn pawn && pawn.RaceProps.Humanlike && pawn.def is RaceAddonThingDef thingDef && pawn.GetComp<RaceAddonComp>() is var racomp)
            {

            }
        }
    }
}
