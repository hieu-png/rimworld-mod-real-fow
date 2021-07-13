using System;
using System.Collections.Generic;
using RimWorld;
using RimWorldRealFoW;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
    // Token: 0x02000014 RID: 20
    public class CompFieldOfViewWatcher : ThingSubComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            this.setupDone = true;
            this.calculated = false;
            this.lastPosition = CompFieldOfViewWatcher.iv3Invalid;
            this.lastSightRange = 0;
            this.lastPeekDirections = null;
            this.viewMap1 = null;
            this.viewMap2 = null;
            this.viewRect = new CellRect(-1, -1, 0, 0);
            this.viewPositions = new IntVec3[5];
            this.compHiddenable = this.mainComponent.compHiddenable;

            this.compGlower = this.parent.GetComp<CompGlower>();
            this.compPowerTrader = this.parent.GetComp<CompPowerTrader>();
            this.compRefuelable = this.parent.GetComp<CompRefuelable>();
            this.compFlickable = this.parent.GetComp<CompFlickable>();
            this.compMannable = this.parent.GetComp<CompMannable>();
            this.compProvideVision = this.parent.GetComp<CompProvideVision>();

            this.pawn = (this.parent as Pawn);
            this.building = (this.parent as Building);
            this.turret = (this.parent as Building_TurretGun);

            if (this.pawn != null)
            {
                this.raceProps = this.pawn.RaceProps;
                this.hediffs = this.pawn.health.hediffSet.hediffs;
                this.capacities = this.pawn.health.capacities;
                this.pawnPather = this.pawn.pather;
            }
            this.def = this.parent.def;
            if (this.def.race != null)
            {
                this.isMechanoid = this.def.race.IsMechanoid;
            }
            else
            {
                this.isMechanoid = false;
            }
           // RealFoWModDefaultsDef named = DefDatabase<RealFoWModDefaultsDef>.GetNamed(RealFoWModDefaultsDef.DEFAULT_DEF_NAME, true);

            if (!this.isMechanoid)
            {
                this.baseViewRange = RFOWSettings.baseViewRange;
            }
            else
            {
                this.baseViewRange = Mathf.Round(RFOWSettings.baseViewRange * 1.25f);
            }
            if (
            this.pawn == null 
            && (this.turret == null || this.compMannable != null) 
            && this.compProvideVision == null 
            && this.building == null)
            {
                Log.Message("Removing unneeded FoV watcher from " + this.parent.ThingID, false);
                this.disabled = true;
                this.mainComponent.compFieldOfViewWatcher = null;
            }
            else
            {
                this.disabled = false;
            }
            this.initMap();
            this.lastMovementTick = Find.TickManager.TicksGame;
            this.lastPositionUpdateTick = this.lastMovementTick;
            this.updateFoV(false);
        }

        // Token: 0x06000067 RID: 103 RVA: 0x00006DDF File Offset: 0x00004FDF
        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.lastMovementTick, "fovLastMovementTick", Find.TickManager.TicksGame, false);
        }

        // Token: 0x06000068 RID: 104 RVA: 0x00006DFE File Offset: 0x00004FFE
        public override void ReceiveCompSignal(string signal)
        {
            this.updateFoV(false);
        }

        // Token: 0x06000069 RID: 105 RVA: 0x00006E0C File Offset: 0x0000500C
        public override void CompTick()
        {
            if (!this.disabled)
            {
                int ticksGame = Find.TickManager.TicksGame;
                if (this.pawn != null)
                {
                    if (this.pawnPather == null)
                    {
                        this.pawnPather = this.pawn.pather;
                    }
                    if (this.pawnPather != null && this.pawnPather.Moving)
                    {
                        this.lastMovementTick = ticksGame;
                    }
                    if (this.lastPosition != CompFieldOfViewWatcher.iv3Invalid && this.lastPosition != this.parent.Position)
                    {
                        this.lastPositionUpdateTick = ticksGame;
                        updateFoV(false);
                    }
                    else
                    {
                        if ((ticksGame - this.lastPositionUpdateTick) % 30 == 0)
                        {
                            updateFoV(false);
                        }
                    }
                }
                else
                {
                    if ((this.lastPosition != CompFieldOfViewWatcher.iv3Invalid
                          && this.lastPosition != this.parent.Position) || ticksGame % 30 == 0)
                    {
                        this.updateFoV(false);
                    }
                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
        }

        // Token: 0x0600006A RID: 106 RVA: 0x00006F34 File Offset: 0x00005134
        private void initMap()
        {
            if (this.map != this.parent.Map)
            {
                if (this.map != null && this.lastFaction != null)
                {
                    this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                }
                if (!this.disabled && this.mapCompSeenFog != null)
                {
                    this.mapCompSeenFog.fowWatchers.Remove(this);
                }
                this.map = this.parent.Map;
                this.mapCompSeenFog = this.map.getMapComponentSeenFog();
                this.thingGrid = this.map.thingGrid;
                this.glowGrid = this.map.glowGrid;
                this.roofGrid = this.map.roofGrid;
                this.weatherManager = this.map.weatherManager;
                this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(this.parent.Faction);
                if (!this.disabled)
                {
                    this.mapCompSeenFog.fowWatchers.Add(this);
                }
                this.mapSizeX = this.map.Size.x;
                this.mapSizeZ = this.map.Size.z;
            }
        }

        // Token: 0x0600006B RID: 107 RVA: 0x0000708C File Offset: 0x0000528C
        public void updateFoV(bool forceUpdate = false)
        {
            if (!this.disabled || !this.setupDone || Current.ProgramState == ProgramState.MapInitializing)
            {
                ThingWithComps parent = this.parent;
                IntVec3 position = parent.Position;
                if (parent != null && parent.Spawned && parent.Map != null && position != CompFieldOfViewWatcher.iv3Invalid)
                {

                    this.initMap();
                    Faction faction = parent.Faction;
                    if (faction != null && (this.pawn == null || !this.pawn.Dead))
                    {

                        if (this.pawn != null)
                        {

                            IntVec3[] peekDirection = null;
                            int sightRange;
                            if (this.raceProps != null
                                && this.raceProps.Animal
                                && (this.pawn.playerSettings == null
                                || this.pawn.playerSettings.Master == null
                                || this.pawn.training == null
                                || !this.pawn.training.HasLearned(TrainableDefOf.Release)))
                            {

                                sightRange = -1;
                            }
                            else
                            {
                                sightRange = Mathf.RoundToInt(this.calcPawnSightRange(position, false, false));
                                if(this.raceProps.Animal) {
                                    sightRange = (int)(sightRange*RFOWSettings.animalVisionModifier*raceProps.baseBodySize*0.8f);
                                }
                                if ((this.pawnPather == null
                                    || !this.pawnPather.Moving)
                                    && this.pawn.CurJob != null)
                                {
                                    JobDef jobDef = this.pawn.CurJob.def;
                                    if (
                                        jobDef == JobDefOf.AttackStatic
                                        || jobDef == JobDefOf.AttackMelee
                                        || jobDef == JobDefOf.Wait_Combat
                                        || jobDef == JobDefOf.Hunt)
                                    {
                                        peekDirection = GenAdj.CardinalDirections;
                                    }
                                    else if (
                                        jobDef == JobDefOf.Mine
                                        && this.pawn.CurJob.targetA != null
                                        && this.pawn.CurJob.targetA.Cell != IntVec3.Invalid)
                                    {
                                        peekDirection = Utils.FoWThingUtils.getPeekArray(this.pawn.CurJob.targetA.Cell - this.parent.Position);
                                    }

                                }
                            }
                            if (!this.calculated
                                || forceUpdate
                                || faction != this.lastFaction
                                || position != this.lastPosition
                                || sightRange != this.lastSightRange
                                || peekDirection != this.lastPeekDirections)
                            {
                                this.calculated = true;
                                this.lastPosition = position;
                                this.lastSightRange = sightRange;
                                this.lastPeekDirections = peekDirection;
                                if (this.lastFaction != faction)
                                {
                                    if (this.lastFaction != null)
                                    {
                                        this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                    }
                                    this.lastFaction = faction;
                                    this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
                                }
                                if (sightRange != -1)
                                {
                                    this.calculateFoV(parent, sightRange, peekDirection);
                                }
                                else
                                {
                                    this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                }
                            }
                        }
                        else if (this.turret != null && this.compMannable == null)
                        {
                            //Turret is more sensor based so reduced vision range, still can feed back some info

                            int sightRange = Mathf.RoundToInt(this.turret.GunCompEq.PrimaryVerb.verbProps.range*RFOWSettings.turretVisionModifier);

                            if ((this.compPowerTrader != null && !this.compPowerTrader.PowerOn)
                                || (this.compRefuelable != null && !this.compRefuelable.HasFuel)
                                || (this.compFlickable != null && !this.compFlickable.SwitchIsOn)
                                || !this.mapCompSeenFog.workingCameraConsole
                                )
                            {
                                sightRange = 0;
                            }
                            if (
                                !this.calculated
                                || forceUpdate
                                || faction != this.lastFaction
                                || position != this.lastPosition
                                || sightRange != this.lastSightRange)
                            {
                                this.calculated = true;
                                this.lastPosition = position;
                                this.lastSightRange = sightRange;
                                if (this.lastFaction != faction)
                                {
                                    if (this.lastFaction != null)
                                    {
                                        this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                    }
                                    this.lastFaction = faction;
                                    this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
                                }
                                if (sightRange != 0)
                                {
                                    this.calculateFoV(parent, sightRange, null);
                                }
                                else
                                {
                                    this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                    this.revealOccupiedCells();
                                }
                            }
                        }
                        else if (this.compProvideVision != null)
                        {

                            int viewRadius = Mathf.RoundToInt(this.compProvideVision.Props.viewRadius*RFOWSettings.buildingVisionModifier);
                            if ((this.compPowerTrader != null && !this.compPowerTrader.PowerOn)
                                || (this.compRefuelable != null && !this.compRefuelable.HasFuel)
                                || (this.compFlickable != null && !this.compFlickable.SwitchIsOn)
                                || (this.compProvideVision.Props.needManned && !this.mapCompSeenFog.workingCameraConsole)
                                )

                            {

                                viewRadius = 0;
                            }

                            if (!this.calculated
                                || forceUpdate
                                || faction != this.lastFaction
                                || position != this.lastPosition
                                || viewRadius != this.lastSightRange)
                            {
                                this.calculated = true;
                                this.lastPosition = position;
                                this.lastSightRange = viewRadius;
                                if (this.lastFaction != faction)
                                {
                                    if (this.lastFaction != null)
                                    {
                                        this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                    }
                                    this.lastFaction = faction;
                                    this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
                                }
                                if (viewRadius != 0)
                                {
                                    this.calculateFoV(parent, viewRadius, null);
                                }
                                else
                                {
                                    this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                    this.revealOccupiedCells();
                                }
                            }
                        }
                        else if (this.building != null)
                        {

                            int sightRange = 0;
                           
                            if (
                            !this.calculated 
                            || forceUpdate 
                            || faction != this.lastFaction 
                            || position != this.lastPosition 
                            || sightRange != this.lastSightRange)
                            {
                                this.calculated = true;
                                this.lastPosition = position;
                                this.lastSightRange = sightRange;
                                if (this.lastFaction != faction)
                                {
                                    if (this.lastFaction != null)
                                    {
                                        this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                    }
                                    this.lastFaction = faction;
                                    this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
                                }
                                this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                                this.revealOccupiedCells();
                            }
                        }
                        else
                        {
                            Log.Warning("Non disabled thing... " + this.parent.ThingID, false);
                        }
                    }
                    else
                    {
                        if (faction != this.lastFaction)
                        {
                            if (this.lastFaction != null)
                            {
                                this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
                            }
                            this.lastFaction = faction;
                            this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
                        }
                    }
                }
            }
        }

        public float calcPawnSightRange(IntVec3 position, bool forTargeting, bool shouldMove)
        {
            float result;
            if (this.pawn == null)
            {
                Log.Error("calcPawnSightRange performed on non pawn thing", false);
                result = 0f;
            }
            else
            {
                float num = 0f;
                this.initMap();
                bool sleeping = !this.isMechanoid && this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep;
                if (!shouldMove && !sleeping && (this.pawnPather == null || !this.pawnPather.Moving))
                {
                    Verb attackVerb = null;
                    if (this.pawn.CurJob != null)
                    {
                        JobDef jobDef = this.pawn.CurJob.def;
                        //Get manned turret sight range.
                        if (jobDef == JobDefOf.ManTurret)
                        {
                            Building_Turret building_Turret = this.pawn.CurJob.targetA.Thing as Building_Turret;
                            bool flag6 = building_Turret != null;
                            if (flag6)
                            {
                                attackVerb = building_Turret.AttackVerb;
                            }
                        }
                        else
                        {
                            if (jobDef == JobDefOf.AttackStatic
                                || jobDef == JobDefOf.AttackMelee
                                || jobDef == JobDefOf.Wait_Combat
                                || jobDef == JobDefOf.Hunt)
                            {
                                bool flag8 = this.pawn.equipment != null;
                                if (flag8)
                                {
                                    ThingWithComps primary = this.pawn.equipment.Primary;
                                    bool flag9 = primary != null && primary.def.IsRangedWeapon;
                                    if (flag9)
                                    {
                                        attackVerb = primary.GetComp<CompEquippable>().PrimaryVerb;
                                    }
                                }
                            }
                        }
                    }
                    if (attackVerb != null
                        && attackVerb.verbProps.range > this.baseViewRange
                        && attackVerb.verbProps.requireLineOfSight
                        && attackVerb.EquipmentSource.def.IsRangedWeapon)
                    {
                        float range = attackVerb.verbProps.range;

                        if (this.baseViewRange < range)
                        {
                            int num2 = Find.TickManager.TicksGame - this.lastMovementTick;
                            float statValue = this.pawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
                            int num3 = (attackVerb.verbProps.warmupTime * statValue).SecondsToTicks() * Mathf.RoundToInt((range - this.baseViewRange) / 2f);
                            bool flag12 = num2 >= num3;
                            if (flag12)
                            {
                                num = range * this.capacities.GetLevel(PawnCapacityDefOf.Sight);
                            }
                            else
                            {
                                int sightRange = Mathf.RoundToInt((range - this.baseViewRange) * ((float)num2 / (float)num3));
                                num = (this.baseViewRange + (float)sightRange) * this.capacities.GetLevel(PawnCapacityDefOf.Sight);
                            }
                        }
                    }
                }
                if (num == 0f)
                {
                    num = this.baseViewRange * this.capacities.GetLevel(PawnCapacityDefOf.Sight);
                }
                if (!forTargeting && sleeping)
                {
                    num *= 0.2f;
                }
                List<CompAffectVision> visionAffectingBuilding = this.mapCompSeenFog.compAffectVisionGrid[position.z * this.mapSizeX + position.x];
                bool ignoreWeather = false;
                bool ignoreDarkness = false;
                foreach (CompAffectVision visionAffecter in visionAffectingBuilding)
                {
                    if (visionAffecter.Props.denyDarkness)
                        ignoreDarkness = true;
                    if (visionAffecter.Props.denyWeather)
                        ignoreWeather = true;
                    num *= visionAffecter.Props.fovMultiplier;
                }
                if (!this.isMechanoid)
                {
                    float currGlow = this.glowGrid.GameGlowAt(position, false);
                    if (currGlow != 1f)
                    {
                        float darknessModifier = 0.6f;
                        if (ignoreDarkness)
                            darknessModifier = 1f;
                        int count2 = this.hediffs.Count;
                        for (int j = 0; j < count2; j++)
                        {
                            if (this.hediffs[j].def == HediffDefOf.BionicEye)
                            {
                                darknessModifier += 0.2f;
                            }
                        }
                        if (darknessModifier < 1f)
                        {
                            num *= Mathf.Lerp(darknessModifier, 1f, currGlow);
                        }
                    }
                    if (!this.roofGrid.Roofed(position.x, position.z)&&!ignoreWeather)
                    {
                        float curWeatherAccuracyMultiplier = this.weatherManager.CurWeatherAccuracyMultiplier;
                        if (curWeatherAccuracyMultiplier != 1f)
                        {
                            num *= Mathf.Lerp(0.5f, 1f, curWeatherAccuracyMultiplier);
                        }
                    }
                }
                if (num < 1f)
                {
                    result = 1f;
                }
                else
                {
                    result = num;
                }
            }
            return result;
        }

        // Token: 0x0600006D RID: 109 RVA: 0x00007C54 File Offset: 0x00005E54
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            if (!this.disabled && this.mapCompSeenFog != null)
            {
                this.mapCompSeenFog.fowWatchers.Remove(this);
            }
            if (this.lastFaction != null)
            {
                this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
            }
        }

        // Token: 0x0600006E RID: 110 RVA: 0x00007CB8 File Offset: 0x00005EB8
        public void calculateFoV(Thing thing, int intRadius, IntVec3[] peekDirections)
        {
            if (this.setupDone)
            {
                int mapSizeX = this.mapSizeX;
                int mapSizeY = this.mapSizeZ;

                bool[] oldMapView = viewMapSwitch ? this.viewMap1 : this.viewMap2;
                bool[] newMapView = viewMapSwitch ? this.viewMap2 : this.viewMap1;

                IntVec3 position = thing.Position;

                Faction faction = this.lastFaction;

                short[] factionShownCells = this.lastFactionShownCells;

                int peekRadius = (peekDirections != null) ? (intRadius + 1) : intRadius;

                CellRect occupiedRect = thing.OccupiedRect();

                int newViewRecMinX = Math.Min(position.x - peekRadius, occupiedRect.minX);
                int newViewRecMaxX = Math.Max(position.x + peekRadius, occupiedRect.maxX);
                int newViewRecMinZ = Math.Min(position.z - peekRadius, occupiedRect.minZ);
                int newViewRecMaxZ = Math.Max(position.z + peekRadius, occupiedRect.maxZ);

                int newViewWidth = newViewRecMaxX - newViewRecMinX + 1;
                int newViewArea = newViewWidth * (newViewRecMaxZ - newViewRecMinZ + 1);

                int oldViewRecMinZ = this.viewRect.minZ;
                int oldViewRecMaxZ = this.viewRect.maxZ;
                int oldViewRecMinX = this.viewRect.minX;
                int oldViewRecMaxX = this.viewRect.maxX;

                int oldViewWidth = this.viewRect.Width;
                int oldViewArea = this.viewRect.Area;

                if (newMapView == null || newMapView.Length < newViewArea)
                {
                    newMapView = new bool[(int)((float)newViewArea * 1.2f)];
                    if (this.viewMapSwitch)
                    {
                        this.viewMap2 = newMapView;
                    }
                    else
                    {
                        this.viewMap1 = newMapView;
                    }
                }
                int occupiedX;
                int occupiedZ;
               // int oldViewRectIdx;
                for (occupiedX = occupiedRect.minX; occupiedX <= occupiedRect.maxX; occupiedX++)
                {
                    for (occupiedZ = occupiedRect.minZ; occupiedZ <= occupiedRect.maxZ; occupiedZ++)
                    {
                        newMapView[(occupiedZ - newViewRecMinZ) * newViewWidth + (occupiedX - newViewRecMinX)] = true;

                        if (oldMapView == null
                            || occupiedX < oldViewRecMinX
                            || occupiedZ < oldViewRecMinZ
                            || occupiedX > oldViewRecMaxX
                            || occupiedZ > oldViewRecMaxZ)
                        {
                            this.mapCompSeenFog.incrementSeen(faction, factionShownCells, occupiedZ * mapSizeX + occupiedX);
                        }
                        else
                        {
                            int num10 = (occupiedZ - oldViewRecMinZ) * oldViewWidth + (occupiedX - oldViewRecMinX);
                            ref bool ptr = ref oldMapView[num10];
                            if (!ptr)
                            {
                                this.mapCompSeenFog.incrementSeen(faction, factionShownCells, occupiedZ * mapSizeX + occupiedX);
                            }
                            else
                            {
                                ptr = false;
                            }
                        }
                    }
                }
                if (intRadius > 0)
                {
                    bool[] viewBlockerCells = this.mapCompSeenFog.viewBlockerCells;
                    this.viewPositions[0] = position;
                    int viewPositionCount;
                    if (peekDirections == null)
                    {
                        viewPositionCount = 1;
                    }
                    else
                    {
                        viewPositionCount = 1 + peekDirections.Length;
                        for (int k = 0; k < viewPositionCount - 1; k++)
                        {
                            this.viewPositions[1 + k] = position + peekDirections[k];
                        }
                    }
                    int num12 = this.map.Size.x - 1;
                    int num13 = this.map.Size.z - 1;
                    for (int l = 0; l < viewPositionCount; l++)
                    {
                        ref IntVec3 ptr2 = ref this.viewPositions[l];
                        bool flag8 = ptr2.x >= 0 && ptr2.z >= 0 && ptr2.x <= num12 && ptr2.z <= num13 && (l == 0 || ptr2.IsInside(thing) || !viewBlockerCells[ptr2.z * mapSizeX + ptr2.x]);
                        if (flag8)
                        {
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, intRadius, viewBlockerCells, mapSizeX, mapSizeY, true, this.mapCompSeenFog, faction, factionShownCells, newMapView, newViewRecMinX, newViewRecMinZ, newViewWidth, oldMapView, oldViewRecMinX, oldViewRecMaxX, oldViewRecMinZ, oldViewRecMaxZ, oldViewWidth, byte.MaxValue, -1, -1);
                        }
                    }
                }
                if (oldMapView != null)
                {
                    for (int m = 0; m < oldViewArea; m++)
                    {
                        ref bool ptr3 = ref oldMapView[m];
                        bool flag10 = ptr3;
                        if (flag10)
                        {
                            ptr3 = false;
                            int num14 = oldViewRecMinX + m % oldViewWidth;
                            int num15 = oldViewRecMinZ + m / oldViewWidth;
                            bool flag11 = num15 >= 0 && num15 <= mapSizeY && num14 >= 0 && num14 <= mapSizeX;
                            if (flag11)
                            {
                                this.mapCompSeenFog.decrementSeen(faction, factionShownCells, num15 * mapSizeX + num14);
                            }
                        }
                    }
                }
                this.viewMapSwitch = !this.viewMapSwitch;
                this.viewRect.maxX = newViewRecMaxX;
                this.viewRect.minX = newViewRecMinX;
                this.viewRect.maxZ = newViewRecMaxZ;
                this.viewRect.minZ = newViewRecMinZ;
            }
        }

        // Token: 0x0600006F RID: 111 RVA: 0x00008180 File Offset: 0x00006380
        public void refreshFovTarget(ref IntVec3 targetPos)
        {
            bool flag = !this.setupDone;
            if (!flag)
            {
                Thing parent = this.parent;
                bool[] array = this.viewMapSwitch ? this.viewMap1 : this.viewMap2;
                bool[] array2 = this.viewMapSwitch ? this.viewMap2 : this.viewMap1;
                bool flag2 = array == null || this.lastPosition != this.parent.Position;
                if (flag2)
                {
                    this.updateFoV(true);
                }
                else
                {
                    int radius = this.lastSightRange;
                    IntVec3[] array3 = this.lastPeekDirections;
                    int num = this.mapSizeX;
                    int num2 = this.mapSizeZ;
                    IntVec3 position = parent.Position;
                    Faction faction = this.lastFaction;
                    short[] factionShownCells = this.lastFactionShownCells;
                    CellRect cellRect = parent.OccupiedRect();
                    int minZ = this.viewRect.minZ;
                    int maxZ = this.viewRect.maxZ;
                    int minX = this.viewRect.minX;
                    int maxX = this.viewRect.maxX;
                    int width = this.viewRect.Width;
                    int area = this.viewRect.Area;
                    bool flag3 = array2 == null || array2.Length < area;
                    if (flag3)
                    {
                        array2 = new bool[(int)((float)area * 1.2f)];
                        bool flag4 = this.viewMapSwitch;
                        if (flag4)
                        {
                            this.viewMap2 = array2;
                        }
                        else
                        {
                            this.viewMap1 = array2;
                        }
                    }
                    for (int i = cellRect.minX; i <= cellRect.maxX; i++)
                    {
                        for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                        {
                            int num3 = (j - minZ) * width + (i - minX);
                            array2[num3] = true;
                            array[num3] = false;
                        }
                    }
                    bool[] viewBlockerCells = this.mapCompSeenFog.viewBlockerCells;
                    this.viewPositions[0] = position;
                    bool flag5 = array3 == null;
                    int sightRange;
                    if (flag5)
                    {
                        sightRange = 1;
                    }
                    else
                    {
                        sightRange = 1 + array3.Length;
                        for (int k = 0; k < sightRange - 1; k++)
                        {
                            this.viewPositions[1 + k] = position + array3[k];
                        }
                    }
                    int num5 = this.map.Size.x - 1;
                    int num6 = this.map.Size.z - 1;
                    bool flag6 = false;
                    bool flag7 = false;
                    bool flag8 = false;
                    bool flag9 = false;
                    for (int l = 0; l < sightRange; l++)
                    {
                        ref IntVec3 ptr = ref this.viewPositions[l];
                        bool flag10 = ptr.x >= 0 && ptr.z >= 0 && ptr.x <= num5 && ptr.z <= num6 && (l == 0 || ptr.IsInside(parent) || !viewBlockerCells[ptr.z * num + ptr.x]);
                        if (flag10)
                        {
                            bool flag11 = ptr.x <= targetPos.x;
                            if (flag11)
                            {
                                bool flag12 = ptr.z <= targetPos.z;
                                if (flag12)
                                {
                                    flag6 = true;
                                }
                                else
                                {
                                    flag9 = true;
                                }
                            }
                            else
                            {
                                bool flag13 = ptr.z <= targetPos.z;
                                if (flag13)
                                {
                                    flag7 = true;
                                }
                                else
                                {
                                    flag8 = true;
                                }
                            }
                        }
                    }
                    for (int m = 0; m < sightRange; m++)
                    {
                        ref IntVec3 ptr2 = ref this.viewPositions[m];
                        bool flag14 = ptr2.x >= 0 && ptr2.z >= 0 && ptr2.x <= num5 && ptr2.z <= num6 && (m == 0 || ptr2.IsInside(parent) || !viewBlockerCells[ptr2.z * num + ptr2.x]);
                        if (flag14)
                        {
                            bool flag15 = flag6;
                            if (flag15)
                            {
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 0, -1, -1);
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 1, -1, -1);
                            }
                            bool flag16 = flag7;
                            if (flag16)
                            {
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 2, -1, -1);
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 3, -1, -1);
                            }
                            bool flag17 = flag8;
                            if (flag17)
                            {
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 4, -1, -1);
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 5, -1, -1);
                            }
                            bool flag18 = flag9;
                            if (flag18)
                            {
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 6, -1, -1);
                                ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, minX, minZ, width, array, minX, maxX, minZ, maxZ, width, 7, -1, -1);
                            }
                        }
                    }
                    for (int n = 0; n < area; n++)
                    {
                        ref bool ptr3 = ref array[n];
                        bool flag19 = ptr3;
                        if (flag19)
                        {
                            ptr3 = false;
                            int num7 = minX + n % width;
                            int num8 = minZ + n / width;
                            bool flag20 = position.x <= num7;
                            byte b;
                            if (flag20)
                            {
                                bool flag21 = position.z <= num8;
                                if (flag21)
                                {
                                    b = 1;
                                }
                                else
                                {
                                    b = 4;
                                }
                            }
                            else
                            {
                                bool flag22 = position.z <= num8;
                                if (flag22)
                                {
                                    b = 2;
                                }
                                else
                                {
                                    b = 3;
                                }
                            }
                            bool flag23 = (!flag6 && b == 1) || (!flag7 && b == 2) || (!flag8 && b == 3) || (!flag9 && b == 4);
                            if (flag23)
                            {
                                array2[n] = true;
                            }
                            else
                            {
                                bool flag24 = num8 >= 0 && num8 <= num2 && num7 >= 0 && num7 <= num;
                                if (flag24)
                                {
                                    this.mapCompSeenFog.decrementSeen(faction, factionShownCells, num8 * num + num7);
                                }
                            }
                        }
                    }
                    this.viewMapSwitch = !this.viewMapSwitch;
                }
            }
        }

        // Token: 0x06000070 RID: 112 RVA: 0x000088AC File Offset: 0x00006AAC
        private void unseeSeenCells(Faction faction, short[] factionShownCells)
        {
            bool[] array = this.viewMapSwitch ? this.viewMap1 : this.viewMap2;
            bool flag = array != null;
            if (flag)
            {
                int minZ = this.viewRect.minZ;
                int maxZ = this.viewRect.maxZ;
                int minX = this.viewRect.minX;
                int maxX = this.viewRect.maxX;
                int x = this.map.Size.x;
                int z = this.map.Size.z;
                int width = this.viewRect.Width;
                int area = this.viewRect.Area;
                for (int i = 0; i < area; i++)
                {
                    bool flag2 = array[i];
                    if (flag2)
                    {
                        array[i] = false;
                        int num = minX + i % width;
                        int num2 = minZ + i / width;
                        bool flag3 = num2 >= 0 && num2 <= z && num >= 0 && num <= x;
                        if (flag3)
                        {
                            this.mapCompSeenFog.decrementSeen(faction, factionShownCells, num2 * x + num);
                        }
                    }
                }
                this.viewRect.maxX = -1;
                this.viewRect.minX = -1;
                this.viewRect.maxZ = -1;
                this.viewRect.minZ = -1;
            }
        }

        // Token: 0x06000071 RID: 113 RVA: 0x000089F8 File Offset: 0x00006BF8
        private void revealOccupiedCells()
        {
            bool flag = this.parent.Faction == Faction.OfPlayer;
            if (flag)
            {
                CellRect cellRect = this.parent.OccupiedRect();
                for (int i = cellRect.minX; i <= cellRect.maxX; i++)
                {
                    for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                    {
                        this.mapCompSeenFog.revealCell(j * this.mapSizeX + i);
                    }
                }
            }
        }

        // Token: 0x0400005E RID: 94
        private static readonly IntVec3 iv3Invalid = IntVec3.Invalid;

        // Token: 0x0400005F RID: 95
        private bool calculated;

        // Token: 0x04000060 RID: 96
        private IntVec3 lastPosition;

        // Token: 0x04000061 RID: 97
        public int lastSightRange;

        // Token: 0x04000062 RID: 98
        private IntVec3[] lastPeekDirections;

        // Token: 0x04000063 RID: 99
        private Faction lastFaction;

        // Token: 0x04000064 RID: 100
        private short[] lastFactionShownCells;

        // Token: 0x04000065 RID: 101
        private float baseViewRange;

        // Token: 0x04000066 RID: 102
        private bool[] viewMap1;

        // Token: 0x04000067 RID: 103
        private bool[] viewMap2;

        // Token: 0x04000068 RID: 104
        private CellRect viewRect;

        // Token: 0x04000069 RID: 105
        private bool viewMapSwitch = false;

        // Token: 0x0400006A RID: 106
        private IntVec3[] viewPositions;

        // Token: 0x0400006B RID: 107
        private Map map;

        // Token: 0x0400006C RID: 108
        private MapComponentSeenFog mapCompSeenFog;

        // Token: 0x0400006D RID: 109
        private ThingGrid thingGrid;

        // Token: 0x0400006E RID: 110
        private GlowGrid glowGrid;

        // Token: 0x0400006F RID: 111
        private RoofGrid roofGrid;

        // Token: 0x04000070 RID: 112
        private WeatherManager weatherManager;

        // Token: 0x04000071 RID: 113
        private int mapSizeX;

        // Token: 0x04000072 RID: 114
        private int mapSizeZ;

        // Token: 0x04000073 RID: 115
        private CompHiddenable compHiddenable;

        // Token: 0x04000074 RID: 116
        private CompGlower compGlower;

        // Token: 0x04000075 RID: 117
        private CompPowerTrader compPowerTrader;

        // Token: 0x04000076 RID: 118
        private CompRefuelable compRefuelable;

        // Token: 0x04000077 RID: 119
        private CompFlickable compFlickable;

        // Token: 0x04000078 RID: 120
        private CompMannable compMannable;

        // Token: 0x04000079 RID: 121
        private CompProvideVision compProvideVision;


        // Token: 0x0400007A RID: 122
        private bool setupDone = false;

        // Token: 0x0400007B RID: 123
        private Pawn pawn;

        // Token: 0x0400007C RID: 124
        private ThingDef def;

        // Token: 0x0400007D RID: 125
        private bool isMechanoid;

        // Token: 0x0400007E RID: 126
        private PawnCapacitiesHandler capacities;

        // Token: 0x0400007F RID: 127
        private Building building;

        // Token: 0x04000080 RID: 128
        private Building_TurretGun turret;

        // Token: 0x04000081 RID: 129
        private List<Hediff> hediffs;

        // Token: 0x04000082 RID: 130
        private Pawn_PathFollower pawnPather;

        // Token: 0x04000083 RID: 131
        private RaceProperties raceProps;

        // Token: 0x04000084 RID: 132
        private int lastMovementTick;

        // Token: 0x04000085 RID: 133
        private int lastPositionUpdateTick;

        // Token: 0x04000086 RID: 134
        private bool disabled;
    }
}