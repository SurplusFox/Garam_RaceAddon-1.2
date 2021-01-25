using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Garam_RaceAddon
{
    [HarmonyPatch(typeof(PawnRenderer))]
    [HarmonyPatch("RenderPawnInternal", new[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool) })]
    public static class RenderPawnInternal
    {
		[HarmonyPriority(int.MinValue)]
        [HarmonyPrefix]
        public static bool Prefix(PawnRenderer __instance, Pawn ___pawn, PawnWoundDrawer ___woundOverlays, PawnHeadOverlays ___statusOverlays,
            Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump, bool invisible)
		{
			if (___pawn.def is RaceAddonThingDef thingDef)
			{
				if (!__instance.graphics.AllResolved)
				{
					__instance.graphics.ResolveAllGraphics();
				}
				RaceAddonComp racomp = ___pawn.GetComp<RaceAddonComp>();
				Quaternion bodyQuat = Quaternion.AngleAxis(angle, Vector3.up);
				Quaternion headQuat = bodyQuat;
				rootLoc.z -= racomp.cachedDrawLocCorrection;
				Vector3 bodyLoc = rootLoc;
				Vector3 headLoc = __instance.BaseHeadOffsetAt(bodyFacing);
				GetModifiedValue(racomp, ref bodyQuat, ref headQuat, ref bodyLoc, ref headLoc, ___pawn, ref renderBody, ref bodyFacing, ref headFacing, portrait);
				// For Resolve Head Targeting Error
				Mesh bodyMesh = racomp.raceAddonGraphicSet.bodyMeshSet.MeshAt(bodyFacing);
				Mesh headMesh = racomp.raceAddonGraphicSet.headMeshSet.MeshAt(headFacing);

				List<ApparelGraphicRecord> apparelGraphics = __instance.graphics.apparelGraphics;

				if (renderBody)
				{
					List<Material> bodyMatList = __instance.graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
					for (int i = 0; i < bodyMatList.Count; i++)
					{
						// Draw Body And Apparel
						Material mat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, bodyMatList[i], ___pawn, portrait);
						GenDraw.DrawMeshNowOrLater(bodyMesh, bodyLoc.SetLayer((i + 3) * 10), bodyQuat, mat, portrait);
					}
					if (bodyDrawType == RotDrawMode.Fresh && thingDef.raceAddonSettings.graphicSetting.drawWound)
					{
						// Draw Wound
						___woundOverlays.RenderOverBody(bodyLoc.SetLayer(60), bodyMesh, bodyQuat, portrait);
					}
					for (int k = 0; k < apparelGraphics.Count; k++)
					{
						// Draw Shell
						if (apparelGraphics[k].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell && !apparelGraphics[k].sourceApparel.def.apparel.shellRenderedBehindHead)
						{
							Material original3 = apparelGraphics[k].graphic.MatAt(bodyFacing);
							original3 = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, original3, ___pawn, portrait);
							GenDraw.DrawMeshNowOrLater(bodyMesh, bodyLoc.SetLayer(bodyFacing == Rot4.North ? 80 : 70), bodyQuat, original3, portrait);
						}
						// Draw Pack
						if (PawnRenderer.RenderAsPack(apparelGraphics[k].sourceApparel))
						{
							Material original4 = apparelGraphics[k].graphic.MatAt(bodyFacing);
							original4 = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, original4, ___pawn, portrait);
							if (apparelGraphics[k].sourceApparel.def.apparel.wornGraphicData != null)
							{
								Vector2 vector3 = apparelGraphics[k].sourceApparel.def.apparel.wornGraphicData.BeltOffsetAt(bodyFacing, ___pawn.story.bodyType);
								Vector2 vector4 = apparelGraphics[k].sourceApparel.def.apparel.wornGraphicData.BeltScaleAt(___pawn.story.bodyType);
								Matrix4x4 matrix = Matrix4x4.Translate(bodyLoc.SetLayer(bodyFacing == Rot4.South ? 10 : 90)) * Matrix4x4.Rotate(bodyQuat) * Matrix4x4.Translate(new Vector3(vector3.x, 0f, vector3.y)) * Matrix4x4.Scale(new Vector3(vector4.x, 1f, vector4.y));
								GenDraw.DrawMeshNowOrLater_NewTemp(bodyMesh, matrix, original4, portrait);
							}
							else
							{
								GenDraw.DrawMeshNowOrLater(bodyMesh, bodyLoc.SetLayer(bodyFacing == Rot4.North ? 80 : 70), bodyQuat, original4, portrait);
							}
						}
					}
					//Draw Body Addons
					if (bodyDrawType != RotDrawMode.Dessicated && racomp.raceAddonGraphicSet.addonGraphics.FindAll(x => x.data.addonDef.drawingToBody) is var list && list.Count > 0)
					{
						foreach (var record in list)
						{
							if (!___pawn.InBed() || ___pawn.InBed() && record.data.addonDef.drawnInBed)
							{
								Material addonMat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, record.MatAt(bodyFacing, bodyDrawType), ___pawn, portrait);
								if (addonMat != null)
								{
									Vector3 offset = record.data.addonDef.offsets.GetLoc(bodyFacing);
									GenDraw.DrawMeshNowOrLater(bodyMesh, offset + bodyLoc.SetLayer(record.data.addonDef.drawPriority.GetPriority(bodyFacing)), bodyQuat, addonMat, portrait);
								}
							}
						}
					}
				}
				if (__instance.graphics.headGraphic != null)
				{
					// Draw Head
					if (__instance.graphics.HeadMatAt_NewTemp(headFacing, bodyDrawType, headStump, portrait) is var headMat && headMat != null)
					{
						GenDraw.DrawMeshNowOrLater(headMesh, headLoc.SetLayer(headFacing == Rot4.North ? 70 : 80), headQuat, headMat, portrait);
					}
					// Draw Hat, Mask
					bool hideHair = false;
					if (!portrait || !Prefs.HatsOnlyOnMap)
					{
						for (int j = 0; j < apparelGraphics.Count; j++)
						{
							if (apparelGraphics[j].sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
							{
								hideHair = !apparelGraphics[j].sourceApparel.def.apparel.hatRenderedFrontOfFace;
								Material mat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, apparelGraphics[j].graphic.MatAt(headFacing), ___pawn, portrait);
								GenDraw.DrawMeshNowOrLater(headMesh, headLoc.SetLayer(hideHair ? 120 : (bodyFacing == Rot4.North) ? 10 : 110), headQuat, mat, portrait);
							}
						}
					}
					if (bodyDrawType != RotDrawMode.Dessicated && !headStump)
					{
						bool blinkNow = false;
						bool winkNow = false;
						if (racomp.raceAddonGraphicSet.eyeBlinker != null)
						{
							blinkNow = racomp.raceAddonGraphicSet.eyeBlinker.BlinkNow;
							winkNow = racomp.raceAddonGraphicSet.eyeBlinker.WinkNow;
						}
						// Draw Upper Face
						if (racomp.raceAddonGraphicSet.upperFaceGraphic != null)
						{
							Material faceMat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, racomp.raceAddonGraphicSet.upperFaceGraphic.MatAt(headFacing, portrait, blinkNow, winkNow), ___pawn);
							GenDraw.DrawMeshNowOrLater(headMesh, headLoc.SetLayer(105), headQuat, faceMat, portrait);
						}
						// Draw Lower Face
						if (racomp.raceAddonGraphicSet.lowerFaceGraphic != null)
						{
							Material faceMat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, racomp.raceAddonGraphicSet.lowerFaceGraphic.MatAt(headFacing, portrait, blinkNow, winkNow), ___pawn);
							GenDraw.DrawMeshNowOrLater(headMesh, headLoc.SetLayer(85), headQuat, faceMat, portrait);
						}
						// Draw Upper Hair
						if (!hideHair || racomp.raceAddonGraphicSet.drawUpperHair)
						{
							GenDraw.DrawMeshNowOrLater(headMesh, headLoc.SetLayer(100), headQuat, __instance.graphics.HairMatAt_NewTemp(headFacing, portrait), portrait);
						}
						// Draw Lower Hair
						if (racomp.raceAddonGraphicSet.hairGraphic != null && (!hideHair || racomp.raceAddonGraphicSet.drawLowerHair))
						{
							if (renderBody || ___pawn.InBed() && (___pawn.story.hairDef as ImprovedHairDef).drawnInBed)
							{
								Material mat = racomp.raceAddonGraphicSet.hairGraphic.MatAt(headFacing);
								mat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, mat, ___pawn, portrait);
								GenDraw.DrawMeshNowOrLater(headMesh, headLoc.SetLayer(20), headQuat, mat, portrait);
							}
						}
						// Draw Head Addons
						if (racomp.raceAddonGraphicSet.addonGraphics.FindAll(x => !x.data.addonDef.drawingToBody) is var list && list.Count > 0)
						{
							foreach (var record in list)
							{
								if (!___pawn.InBed() || ___pawn.InBed() && record.data.addonDef.drawnInBed)
								{
									Material addonMat = OverrideMaterialIfNeeded_NewTemp(__instance.graphics, record.MatAt(headFacing, bodyDrawType), ___pawn, portrait);
									if (addonMat != null)
									{
										Vector3 offset = record.data.addonDef.offsets.GetLoc(bodyFacing);
										GenDraw.DrawMeshNowOrLater(headMesh, offset + headLoc.SetLayer(record.data.addonDef.drawPriority.GetPriority(headFacing)), headQuat, addonMat, portrait);
									}
								}
							}
						}
					}
				}
				/*
				if (!portrait)
				{
					DrawEquipment(rootLoc, ___pawn, racomp.raceAddonGraphicSet.equipmentMeshSet.MeshAt(bodyFacing));
					if (___pawn.apparel != null)
					{
						List<Apparel> wornApparel = ___pawn.apparel.WornApparel;
						for (int l = 0; l < wornApparel.Count; l++)
						{
							wornApparel[l].DrawWornExtras();
						}
					}
					___statusOverlays.RenderStatusOverlays(bodyLoc.SetLayer(130), bodyQuat, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
				}
				return false;
				*/
			}
			return true;
		}

		[HarmonyPriority(int.MaxValue)]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codes, ILGenerator il)
		{
			var target_Start = codes.ToList().FindAll(x => x.opcode == OpCodes.Brtrue_S).Last();
			Label start = il.DefineLabel();
			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PawnRenderer), "pawn"));
			yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RenderPawnInternal), nameof(RenderPawnInternal.CheckJumping)));
			yield return new CodeInstruction(OpCodes.Brtrue, start);
			foreach (var code in codes)
            {
				yield return code;
				if (code == target_Start)
				{
					yield return new CodeInstruction(OpCodes.Nop) { labels = new List<Label> { start } };
				}
            }
		}

		private static Material OverrideMaterialIfNeeded_NewTemp(PawnGraphicSet graphics, Material original, Pawn pawn, bool portrait = false)
		{
			Material baseMat = (!portrait && pawn.IsInvisible()) ? InvisibilityMatPool.GetInvisibleMat(original) : original;
			return graphics.flasher.GetDamagedMat(baseMat);
		}

		private static void GetModifiedValue(RaceAddonComp racomp, ref Quaternion bodyQuat, ref Quaternion headQuat, ref Vector3 bodyLoc, ref Vector3 headLoc,
			Pawn pawn, ref bool renderBody, ref Rot4 bodyFacing, ref Rot4 headFacing, bool portrait)
		{
			bool headAnimation = false;
			if (!portrait && pawn.Awake())
			{
				if (racomp.raceAddonGraphicSet.headRotator != null && !pawn.Drafted)
				{
					headQuat *= racomp.raceAddonGraphicSet.headRotator.GetQuat();
				}
				if (racomp.raceAddonGraphicSet.headTargeter != null && !pawn.Downed)
				{
					var initialRot = headFacing;
					headFacing.Rotate(racomp.raceAddonGraphicSet.headTargeter.RotDirection);
					if (initialRot != headFacing)
					{
						headAnimation = true;
					}
				}
			}
			if (!portrait && headAnimation)
			{
				racomp.torsoDef.headTargetingOffsets.Correction(ref headLoc, bodyFacing, headFacing);
			}
			if (portrait || renderBody)
			{
				headLoc.x *= racomp.raceAddonGraphicSet.presentAgeSetting.drawSize.head.x;
				headLoc.z *= racomp.raceAddonGraphicSet.presentAgeSetting.drawSize.head.y;
			}
			headLoc = bodyLoc + (bodyQuat * headLoc);
		}

		private static Vector3 SetLayer(this Vector3 loc, int priority)
        {
			loc.y += 0.003f * (priority / 10f);
			return loc;
		}

		private static bool CheckJumping(Pawn pawn)
        {
			return pawn.def is RaceAddonThingDef;
        }

		/*
		private static void DrawEquipment(Vector3 rootLoc, Pawn pawn, Mesh mesh)
		{
			if (pawn.Dead || !pawn.Spawned || pawn.equipment == null || pawn.equipment.Primary == null || (pawn.CurJob != null && pawn.CurJob.def.neverShowWeapon))
			{
				return;
			}
            if (pawn.stances.curStance is Stance_Busy stance_Busy && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
            {
                Vector3 a = (!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos;
                float num = 0f;
                if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
                {
                    num = (a - pawn.DrawPos).AngleFlat();
                }
                Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
				drawLoc = drawLoc.SetLayer(120);
                DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num, mesh);
			}
            else if (CarryWeaponOpenly(pawn))
            {
                if (pawn.Rotation == Rot4.South)
                {
                    Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
                    drawLoc2.y += 9f / 245f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, 143f, mesh);
				}
                else if (pawn.Rotation == Rot4.North)
                {
                    Vector3 drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
                    drawLoc3.y += 0f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc3, 143f, mesh);
                }
                else if (pawn.Rotation == Rot4.East)
                {
                    Vector3 drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
                    drawLoc4.y += 9f / 245f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc4, 143f, mesh);
				}
                else if (pawn.Rotation == Rot4.West)
                {
                    Vector3 drawLoc5 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
                    drawLoc5.y += 9f / 245f;
                    DrawEquipmentAiming(pawn.equipment.Primary, drawLoc5, 217f, mesh);
                }
            }	
        }

		private static void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle, Mesh mesh)
		{
            float num = aimAngle - 90f;
            if (aimAngle > 20f && aimAngle < 160f)
            {
                num += eq.def.equippedAngleOffset;
            }
            else if (aimAngle > 200f && aimAngle < 340f)
            {
                num -= 180f;
                num -= eq.def.equippedAngleOffset;
            }
            else
            {
                num += eq.def.equippedAngleOffset;
            }
            num %= 360f;
			Graphics.DrawMesh(mesh, drawLoc, Quaternion.AngleAxis(num, Vector3.up), (!(eq.Graphic is Graphic_StackCount graphic_StackCount)) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, 0);
        }

		private static bool CarryWeaponOpenly(Pawn pawn)
		{
			if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
			{
				return false;
			}
			if (pawn.Drafted)
			{
				return true;
			}
			if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
			{
				return true;
			}
			if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)
			{
				return true;
			}
			Lord lord = pawn.GetLord();
			if (lord != null && lord.LordJob != null && lord.LordJob.AlwaysShowWeapon)
			{
				return true;
			}
			return false;
		}
		*/
	}

	[HarmonyPatch(typeof(PawnRenderer))]
	[HarmonyPatch("BaseHeadOffsetAt")]
	public static class BaseHeadOffsetAt
    {
		[HarmonyPostfix]
		public static void Postfix(ref Vector3 __result, Rot4 rotation, Pawn ___pawn)
		{
			if (___pawn.def is RaceAddonThingDef && ___pawn.GetComp<RaceAddonComp>() is var racomp)
			{
				__result += racomp.torsoDef.headOffsetsCorrection.GetLoc(rotation);
			}
		}
    }
}