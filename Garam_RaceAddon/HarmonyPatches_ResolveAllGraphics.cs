using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnGraphicSet))]
    [HarmonyPatch("ResolveAllGraphics")]
    public static class ResolveAllGraphics
    {
        [HarmonyPrefix]
        public static bool Prefix(PawnGraphicSet __instance)
        {
            if (__instance.pawn.def is RaceAddonThingDef thingDef)
            {
                Pawn pawn = __instance.pawn;
                RaceAddonComp racomp = pawn.GetComp<RaceAddonComp>();
                if (racomp.raceAddonGraphicSet == null)
                {
                    racomp.raceAddonGraphicSet = new RaceAddonGraphicSet(pawn, racomp);
                }
                TorsoDef torsoDef = racomp.torsoDef;
                __instance.ClearCache();
                RenewalBodyGraphic(__instance, torsoDef.bodyPath.normal, torsoDef.shaderType.Shader, racomp, thingDef.raceAddonSettings.graphicSetting.rottingColor);
                RenewalHeadGraphic(__instance, torsoDef.headPath.normal, torsoDef.shaderType.Shader, racomp, thingDef.raceAddonSettings.graphicSetting.rottingColor);
                __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(torsoDef.bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
                __instance.skullGraphic = GraphicDatabase.Get<Graphic_Multi>(torsoDef.skullPath, ShaderDatabase.Cutout);
                __instance.headStumpGraphic = GraphicDatabase.Get<Graphic_Multi>(torsoDef.stumpPath, torsoDef.shaderType.Shader, Vector2.one, racomp.skinColor_Main, racomp.skinColor_Sub);
                __instance.desiccatedHeadStumpGraphic = GraphicDatabase.Get<Graphic_Multi>(torsoDef.stumpPath, torsoDef.shaderType.Shader, Vector2.one, thingDef.raceAddonSettings.graphicSetting.rottingColor);
                if (pawn.story.hairDef is ImprovedHairDef hairDef)
                {
                    __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(hairDef.texPath, hairDef.shaderType.Shader, Vector2.one, pawn.story.hairColor, racomp.hairColor_Sub);
                    racomp.raceAddonGraphicSet.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(hairDef.lowerPath, hairDef.shaderType.Shader, Vector2.one, pawn.story.hairColor, racomp.hairColor_Sub);
                }
                else
                {
                    __instance.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
                    racomp.raceAddonGraphicSet.hairGraphic = null;
                }
                __instance.ResolveApparelGraphics();
                return false;
            }
            return true;
        }

        public static void RenewalBodyGraphic(PawnGraphicSet pawnGraphicSet, string bodyPath, Shader shader, RaceAddonComp racomp, Color rottingColor)
        {
            pawnGraphicSet.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(bodyPath, shader, Vector2.one, racomp.skinColor_Main, racomp.skinColor_Sub);
            pawnGraphicSet.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(bodyPath, shader, Vector2.one, rottingColor);
            Traverse.Create(pawnGraphicSet).Field<int>("cachedMatsBodyBaseHash").Value = -1;
        }

        public static void RenewalHeadGraphic(PawnGraphicSet pawnGraphicSet, string headPath, Shader shader, RaceAddonComp racomp, Color rottingColor)
        {
            pawnGraphicSet.headGraphic = GraphicDatabase.Get<Graphic_Multi>(headPath, shader, Vector2.one, racomp.skinColor_Main, racomp.skinColor_Sub);
            pawnGraphicSet.desiccatedHeadGraphic = GraphicDatabase.Get<Graphic_Multi>(headPath, shader, Vector2.one, rottingColor);
        }
    }
}