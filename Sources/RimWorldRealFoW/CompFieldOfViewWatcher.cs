﻿using System;
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
        
        enum ThingType
        {
            turret,
            building,
            visionProvider,
            pawn,
            animal
        }

        ThingType thingType;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {

            this.setupDone = true;
            this.calculated = false;
            this.lastPosition = CompFieldOfViewWatcher.iv3Invalid;
            this.lastSightRange = 0;
            this.lastPeekDirections = null;
            this.lastOnTower = false;
            this.viewMap1 = null;
            this.viewMap2 = null;
            this.viewRect = new CellRect(-1, -1, 0, 0);
            this.viewPositions = new IntVec3[5];
            this.compHiddenable = this.mainComponent.compHiddenable;
            // this.attackVerbRange = new Dictionary<Verb, float>();

            this.compGlower = this.parent.GetComp<CompGlower>();
            this.compPowerTrader = this.parent.GetComp<CompPowerTrader>();
            this.compRefuelable = this.parent.GetComp<CompRefuelable>();
            this.compFlickable = this.parent.GetComp<CompFlickable>();
            this.compMannable = this.parent.GetComp<CompMannable>();
            this.compProvideVision = this.parent.GetComp<CompProvideVision>();

            this.pawn = (this.parent as Pawn);
            this.building = (this.parent as Building);
            this.turret = (this.parent as Building_TurretGun);
            this.disabled = false;

            if (this.pawn != null)
            {
                this.raceProps = this.pawn.RaceProps;
                this.hediffs = this.pawn.health.hediffSet.hediffs;
                this.capacities = this.pawn.health.capacities;
                this.pawnPather = this.pawn.pather;

                if (this.raceProps.Animal)
                {
                    thingType = ThingType.animal;
                }
                else
                {
                    thingType = ThingType.pawn;
                }

                //this.def = this.parent.def;

                this.dayVisionEffectiveness = pawn.GetStatValue(FoWDef.DayVisionEffectiveness, false);
                this.nightVisionEffectiveness = pawn.GetStatValue(FoWDef.NightVisionEffectiveness, true);
                this.baseViewRange = RFOWSettings.baseViewRange * dayVisionEffectiveness;


            }
            else if (this.turret != null && this.compMannable == null)
            {
                thingType = ThingType.turret;
            }
            else if (this.compProvideVision != null)
            {
                thingType = ThingType.visionProvider;
                this.building.def.specialDisplayRadius = compProvideVision.Props.viewRadius * RFOWSettings.buildingVisionModifier - 0.1f;

            }
            else if (this.building != null)
            {
                thingType = ThingType.building;
            }
            else
            {
                Log.Message("Removing unneeded FoV watcher from " + this.parent.ThingID);
                this.disabled = true;
                this.mainComponent.compFieldOfViewWatcher = null;
            }

            this.initMap();
            this.lastMovementTick = Find.TickManager.TicksGame;
            this.lastPositionUpdateTick = this.lastMovementTick;
                this.updateFoV();
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look<int>(ref this.lastMovementTick, "fovLastMovementTick", Find.TickManager.TicksGame, false);
        }

        public override void ReceiveCompSignal(string signal)
        {
            this.updateFoV();
        }

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
                    if (RFOWSettings.baseHearingRange > 0)
                    {
                        float hearing = capacities.GetLevel(PawnCapacityDefOf.Hearing);
                        if (hearing > 0
                            && pawn.Faction == Faction.OfPlayer
                            && thingType == ThingType.pawn)
                        {
                            if(ticksGame % 100 == 0)
                            livePawnHear(pawn, pawn.Faction);
                            if (ticksGame % 200 == 0)
                            {
                                updateNearbyPawn(
                                pawn,
                                RFOWSettings.baseHearingRange,
                                capacities.GetLevel(PawnCapacityDefOf.Hearing));
                            }
                        }

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

                    if (this.lastPosition != CompFieldOfViewWatcher.iv3Invalid
                        && this.lastPosition != this.parent.Position || ticksGame % 30 == 0)
                    {
                        this.updateFoV();
                    }

                }
            }
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
        }

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
                        if (thingType == ThingType.pawn)
                        {
                            if (RFOWSettings.prisonerGiveVision && pawn.IsPrisonerOfColony)
                            {
                                livePawnCalculateFov(position, 0.2f, forceUpdate, Faction.OfPlayer);
                            }
                            else
                                livePawnCalculateFov(position, 1, forceUpdate, faction);

                        }
                        else if (thingType == ThingType.animal)
                        {
                            if ((this.pawn.playerSettings == null
                                || this.pawn.playerSettings.Master == null
                                || this.pawn.training == null
                                || !this.pawn.training.HasLearned(TrainableDefOf.Release)))
                            {
                                livePawnCalculateFov(position,
                                0,
                                forceUpdate,
                                faction);
                            }
                            else
                            {
                                livePawnCalculateFov(
                                    position,
                                    RFOWSettings.animalVisionModifier * Math.Max(raceProps.baseBodySize * 0.7f, 0.4f),
                                    forceUpdate,
                                    faction);
                            }
                        }
                        else if (thingType == ThingType.turret)
                        {
                            //Turret is more sensor based so reduced vision range, still can feed back some info

                            int sightRange = Mathf.RoundToInt(this.turret.GunCompEq.PrimaryVerb.verbProps.range * RFOWSettings.turretVisionModifier);

                            if ((this.compPowerTrader != null && !this.compPowerTrader.PowerOn)
                                || (this.compRefuelable != null && !this.compRefuelable.HasFuel)
                                || (this.compFlickable != null && !this.compFlickable.SwitchIsOn)
                                //|| !this.mapCompSeenFog.workingCameraConsole
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
                        else if (thingType == ThingType.visionProvider)
                        {
                            int viewRadius = Mathf.RoundToInt(this.compProvideVision.Props.viewRadius * RFOWSettings.buildingVisionModifier);
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
                        else if (thingType == ThingType.building)
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
                            disabled = true;
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
            if (this.pawn == null)
            {

                Log.Message("calcPawnSightRange performed on non pawn thing");
                return 0f;
            }
            float viewRange = baseViewRange;
            //float sightCapacity = ;
            this.initMap();
            float gameTick = Find.TickManager.TicksGame;

            List<CompAffectVision> visionAffectingBuilding = this.mapCompSeenFog.compAffectVisionGrid[position.z * this.mapSizeX + position.x];
            bool ignoreWeather = false;
            bool ignoreDarkness = false;
            bool sleeping = this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep;

            if (!forTargeting && sleeping)
            {
                viewRange = 8f * capacities.GetLevel(PawnCapacityDefOf.Hearing);
            }
            else
            {
                if (!shouldMove && (this.pawnPather == null || !this.pawnPather.Moving))
                {
                    Verb attackVerb = null;
                    if (this.pawn.CurJob != null)
                    {
                        JobDef jobDef = this.pawn.CurJob.def;

                        //Get manned turret sight range.
                        if (jobDef == JobDefOf.ManTurret)
                        {
                            Building_Turret building_Turret = this.pawn.CurJob.targetA.Thing as Building_Turret;
                            if (building_Turret != null)
                            {
                                attackVerb = building_Turret.AttackVerb;
                            }
                        }
                        else
                        {
                            //Standing still increase view range
                            if (jobDef == JobDefOf.AttackStatic
                                || jobDef == JobDefOf.AttackMelee
                                || jobDef == JobDefOf.Wait_Combat
                                || jobDef == JobDefOf.Hunt)
                            {
                                if (this.pawn.equipment != null)
                                {
                                    ThingWithComps primary = this.pawn.equipment.Primary;
                                    if (primary != null && primary.def.IsRangedWeapon)
                                    {
                                        attackVerb = primary.GetComp<CompEquippable>().PrimaryVerb;
                                    }
                                }
                            }
                        }
                    }
                    if (attackVerb != null)

                        if (attackVerb.verbProps.range > viewRange
                        && attackVerb.verbProps.requireLineOfSight
                        && attackVerb.EquipmentSource.def.IsRangedWeapon)
                        {
                            viewRange = attackVerb.verbProps.range;

                        }
                }

                float rangeModifier = this.capacities.GetLevel(PawnCapacityDefOf.Sight);
                foreach (CompAffectVision visionAffecter in visionAffectingBuilding)
                {
                    if (visionAffecter.Props.denyDarkness)
                        ignoreDarkness = true;
                    if (visionAffecter.Props.denyWeather)
                        ignoreWeather = true;
                    rangeModifier *= visionAffecter.Props.fovMultiplier;
                }

                float currGlow = this.glowGrid.GameGlowAt(position, false);
                if (gameTick % 40 == 0)
                {
                    this.nightVisionEffectiveness = pawn.GetStatValue(FoWDef.NightVisionEffectiveness, true);
                }
                if (currGlow < 1)
                {

                    if (nightVisionEffectiveness < 1)
                    {
                        if (!ignoreDarkness)
                        {
                            rangeModifier *= Mathf.Lerp(nightVisionEffectiveness, 1f, currGlow);
                        }
                    }
                    else
                    {
                        rangeModifier *= nightVisionEffectiveness;
                    }
                }

                if (!this.roofGrid.Roofed(position.x, position.z) && !ignoreWeather)
                {
                    float curWeatherAccuracyMultiplier = this.weatherManager.CurWeatherAccuracyMultiplier;
                    if (curWeatherAccuracyMultiplier != 1f)
                    {
                        rangeModifier *= Mathf.Lerp(0.5f, 1f, curWeatherAccuracyMultiplier);
                    }
                }

                viewRange *= rangeModifier;
            }


            if (viewRange < 1f)
            {
                return 8f * capacities.GetLevel(PawnCapacityDefOf.Hearing);
            }
            else
            {
                return viewRange;
            }

        }

        public bool OnTower(IntVec3 position)
        {
            int posIndex = (position.z * mapSizeX) + position.x;
            return OnTower(posIndex);
        }

        public bool OnTower(int index)
        {
            List<CompAffectVision> compsAffectVision = mapCompSeenFog.compAffectVisionGrid[index];
            int compsCount = compsAffectVision.Count;
            for (int i = 0; i < compsCount; i++)
            {
                return true;
            }

            return false;
        }

        public bool IsUnderRoof(IntVec3 position)
        {
            return map.roofGrid.Roofed(position);
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
        public void calculateFoV(Thing thing, int intRadius, IntVec3[] peekDirections, bool onTower = false)
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
                            int oldViewRecInx = (occupiedZ - oldViewRecMinZ) * oldViewWidth + (occupiedX - oldViewRecMinX);
                            ref bool oldViewMapValue = ref oldMapView[oldViewRecInx];
                            if (!oldViewMapValue)
                            {
                                this.mapCompSeenFog.incrementSeen(faction, factionShownCells, occupiedZ * mapSizeX + occupiedX);
                            }
                            else
                            {
                                oldViewMapValue = false;
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
                    int mapWidth = this.map.Size.x - 1;
                    int mapHeight = this.map.Size.z - 1;
                    for (int l = 0; l < viewPositionCount; l++)
                    {
                        ref IntVec3 ptr2 = ref this.viewPositions[l];
                        if (
                        ptr2.x >= 0
                        && ptr2.z >= 0
                        && ptr2.x <= mapWidth
                        && ptr2.z <= mapHeight
                        && (l == 0 || ptr2.IsInside(thing) || (!viewBlockerCells[ptr2.z * mapSizeX + ptr2.x] && !onTower)))
                        {
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, intRadius, viewBlockerCells, mapSizeX, mapSizeY, true, this.mapCompSeenFog, faction, factionShownCells, newMapView, newViewRecMinX, newViewRecMinZ, newViewWidth, oldMapView, oldViewRecMinX, oldViewRecMaxX, oldViewRecMinZ, oldViewRecMaxZ, oldViewWidth, byte.MaxValue, -1, -1, onTower, map.roofGrid);
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

        public void refreshFovTarget(ref IntVec3 targetPos)
        {
            if (!setupDone)
            {
                return;
            }

            Thing parent = this.parent;

            bool[] oldViewMap = this.viewMapSwitch ? this.viewMap1 : this.viewMap2;
            bool[] newViewMap = this.viewMapSwitch ? this.viewMap2 : this.viewMap1;
            if (oldViewMap == null || this.lastPosition != this.parent.Position)
            {
                this.updateFoV(true);
            }
            else
            {
                int radius = this.lastSightRange;
                IntVec3[] peekDirection = this.lastPeekDirections;
                bool onTower = lastOnTower;

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

                if (newViewMap == null || newViewMap.Length < area)
                {
                    newViewMap = new bool[(int)((float)area * 1.2f)];
                    if (this.viewMapSwitch)
                    {
                        this.viewMap2 = newViewMap;
                    }
                    else
                    {
                        this.viewMap1 = newViewMap;
                    }
                }

                for (int i = cellRect.minX; i <= cellRect.maxX; i++)
                {
                    for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
                    {
                        int num3 = (j - minZ) * width + (i - minX);
                        newViewMap[num3] = true;
                        oldViewMap[num3] = false;
                    }
                }
                bool[] viewBlockerCells = this.mapCompSeenFog.viewBlockerCells;
                this.viewPositions[0] = position;
                int sightRange;
                if (peekDirection == null)
                {
                    sightRange = 1;
                }
                else
                {
                    sightRange = 1 + peekDirection.Length;
                    for (int k = 0; k < sightRange - 1; k++)
                    {
                        this.viewPositions[1 + k] = position + peekDirection[k];
                    }
                }
                int num5 = this.map.Size.x - 1;
                int num6 = this.map.Size.z - 1;
                bool q1Updated = false;
                bool q2Updated = false;
                bool q3Updated = false;
                bool q4Updated = false;
                for (int l = 0; l < sightRange; l++)
                {
                    ref IntVec3 ptr = ref this.viewPositions[l];
                    if (
                    ptr.x >= 0 && ptr.z >= 0
                    && ptr.x <= num5 && ptr.z <= num6
                    && (l == 0 || ptr.IsInside(parent) || (!viewBlockerCells[ptr.z * num + ptr.x] && !onTower)))
                    {
                        if (ptr.x <= targetPos.x)
                        {
                            if (ptr.z <= targetPos.z)
                                q1Updated = true;
                            else
                                q4Updated = true;
                        }
                        else
                        {
                            if (ptr.z <= targetPos.z)
                            {
                                q2Updated = true;
                            }
                            else
                            {
                                q3Updated = true;
                            }
                        }
                    }
                }
                for (int m = 0; m < sightRange; m++)
                {
                    ref IntVec3 ptr2 = ref this.viewPositions[m];
                    bool flag14 = ptr2.x >= 0 && ptr2.z >= 0 && ptr2.x <= num5 && ptr2.z <= num6 && (m == 0 || ptr2.IsInside(parent) || (!viewBlockerCells[ptr2.z * num + ptr2.x] && !onTower));
                    if (flag14)
                    {
                        bool flag15 = q1Updated;
                        if (flag15)
                        {
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 0, -1, -1, onTower, map.roofGrid);
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 1, -1, -1, onTower, map.roofGrid);
                        }
                        bool flag16 = q2Updated;
                        if (flag16)
                        {
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 2, -1, -1, onTower, map.roofGrid);
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 3, -1, -1, onTower, map.roofGrid);
                        }
                        bool flag17 = q3Updated;
                        if (flag17)
                        {
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 4, -1, -1, onTower, map.roofGrid);
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 5, -1, -1, onTower, map.roofGrid);
                        }
                        bool flag18 = q4Updated;
                        if (flag18)
                        {
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 6, -1, -1, onTower, map.roofGrid);
                            ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, radius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, newViewMap, minX, minZ, width, oldViewMap, minX, maxX, minZ, maxZ, width, 7, -1, -1, onTower, map.roofGrid);
                        }
                    }
                }
                for (int n = 0; n < area; n++)
                {
                    ref bool ptr3 = ref oldViewMap[n];
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
                        bool flag23 = (!q1Updated && b == 1) || (!q2Updated && b == 2) || (!q3Updated && b == 3) || (!q4Updated && b == 4);
                        if (flag23)
                        {
                            newViewMap[n] = true;
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
        public void updateNearbyPawn(Pawn thisPawn, float range, float rangeMod)
        {
            if (thisPawn.Map != null)
            {
                //nearByPawn.Clear();
                nearByPawn = MapUtils.GetPawnsAround(thisPawn.Position,(int)(range*rangeMod), thisPawn.Map) as List<Pawn>;
               // foreach (Thing thing in GenRadial.RadialDistinctThingsAround(
               //     thisPawn.Position, thisPawn.Map, range * rangeMod, true))
               /*foreach(Pawn other in MapUtils.GetPawnsAround(thisPawn.Position,(int)( range * rangeMod), thisPawn.Map))
                {
                    //Pawn other = thing as Pawn;
                    if (other != null) {
                        nearByPawn.Add(other);
                        }
                }
                /*
                this
                foreach(Pawn other in thisPawn.Map.mapPawns.AllPawnsSpawned) {
                    if(other.Position.DistanceTo(thisPawn.Position) < range * rangeMod) {
                        nearByPawn.Add(other);
                    }
                }*/
            } else nearByPawn.Clear();
        }
        private void livePawnHear(
            Pawn thisPawn,
            Faction faction
            )
        {
            foreach (Pawn other in nearByPawn)
            {
                //Pawn other = thing as Pawn;
                // if (other != null)
                if (
                    other.Faction != faction
                    && (other.pather != null && other.pather.Moving)
                    && !this.mapCompSeenFog.isShown(faction, other.Position)
                    )
                {
                    float otherSize = other.BodySize;
                    // MoteMaker.MakeWaterSplash(other.Position.ToVector3(), this.map, other.BodySize, 2);
                    MapUtils.MakeSoundWave(
                        other.Position.ToVector3() + new Vector3(otherSize * 0.5f, 0, otherSize * 0.5f),
                        this.map,
                        Mathf.Lerp(1.5f, 3.5f, otherSize / 4),
                        Mathf.Lerp(1f, 2.5f, otherSize / 4));

                }

            }

        }


        private void livePawnCalculateFov(
            IntVec3 position,
            float sightRangeMod,
            bool forceUpdate,
            Faction faction
            )

        {
            IntVec3[] peekDirection = null;
            int sightRange = -1;
            bool onTower = OnTower(position);

            if (sightRangeMod != 0)
                sightRange = Mathf.RoundToInt(sightRangeMod * this.calcPawnSightRange(position, false, false));
            if (sightRange != -1)
            {
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

                if (!this.calculated
                    || forceUpdate
                    || faction != this.lastFaction
                    || position != this.lastPosition
                    || sightRange != this.lastSightRange
                    || onTower != this.lastOnTower
                    || peekDirection != this.lastPeekDirections)
                {
                    this.calculated = true;
                    this.lastPosition = position;
                    this.lastSightRange = sightRange;
                    this.lastOnTower = onTower;
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
                    this.calculateFoV(parent, sightRange, peekDirection, onTower);
                }
            }
            else
            {
                this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
            }
        }
        private static readonly IntVec3 iv3Invalid = IntVec3.Invalid;

        private bool calculated;

        private IntVec3 lastPosition;

        public int lastSightRange;

        private IntVec3[] lastPeekDirections;

        private Faction lastFaction;

        private short[] lastFactionShownCells;

        private bool lastOnTower;

        private float baseViewRange;

        private bool[] viewMap1;

        private bool[] viewMap2;

        private CellRect viewRect;

        private bool viewMapSwitch = false;

        private IntVec3[] viewPositions;

        private Map map;

        private MapComponentSeenFog mapCompSeenFog;

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

        private CompMannable compMannable;

        private CompProvideVision compProvideVision;

        public List<Pawn> nearByPawn = new List<Pawn>();
        private bool setupDone = false;

        private Pawn pawn;

        private float hearingTickFrequency = 120;
        private float dayVisionEffectiveness;

        private float nightVisionEffectiveness;
        //private ThingDef def;


        private PawnCapacitiesHandler capacities;

        private Building building;

        private Building_TurretGun turret;

        private List<Hediff> hediffs;

        private Pawn_PathFollower pawnPather;

        private RaceProperties raceProps;

        private int lastMovementTick;

        private int lastPositionUpdateTick;

        private bool disabled;
    }
}