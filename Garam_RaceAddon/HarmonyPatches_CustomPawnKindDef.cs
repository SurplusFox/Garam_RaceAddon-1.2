using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Garam_RaceAddon
{
    /*
    [HarmonyPatch(typeof(WildManUtility))]
    [HarmonyPatch("IsWildMan")]
    public static class IsWildMan
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result && p.kindDef is SuperPawnKindDef spkd && RaceAddon.IsWildMan(spkd))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(TraderCaravanUtility))]
    [HarmonyPatch("GetTraderCaravanRole")]
    public static class GetTraderCaravanRole
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn p, ref TraderCaravanRole __result)
        {
            if (__result == TraderCaravanRole.Guard && p.kindDef is SuperPawnKindDef spkd && RaceAddon.IsSlave(spkd))
            {
                __result = TraderCaravanRole.Chattel;
            }
        }
    }
    */
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("ChangeKind")]
    public static class ChangeKind
    {
        [HarmonyPriority(int.MaxValue)]
        [HarmonyPrefix]
        public static bool Prefix(Pawn __instance, PawnKindDef newKindDef)
        {
            if (__instance.def is RaceAddonThingDef)
            {
                if (__instance.kindDef != newKindDef)
                {
                    __instance.kindDef = newKindDef;
                    if (__instance.kindDef == PawnKindDefOf.WildMan)
                    {
                        __instance.mindState.WildManEverReachedOutside = false;
                        ReachabilityUtility.ClearCacheFor(__instance);
                    }
                }
            }
            return true;
        }
        /*
        [HarmonyPostfix]
        public static void Postfix(Pawn __instance, PawnKindDef newKindDef)
        {
            if (DefDatabase<SuperPawnKindDef>.AllDefsListForReading.Find(x => x.race == __instance.def && x.pawnKindDefReplacement.targetPawnKindDef == newKindDef) is var spkd && spkd != null)
            {
                __instance.kindDef = spkd;
            }
            return;
        }
        */
    }
    /*
    [HarmonyPatch(typeof(ThinkNode_ConditionalPawnKind))]
    [HarmonyPatch("Satisfied")]
    public static class Satisfied
    {
        [HarmonyPostfix]
        public static void Postfix(Pawn pawn, PawnKindDef ___pawnKind, ref bool __result)
        {
            if (__result == false && ___pawnKind == PawnKindDefOf.WildMan && pawn.kindDef is SuperPawnKindDef spkd && RaceAddon.IsWildMan(spkd))
            {
                __result = true;
            }
        }
    }
    */
}