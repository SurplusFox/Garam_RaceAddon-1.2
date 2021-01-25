using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Garam_RaceAddon
{
    public class RaceAddonGraphicSet
    {
        private readonly Pawn pawn;
        private readonly RaceAddonComp racomp;

        internal bool bodyDamaged = false;
        internal bool headDamaged = false;
        private HediffDef lastBodyHediffDef;
        private HediffDef lastHeadHediffDef;

        public AgeSetting presentAgeSetting;

        public GraphicMeshSet headMeshSet;
        public GraphicMeshSet bodyMeshSet;
        //public GraphicMeshSet equipmentMeshSet;

        public readonly EyeBlinker eyeBlinker;
        public readonly HeadRotator headRotator;
        public readonly HeadTargeter headTargeter;
        
        public FaceGraphicRecord upperFaceGraphic;
        public FaceGraphicRecord lowerFaceGraphic;
        public List<AddonGraphicRecord> addonGraphics = new List<AddonGraphicRecord>();

        public Graphic hairGraphic;

        public bool drawHat = true;
        public bool drawUpperHair = true;
        public bool drawLowerHair = true;

        public RaceAddonGraphicSet(Pawn pawn, RaceAddonComp racomp)
        {
            if (pawn.def is RaceAddonThingDef thingDef)
            {
                this.pawn = pawn;
                this.racomp = racomp;

                presentAgeSetting = thingDef.raceAddonSettings.ageSettings.GetPresent(pawn);

                headMeshSet = new GraphicMeshSet((1.5f * presentAgeSetting.drawSize.head.x) * racomp.drawSizeDeviation, (1.5f * presentAgeSetting.drawSize.head.y) * racomp.drawSizeDeviation);
                bodyMeshSet = new GraphicMeshSet((1.5f * presentAgeSetting.drawSize.body.x) * racomp.drawSizeDeviation, (1.5f * presentAgeSetting.drawSize.body.y) * racomp.drawSizeDeviation);
                //equipmentMeshSet = new GraphicMeshSet(presentAgeSetting.drawSize.equipment.x, presentAgeSetting.drawSize.equipment.y);
                racomp.cachedDrawLocCorrection = 0f;
                if (thingDef.raceAddonSettings.graphicSetting.footPositionCorrection_DrawSize)
                    racomp.cachedDrawLocCorrection += (1.5f - (1.5f * presentAgeSetting.drawSize.body.y)) / 2f;
                if (thingDef.raceAddonSettings.graphicSetting.footPositionCorrection_DrawSizeCurve)
                    racomp.cachedDrawLocCorrection += (1.5f - (1.5f * racomp.drawSizeDeviation)) / 2f;

                eyeBlinker = thingDef.raceAddonSettings.graphicSetting.eyeBlink ? new EyeBlinker() : null;
                headRotator = thingDef.raceAddonSettings.graphicSetting.headAnimation ? new HeadRotator() : null;
                headTargeter = thingDef.raceAddonSettings.graphicSetting.headTargeting ? new HeadTargeter(pawn) : null;

                upperFaceGraphic = racomp.upperFaceDef == null ? null : new FaceGraphicRecord(pawn, racomp.upperFaceDef, racomp.faceColor_Main, racomp.faceColor_Sub, thingDef.raceAddonSettings.graphicSetting.fixedUpperFace);
                lowerFaceGraphic = racomp.lowerFaceDef == null ? null : new FaceGraphicRecord(pawn, racomp.lowerFaceDef, racomp.faceColor_Main, racomp.faceColor_Sub, thingDef.raceAddonSettings.graphicSetting.fixedLowerFace);
                foreach (var addonData in racomp.addonDatas)
                {
                    var linkedBodyPart = addonData.addonDef.linkedBodyPart.GetBodyPartRecord(pawn.def.race.body);
                    addonGraphics.Add(new AddonGraphicRecord(addonData, linkedBodyPart, thingDef.raceAddonSettings.graphicSetting.rottingColor));
                }
            }
            else
            {
                RaceAddon.Notify("RaceAddonGraphicSet Init failure, Unkown ThingDef => " + pawn.Name.ToStringShort, true);
            }
        }

        public void Update_Normal()
        {
            eyeBlinker?.Check(pawn.needs.mood.CurLevel);
            headRotator?.Check();
            headTargeter?.Check();
        }

        public void Update_Rare()
        {
            CheckAgeSetting();
            //BodyCheck(pawn.health.hediffSet);
            //HeadCheck(pawn.health.hediffSet);
            TorsoCheck(pawn.health.hediffSet);
            upperFaceGraphic?.Check(pawn.health.hediffSet);
            lowerFaceGraphic?.Check(pawn.health.hediffSet);
            addonGraphics.ForEach(x => x.Check(pawn.health.hediffSet));
        }

        public void CheckAgeSetting()
        {
            var thingDef = pawn.def as RaceAddonThingDef;
            var newAgeSetting = thingDef.raceAddonSettings.ageSettings.GetPresent(pawn);
            if (presentAgeSetting != newAgeSetting)
            {
                presentAgeSetting = newAgeSetting;
                AppearanceDef appearanceDef = presentAgeSetting.appearances?.GetAppearanceDef(pawn, true);
                if (appearanceDef != null && !presentAgeSetting.keepAppearance)
                {
                    RaceAddon.SetAppearance(pawn, racomp, thingDef, appearanceDef);
                }
                racomp.raceAddonGraphicSet = new RaceAddonGraphicSet(pawn, racomp);
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                if (pawn.IsColonist)
                {
                    Find.LetterStack.ReceiveLetter("RaceAddon_GrowUp_Label".Translate(pawn.Name.ToStringShort), "RaceAddon_GrowUp_String".Translate(), LetterDefOf.PositiveEvent);
                }
                /*
                if (presentAgeSetting.ageBackstory != null && thingDef.raceAddonSettings.ageSettings[thingDef.raceAddonSettings.ageSettings.IndexOf(presentAgeSetting) - 1].ageBackstory.Backstory == pawn.story.childhood)
                {
                    pawn.story.childhood = presentAgeSetting.ageBackstory.Backstory;

                    if (presentAgeSetting.ageBackstory.skillGains.Count > 0)
                    {
                        foreach (SkillGain skillGain in presentAgeSetting.ageBackstory.skillGains)
                        {
                            pawn.skills.Learn(skillGain.skill, skillGain.xp, true);
                            pawn.skills.skills.ForEach(x => x.Level += x.def == skillGain.skill ? skillGain.xp : 0);
                        }
                    }
                    if (presentAgeSetting.ageBackstory.forcedHediffs.Count > 0)
                    {
                        foreach (var set in presentAgeSetting.ageBackstory.forcedHediffs)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(set.hediffDef, pawn, set.targetPart.GetBodyPartRecord(pawn.def.race.body));
                            hediff.Severity = set.severity;
                            pawn.health.AddHediff(hediff);
                        }
                    }

                    pawn.workSettings = new Pawn_WorkSettings(pawn);
                    pawn.workSettings.EnableAndInitialize();
                }
                */
            }
        }
        /*
        public void BodyCheck(HediffSet hediffSet)
        {
            //var list = hediffSet.hediffs.FindAll(x => x.Part != null && x.Part.IsInGroup(racomp.torsoDef.linkedBodyGroup));
            var list = hediffSet.hediffs.FindAll(x => x.Part == null && racomp.torsoDef.linkedBodyGroup == null || x.Part != null && x.Part.IsInGroup(racomp.torsoDef.linkedBodyGroup));
            if (racomp.torsoDef.bodyPath.customs.FindAll(x => list.Any(y => y.def == x.hediffDef)) is var list2 && list2.Count > 0)
            {
                list2.Sort((x, y) => y.priority - x.priority);
                if (list2[0].hediffDef != lastBodyHediffDef)
                {
                    ResolveAllGraphics.RenewalBodyGraphic(pawn.Drawer.renderer.graphics, list2[0].path, racomp.torsoDef.shaderType.Shader, racomp, (pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.rottingColor);
                    lastBodyHediffDef = list2[0].hediffDef;
                }
            }
        }

        public void HeadCheck(HediffSet hediffSet)
        {
            //var list = hediffSet.hediffs.FindAll(x => x.Part != null && x.Part.IsInGroup(racomp.torsoDef.linkedHeadGroup));
            var list = hediffSet.hediffs.FindAll(x => x.Part == null && racomp.torsoDef.linkedHeadGroup == null || x.Part != null && x.Part.IsInGroup(racomp.torsoDef.linkedHeadGroup));
            if (racomp.torsoDef.headPath.customs.FindAll(x => list.Any(y => y.def == x.hediffDef)) is var list2 && list2.Count > 0)
            {
                list2.Sort((x, y) => y.priority - x.priority);
                if (list2[0].hediffDef != lastHeadHediffDef)
                {
                    ResolveAllGraphics.RenewalHeadGraphic(pawn.Drawer.renderer.graphics, list2[0].path, racomp.torsoDef.shaderType.Shader, racomp, (pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.rottingColor);
                    lastHeadHediffDef = list2[0].hediffDef;
                }
            }
        }
        */
        public void TorsoCheck(HediffSet hediffSet)
        {
            if (racomp.torsoDef.headPath.customs.FindAll(x => hediffSet.HasHediff(x.hediffDef, null)) is var headList && headList.Count > 0)
            {
                headList.Sort((x, y) => y.priority - x.priority);
                if (headList[0].hediffDef != lastHeadHediffDef)
                {
                    ResolveAllGraphics.RenewalHeadGraphic(pawn.Drawer.renderer.graphics, headList[0].path, racomp.torsoDef.shaderType.Shader, racomp, (pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.rottingColor);
                    lastHeadHediffDef = headList[0].hediffDef;
                }
            }
            if (racomp.torsoDef.bodyPath.customs.FindAll(x => hediffSet.HasHediff(x.hediffDef, null)) is var bodyList && bodyList.Count > 0)
            {
                bodyList.Sort((x, y) => y.priority - x.priority);
                if (bodyList[0].hediffDef != lastBodyHediffDef)
                {
                    ResolveAllGraphics.RenewalHeadGraphic(pawn.Drawer.renderer.graphics, bodyList[0].path, racomp.torsoDef.shaderType.Shader, racomp, (pawn.def as RaceAddonThingDef).raceAddonSettings.graphicSetting.rottingColor);
                    lastBodyHediffDef = bodyList[0].hediffDef;
                }
            }
            var injuredParts = hediffSet.GetInjuredParts();
            bodyDamaged = injuredParts.Any(x => x.IsInGroup(racomp.torsoDef.linkedBodyGroup));
            headDamaged = injuredParts.Any(x => x.IsInGroup(racomp.torsoDef.linkedHeadGroup));
        }
    }
    public class FaceGraphicRecord
    {
        private readonly bool fixedBlinkWinkFace;
        private readonly Pawn pawn;
        public readonly Graphic mentalBreak;
        public readonly Graphic aboutToBreak;
        public readonly Graphic onEdge;
        public readonly Graphic stressed;
        public readonly Graphic neutral;
        public readonly Graphic content;
        public readonly Graphic happy;
        public readonly Graphic sleeping;
        public readonly Graphic painShock;
        public readonly Graphic dead;
        public readonly Graphic blink;
        public readonly Graphic wink;
        public readonly Graphic damaged;
        public readonly Graphic drafted;
        public readonly Graphic attacking;
        public readonly FaceDef faceDef;
        public Pair<CustomPath, Graphic> custom;

        public FaceGraphicRecord(Pawn pawn, FaceDef faceDef, Color main, Color sub, bool fixedBlinkWinkFace)
        {
            this.fixedBlinkWinkFace = fixedBlinkWinkFace;
            this.pawn = pawn;
            this.faceDef = faceDef;
            var shader = faceDef.shaderType.Shader;
            mentalBreak =   faceDef.mentalBreakPath.GetGraphic(shader, main, sub);
            aboutToBreak =  faceDef.aboutToBreakPath.GetGraphic(shader, main, sub);
            onEdge =        faceDef.onEdgePath.GetGraphic(shader, main, sub);
            stressed =      faceDef.stressedPath.GetGraphic(shader, main, sub);
            neutral =       faceDef.neutralPath.GetGraphic(shader, main, sub);
            content =       faceDef.contentPath.GetGraphic(shader, main, sub);
            happy =         faceDef.happyPath.GetGraphic(shader, main, sub);
            sleeping =      faceDef.sleepingPath.GetGraphic(shader, main, sub);
            painShock =     faceDef.painShockPath.GetGraphic(shader, main, sub);
            dead =          faceDef.deadPath.GetGraphic(shader, main, sub);
            blink =         faceDef.blinkPath.GetGraphic(shader, main, sub);
            wink =          faceDef.winkPath.GetGraphic(shader, main, sub);
            damaged =       faceDef.damagedPath.GetGraphic(shader, main, sub);
            drafted =       faceDef.draftedPath.GetGraphic(shader, main, sub);
            attacking =     faceDef.attackingPath.GetGraphic(shader, main, sub);
        }

        public void Update(HediffSet hediffSet)
        {
            foreach (var custom in faceDef.customs)
            {
                if (hediffSet.HasHediff(custom.hediffDef) && custom.priority > this.custom.First.priority)
                {
                    this.custom = new Pair<CustomPath, Graphic>(custom, custom.path.GetGraphic(neutral.Shader, neutral.Color, neutral.ColorTwo));
                    return;
                }
            }
        }

        public void Check(HediffSet hediffSet)
        {
            if (!hediffSet.HasHediff(custom.First?.hediffDef))
            {
                custom = new Pair<CustomPath, Graphic>();
            }
        }

        public Material MatAt(Rot4 rot, bool portrait, bool blinkNow, bool winkNow)
        {
            if (pawn.Dead)
            {
                return dead.MatAt(rot);
            }
            if (portrait)
            {
                return neutral.MatAt(rot);
            }
            if (pawn.health.InPainShock)
            {
                return painShock.MatAt(rot);
            }
            if (!pawn.Awake())
            {
                return sleeping.MatAt(rot);
            }
            if (attacking != null && pawn.CurJobDef != JobDefOf.Wait_Combat && pawn.IsFighting())
            {
                return attacking.MatAt(rot);
            }
            if (damaged != null && pawn.Drawer.renderer.graphics.flasher.FlashingNowOrRecently)
            {
                return damaged.MatAt(rot);
            }
            if (blinkNow && !fixedBlinkWinkFace)
            {
                return blink.MatAt(rot);
            }
            if (winkNow && !fixedBlinkWinkFace)
            {
                return wink.MatAt(rot);
            }
            if (drafted != null && pawn.Drafted)
            {
                return drafted.MatAt(rot);
            }
            if (pawn.MentalStateDef != null)
            {
                return mentalBreak.MatAt(rot);
            }
            if (custom.Second != null)
            {
                return custom.Second.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true))
            {
                return aboutToBreak.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < pawn.GetStatValue(StatDefOf.MentalBreakThreshold, true) + 0.05f)
            {
                return onEdge.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor)
            {
                return stressed.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < 0.65f)
            {
                return neutral.MatAt(rot);
            }
            if (pawn.needs.mood.CurLevel < 0.9f)
            {
                return content.MatAt(rot);
            }
            return happy.MatAt(rot);
        }
    }

    public class AddonGraphicRecord
    {
        public readonly Graphic normal;
        public readonly Graphic normalRotting;
        public readonly Graphic damaged;
        public readonly Graphic damagedRotting;
        public Pair<CustomPath, Graphic> custom;
        public Pair<CustomPath, Graphic> customRotting;
        public bool partDamaged = false;
        public readonly AddonData data;
        public readonly BodyPartRecord linkedBodyPart;

        public AddonGraphicRecord(AddonData data, BodyPartRecord linkedBodyPart, Color rottingColor)
        {
            this.data = data;
            this.linkedBodyPart = linkedBodyPart;
            var shader = data.addonDef.shaderType.Shader;
            normal = data.addonDef.addonPath.normal.GetGraphic(shader, data.addonColor_Main, data.addonColor_Sub);;
            normalRotting = data.addonDef.addonPath.normal.GetGraphic(shader, rottingColor, Color.clear);
            damaged = data.addonDef.addonPath.damaged.GetGraphic(shader, data.addonColor_Main, data.addonColor_Sub); ;
            damagedRotting = data.addonDef.addonPath.damaged.GetGraphic(shader, rottingColor, Color.clear);
        }

        public void Update(HediffSet hediffSet)
        {
            foreach (var custom in data.addonDef.addonPath.customs)
            {
                if (hediffSet.HasHediff(custom.hediffDef, linkedBodyPart) && (this.custom.First == null || custom.priority > this.custom.First.priority))
                {
                    this.custom = new Pair<CustomPath, Graphic>(custom, custom.path.GetGraphic(normal.Shader, normal.Color, normal.ColorTwo));
                    customRotting = new Pair<CustomPath, Graphic>(custom, custom.path.GetGraphic(normalRotting.Shader, normalRotting.Color, normalRotting.ColorTwo));
                    break;
                }
            }
            if (linkedBodyPart != null)
            {
                partDamaged = hediffSet.GetInjuredParts().Contains(linkedBodyPart);
            }
        }

        public void Check(HediffSet hediffSet)
        {
            if (!hediffSet.HasHediff(custom.First?.hediffDef, linkedBodyPart))
            {
                custom = new Pair<CustomPath, Graphic>();
                customRotting = new Pair<CustomPath, Graphic>();
                Update(hediffSet);
            }
            if (partDamaged && linkedBodyPart != null)
            {
                partDamaged = hediffSet.GetInjuredParts().Contains(linkedBodyPart);
            }
        }

        public Material MatAt(Rot4 rot, RotDrawMode rotting)
        {
            if (rotting == RotDrawMode.Fresh || !data.addonDef.rotting)
            {
                if (custom.Second != null)
                {
                    return custom.Second.MatAt(rot);
                }
                if (damaged != null && partDamaged)
                {
                    return damaged.MatAt(rot);
                }
                return normal.MatAt(rot);
            }
            if (rotting == RotDrawMode.Rotting)
            {
                if (customRotting.Second != null)
                {
                    return customRotting.Second.MatAt(rot);
                }
                if (damaged != null && partDamaged)
                {
                    return damaged.MatAt(rot);
                }
                return normalRotting.MatAt(rot);
            }
            return null;
        }
    }

    public class EyeBlinker
    {
        private int nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);

        private int currentTick = 0;
        private int tickForOverallAnimation = 0;

        private const int minWaitTick = 120;
        private const int maxWaitTick = 240;

        private const int minTickForAnimation = 30;
        private const int maxTickForAnimation = 60;

        private const float winkChance = 0.2f;

        public void Check(float mood)
        {
            if (!NowPlaying)
            {
                nextDelay--;
                if (nextDelay == 0)
                {
                    // start animation
                    tickForOverallAnimation = Rand.RangeInclusive(minTickForAnimation, maxTickForAnimation);
                    if (Rand.Chance(winkChance * mood))
                    {
                        WinkNow = true;
                    }
                    else
                    {
                        BlinkNow = true;
                    }
                }
            }
            else
            {
                currentTick++;
                if (currentTick > tickForOverallAnimation)
                {
                    // waiting tick
                    WinkNow = false;
                    BlinkNow = false;
                    currentTick = 0;
                    nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                }
            }
        }

        public bool NowPlaying
        {
            get
            {
                return nextDelay <= 0;
            }
        }

        public bool WinkNow { get; private set; } = false;

        public bool BlinkNow { get; private set; } = false;

        public void ForcedExecution()
        {
            nextDelay = 1;
        }
    }

    public class HeadRotator // 꾸악님 고마워용!!!!
    {
        private int nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);

        private int currentTick = 0;
        private int tickForOverallAnimation = 0;    // T
        private float currentAnimationAngle = 0f;   // A
        private int rotateDirectionSign = 0;

        private const int minWaitTick = 60;
        private const int maxWaitTick = 600;

        private const int minTickForAnimation = 120;
        private const int maxTickForAnimation = 240;

        private const float minAngle = 2.0f;
        private const float maxAngle = 8.0f;

        public void Check()
        {
            if (!NowPlaying)
            {
                nextDelay--;
                if (nextDelay == 0)
                {
                    // start animation
                    rotateDirectionSign = Rand.Bool ? 1 : -1;
                    tickForOverallAnimation = Rand.RangeInclusive(minTickForAnimation, maxTickForAnimation) * 2;
                    currentAnimationAngle = minAngle + Rand.Value * (maxAngle - minAngle);
                }
            }
            else
            {
                currentTick++;
                if (currentTick > tickForOverallAnimation)
                {
                    // waiting tick
                    currentTick = 0;
                    nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                }
            }
        }

        public bool NowPlaying
        {
            get
            {
                return nextDelay <= 0;
            }
        }

        private float Angle
        {
            get
            {
                if (!NowPlaying)
                {
                    return 0f;
                }
                else if (currentTick < tickForOverallAnimation / 4)
                {
                    // f1 is solution of ODE like
                    // f1'(0) = 0, f1'(T/4) = 0, f1(0) = 0, f1(T/4) = A
                    // I choose 3th polynomial for my convenience. but there is many solutions.
                    return (-currentAnimationAngle / Mathf.Pow(tickForOverallAnimation, 3f) * Mathf.Pow((float)currentTick, 2)) * (128 * currentTick - 48 * tickForOverallAnimation) * rotateDirectionSign;
                }
                else if (currentTick < tickForOverallAnimation * 3 / 4)
                {
                    // f2 is solution of ODE like
                    // f2'(T/4) = 0, f2'(3T/4) = 0, f2(T/4) = A, f2(3T/4) = -A
                    // I choose A * sin(2pi*x / T) for my convenience. but there is many solutions.
                    return (currentAnimationAngle * Mathf.Sin(2f * Mathf.PI * (float)currentTick / (float)tickForOverallAnimation)) * rotateDirectionSign;
                }
                else
                {
                    // f3 is solution of ODE like
                    // f3'(3T/4) = 0, f3'(T) = 0, f3(3T/4) = -A, f3(T) = 0
                    // I choose 3th polynomial for my convenience. but there is many solutions.
                    return (currentAnimationAngle / Mathf.Pow(tickForOverallAnimation, 3f)) * (float)(80 * tickForOverallAnimation - 128 * currentTick) * Mathf.Pow(currentTick - tickForOverallAnimation, 2f) * rotateDirectionSign;
                }
            }
        }

        public Quaternion GetQuat()
        {
            return Quaternion.AngleAxis(Angle, Vector3.up);
        }
    }

    public class HeadTargeter
    {
        private int nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);

        private readonly Pawn pawn;
        private Pawn target;
        public RotationDirection RotDirection { get; private set; } = RotationDirection.None;

        private int currentTick = 0;
        private int tickForOverallAnimation = 0;

        private const int minWaitTick = 600;
        private const int maxWaitTick = 1800;

        private const int minTickForAnimation = 120;
        private const int maxTickForAnimation = 360;

        public HeadTargeter(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public void Check()
        {
            if (!NowPlaying)
            {
                nextDelay--;
                if (nextDelay == 0)
                {
                    target = GetTarget();
                    if (target != null)
                    {
                        // start animation
                        tickForOverallAnimation = Rand.RangeInclusive(minTickForAnimation, maxTickForAnimation);
                        RotDirection = GetRotDiretion();
                    }
                    else
                    {
                        nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                    }
                }
            }
            else
            {
                currentTick++;
                if (currentTick % 20 == 0)
                {
                    RotDirection = GetRotDiretion();
                }
                if (currentTick > tickForOverallAnimation)
                {
                    // waiting tick
                    target = null;
                    RotDirection = RotationDirection.None;
                    currentTick = 0;
                    nextDelay = Rand.RangeInclusive(minWaitTick, maxWaitTick);
                }
            }
        }

        public bool NowPlaying
        {
            get
            {
                return nextDelay <= 0;
            }
        }

        private Pawn GetTarget()
        {
            List<Pawn> targets = new List<Pawn>();
            for (int i = 1; i < 57; i++)
            {
                IntVec3 intVec = pawn.Position + GenRadial.RadialPattern[i];
                if (pawn.Map != null && intVec.InBounds(pawn.Map))
                {
                    Pawn target = intVec.GetFirstPawn(pawn.Map);
                    if (target != null && !target.Dead && !target.Downed && GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map))
                    {
                        if (target.HostileTo(pawn))
                        {
                            return null;
                        }
                        else
                        {
                            targets.Add(target);
                        }
                    }
                }
            }
            if (targets.Count > 0)
            {
                return targets.RandomElement();
            }
            return null;
        }

        private RotationDirection GetRotDiretion()
        {
            float angle = (target.Position - pawn.Position).ToVector3().AngleFlat();
            Rot4 rot = Pawn_RotationTracker.RotFromAngleBiased(angle);
            if (rot != pawn.Rotation.Opposite)
            {
                switch (pawn.Rotation.AsInt - rot.AsInt)
                {
                    case 0:
                        return RotationDirection.None;
                    case -1:
                        return RotationDirection.Clockwise;
                    case 1:
                        return RotationDirection.Counterclockwise;
                }
            }
            return RotationDirection.None;
        }
    }
}
