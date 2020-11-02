using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGraphicSet))]
    [HarmonyPatch("ResolveApparelGraphics")]
    public static class ResolveApparelGraphics
    {
        [HarmonyPostfix]
        public static void Postfix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                Pawn pawn = __instance.pawn;
                RaceAddonComp racomp = pawn.GetComp<RaceAddonComp>();

                if (pawn.apparel.WornApparel.Find((Apparel x) => x.def.apparel.LastLayer == ApparelLayerDefOf.Overhead) is Apparel apparel)
                {
                    ResolveHatDraw(racomp, thingDef, apparel);
                    ResolveHairDraw(racomp, thingDef, apparel);
                }
            }
        }

        private static void ResolveHatDraw(RaceAddonComp racomp, RaceAddonThingDef thingDef, Apparel apparel)
        {
            if (racomp.raceAddonGraphicSet.presentAgeSetting.hideHat)
            {
                racomp.raceAddonGraphicSet.drawHat = false;
                return;
            }
            if (thingDef.raceAddonSettings.graphicSetting.drawHat)
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawHat_Exceptions.Contains(apparel.def))
                {
                    racomp.raceAddonGraphicSet.drawHat = false;
                }
                else
                {
                    racomp.raceAddonGraphicSet.drawHat = true;
                }
            }
            else
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawHat_Exceptions.Contains(apparel.def))
                {
                    racomp.raceAddonGraphicSet.drawHat = true;
                }
                else
                {
                    racomp.raceAddonGraphicSet.drawHat = false;
                }
            }
        }

        private static void ResolveHairDraw(RaceAddonComp racomp, RaceAddonThingDef thingDef, Apparel apparel)
        {
            if (racomp.raceAddonGraphicSet.drawHat)
            {
                if (thingDef.raceAddonSettings.graphicSetting.drawHair)
                {
                    if (thingDef.raceAddonSettings.graphicSetting.drawHair_Exceptions.Find(x => x.thingDef == apparel.def) is var preset && preset != null)
                    {
                        if (preset.hairOption == 0)
                        {
                            racomp.raceAddonGraphicSet.drawUpperHair = false;
                            racomp.raceAddonGraphicSet.drawLowerHair = false;
                        }
                        else if (preset.hairOption == 1)
                        {
                            racomp.raceAddonGraphicSet.drawUpperHair = false;
                            racomp.raceAddonGraphicSet.drawLowerHair = true;
                        }
                        else if (preset.hairOption == 2)
                        {
                            racomp.raceAddonGraphicSet.drawUpperHair = true;
                            racomp.raceAddonGraphicSet.drawLowerHair = false;
                        }
                    }
                    else
                    {
                        racomp.raceAddonGraphicSet.drawUpperHair = true;
                        racomp.raceAddonGraphicSet.drawLowerHair = true;
                    }
                }
                else
                {
                    if (thingDef.raceAddonSettings.graphicSetting.drawHair_Exceptions.Find(x => x.thingDef == apparel.def) is var preset && preset != null)
                    {
                        if (preset.hairOption == 0)
                        {
                            racomp.raceAddonGraphicSet.drawUpperHair = true;
                            racomp.raceAddonGraphicSet.drawLowerHair = true;
                        }
                        else if (preset.hairOption == 1)
                        {
                            racomp.raceAddonGraphicSet.drawUpperHair = true;
                            racomp.raceAddonGraphicSet.drawLowerHair = false;
                        }
                        else if (preset.hairOption == 2)
                        {
                            racomp.raceAddonGraphicSet.drawUpperHair = false;
                            racomp.raceAddonGraphicSet.drawLowerHair = true;
                        }
                    }
                    else
                    {
                        racomp.raceAddonGraphicSet.drawUpperHair = false;
                        racomp.raceAddonGraphicSet.drawLowerHair = false;
                    }
                }
            }
            else
            {
                racomp.raceAddonGraphicSet.drawUpperHair = true;
                racomp.raceAddonGraphicSet.drawLowerHair = true;
            }
        }
    }
}