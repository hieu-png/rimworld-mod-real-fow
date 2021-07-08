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
		// Token: 0x06000066 RID: 102 RVA: 0x00006B54 File Offset: 0x00004D54
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
			bool flag = this.pawn != null;
			if (flag)
			{
				this.raceProps = this.pawn.RaceProps;
				this.hediffs = this.pawn.health.hediffSet.hediffs;
				this.capacities = this.pawn.health.capacities;
				this.pawnPather = this.pawn.pather;
			}
			this.def = this.parent.def;
			bool flag2 = this.def.race != null;
			if (flag2)
			{
				this.isMechanoid = this.def.race.IsMechanoid;
			}
			else
			{
				this.isMechanoid = false;
			}
			RealFoWModDefaultsDef named = DefDatabase<RealFoWModDefaultsDef>.GetNamed(RealFoWModDefaultsDef.DEFAULT_DEF_NAME, true);
			bool flag3 = !this.isMechanoid;
			if (flag3)
			{
				this.baseViewRange = named.baseViewRange;
			}
			else
			{
				this.baseViewRange = Mathf.Round(named.baseViewRange * 1.25f);
			}
			bool flag4 = this.pawn == null && (this.turret == null || this.compMannable != null) && this.compProvideVision == null && this.building == null;
			if (flag4)
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
			bool flag = this.disabled;
			if (!flag)
			{
				int ticksGame = Find.TickManager.TicksGame;
				bool flag2 = this.pawn != null;
				if (flag2)
				{
					bool flag3 = this.pawnPather == null;
					if (flag3)
					{
						this.pawnPather = this.pawn.pather;
					}
					bool flag4 = this.pawnPather != null && this.pawnPather.Moving;
					if (flag4)
					{
						this.lastMovementTick = ticksGame;
					}
					bool flag5 = this.lastPosition != CompFieldOfViewWatcher.iv3Invalid && this.lastPosition != this.parent.Position;
					if (flag5)
					{
						this.lastPositionUpdateTick = ticksGame;
						this.updateFoV(false);
					}
					else
					{
						bool flag6 = (ticksGame - this.lastPositionUpdateTick) % 30 == 0;
						if (flag6)
						{
							this.updateFoV(false);
						}
					}
				}
				else
				{
					bool flag7 = (this.lastPosition != CompFieldOfViewWatcher.iv3Invalid && this.lastPosition != this.parent.Position) || ticksGame % 30 == 0;
					if (flag7)
					{
						this.updateFoV(false);
					}
				}
			}
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00006F34 File Offset: 0x00005134
		private void initMap()
		{
			bool flag = this.map != this.parent.Map;
			if (flag)
			{
				bool flag2 = this.map != null && this.lastFaction != null;
				if (flag2)
				{
					this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
				}
				bool flag3 = !this.disabled && this.mapCompSeenFog != null;
				if (flag3)
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
				bool flag4 = !this.disabled;
				if (flag4)
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
			bool flag = this.disabled || !this.setupDone || Current.ProgramState == ProgramState.MapInitializing;
			if (!flag)
			{
				ThingWithComps parent = this.parent;
				IntVec3 position = parent.Position;
				bool flag2 = parent != null && parent.Spawned && parent.Map != null && position != CompFieldOfViewWatcher.iv3Invalid;
				if (flag2)
				{
					this.initMap();
					Faction faction = parent.Faction;
					bool flag3 = faction != null && (this.pawn == null || !this.pawn.Dead);
					if (flag3)
					{
						bool flag4 = this.pawn != null;
						if (flag4)
						{
							IntVec3[] array = null;
							bool flag5 = this.raceProps != null && this.raceProps.Animal && (this.pawn.playerSettings == null || this.pawn.playerSettings.Master == null || this.pawn.training == null || !this.pawn.training.HasLearned(TrainableDefOf.Release));
							int num;
							if (flag5)
							{
								num = -1;
							}
							else
							{
								num = Mathf.RoundToInt(this.calcPawnSightRange(position, false, false));
								bool flag6 = (this.pawnPather == null || !this.pawnPather.Moving) && this.pawn.CurJob != null;
								if (flag6)
								{
									JobDef jobDef = this.pawn.CurJob.def;
									bool flag7 = jobDef == JobDefOf.AttackStatic || jobDef == JobDefOf.AttackMelee || jobDef == JobDefOf.Wait_Combat || jobDef == JobDefOf.Hunt;
									if (flag7)
									{
										array = GenAdj.CardinalDirections;
									}
									else
									{
										bool flag8 = jobDef == JobDefOf.Mine && this.pawn.CurJob.targetA != null && this.pawn.CurJob.targetA.Cell != IntVec3.Invalid;
										if (flag8)
										{
											array = FoWThingUtils.getPeekArray(this.pawn.CurJob.targetA.Cell - this.parent.Position);
										}
									}
								}
							}
							bool flag9 = !this.calculated || forceUpdate || faction != this.lastFaction || position != this.lastPosition || num != this.lastSightRange || array != this.lastPeekDirections;
							if (flag9)
							{
								this.calculated = true;
								this.lastPosition = position;
								this.lastSightRange = num;
								this.lastPeekDirections = array;
								bool flag10 = this.lastFaction != faction;
								if (flag10)
								{
									bool flag11 = this.lastFaction != null;
									if (flag11)
									{
										this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
									}
									this.lastFaction = faction;
									this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
								}
								bool flag12 = num != -1;
								if (flag12)
								{
									this.calculateFoV(parent, num, array);
								}
								else
								{
									this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
								}
							}
						}
						else
						{
							bool flag13 = this.turret != null && this.compMannable == null;
							if (flag13)
							{
								int num2 = Mathf.RoundToInt(this.turret.GunCompEq.PrimaryVerb.verbProps.range);
								bool flag14 = Find.Storyteller.difficulty.difficulty >= 4 || (this.compPowerTrader != null && !this.compPowerTrader.PowerOn) || (this.compRefuelable != null && !this.compRefuelable.HasFuel) || (this.compFlickable != null && !this.compFlickable.SwitchIsOn);
								if (flag14)
								{
									num2 = 0;
								}
								bool flag15 = !this.calculated || forceUpdate || faction != this.lastFaction || position != this.lastPosition || num2 != this.lastSightRange;
								if (flag15)
								{
									this.calculated = true;
									this.lastPosition = position;
									this.lastSightRange = num2;
									bool flag16 = this.lastFaction != faction;
									if (flag16)
									{
										bool flag17 = this.lastFaction != null;
										if (flag17)
										{
											this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
										}
										this.lastFaction = faction;
										this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
									}
									bool flag18 = num2 != 0;
									if (flag18)
									{
										this.calculateFoV(parent, num2, null);
									}
									else
									{
										this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
										this.revealOccupiedCells();
									}
								}
							}
							else
							{
								bool flag19 = this.compProvideVision != null;
								if (flag19)
								{
									int num3 = Mathf.RoundToInt(this.compProvideVision.Props.viewRadius);
									bool flag20 = (this.compPowerTrader != null && !this.compPowerTrader.PowerOn) || (this.compRefuelable != null && !this.compRefuelable.HasFuel) || (this.compFlickable != null && !this.compFlickable.SwitchIsOn);
									if (flag20)
									{
										num3 = 0;
									}
									bool flag21 = !this.calculated || forceUpdate || faction != this.lastFaction || position != this.lastPosition || num3 != this.lastSightRange;
									if (flag21)
									{
										this.calculated = true;
										this.lastPosition = position;
										this.lastSightRange = num3;
										bool flag22 = this.lastFaction != faction;
										if (flag22)
										{
											bool flag23 = this.lastFaction != null;
											if (flag23)
											{
												this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
											}
											this.lastFaction = faction;
											this.lastFactionShownCells = this.mapCompSeenFog.getFactionShownCells(faction);
										}
										bool flag24 = num3 != 0;
										if (flag24)
										{
											this.calculateFoV(parent, num3, null);
										}
										else
										{
											this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
											this.revealOccupiedCells();
										}
									}
								}
								else
								{
									bool flag25 = this.building != null;
									if (flag25)
									{
										int num4 = 0;
										bool flag26 = !this.calculated || forceUpdate || faction != this.lastFaction || position != this.lastPosition || num4 != this.lastSightRange;
										if (flag26)
										{
											this.calculated = true;
											this.lastPosition = position;
											this.lastSightRange = num4;
											bool flag27 = this.lastFaction != faction;
											if (flag27)
											{
												bool flag28 = this.lastFaction != null;
												if (flag28)
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
							}
						}
					}
					else
					{
						bool flag29 = faction != this.lastFaction;
						if (flag29)
						{
							bool flag30 = this.lastFaction != null;
							if (flag30)
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

		// Token: 0x0600006C RID: 108 RVA: 0x000077F0 File Offset: 0x000059F0
		public float calcPawnSightRange(IntVec3 position, bool forTargeting, bool shouldMove)
		{
			bool flag = this.pawn == null;
			float result;
			if (flag)
			{
				Log.Error("calcPawnSightRange performed on non pawn thing", false);
				result = 0f;
			}
			else
			{
				float num = 0f;
				this.initMap();
				bool flag2 = !this.isMechanoid && this.pawn.CurJob != null && this.pawn.jobs.curDriver.asleep;
				bool flag3 = !shouldMove && !flag2 && (this.pawnPather == null || !this.pawnPather.Moving);
				if (flag3)
				{
					Verb verb = null;
					bool flag4 = this.pawn.CurJob != null;
					if (flag4)
					{
						JobDef jobDef = this.pawn.CurJob.def;
						bool flag5 = jobDef == JobDefOf.ManTurret;
						if (flag5)
						{
							Building_Turret building_Turret = this.pawn.CurJob.targetA.Thing as Building_Turret;
							bool flag6 = building_Turret != null;
							if (flag6)
							{
								verb = building_Turret.AttackVerb;
							}
						}
						else
						{
							bool flag7 = jobDef == JobDefOf.AttackStatic || jobDef == JobDefOf.AttackMelee || jobDef == JobDefOf.Wait_Combat || jobDef == JobDefOf.Hunt;
							if (flag7)
							{
								bool flag8 = this.pawn.equipment != null;
								if (flag8)
								{
									ThingWithComps primary = this.pawn.equipment.Primary;
									bool flag9 = primary != null && primary.def.IsRangedWeapon;
									if (flag9)
									{
										verb = primary.GetComp<CompEquippable>().PrimaryVerb;
									}
								}
							}
						}
					}
					bool flag10 = verb != null && verb.verbProps.range > this.baseViewRange && verb.verbProps.requireLineOfSight && verb.EquipmentSource.def.IsRangedWeapon;
					if (flag10)
					{
						float range = verb.verbProps.range;
						bool flag11 = this.baseViewRange < range;
						if (flag11)
						{
							int num2 = Find.TickManager.TicksGame - this.lastMovementTick;
							float statValue = this.pawn.GetStatValue(StatDefOf.AimingDelayFactor, true);
							int num3 = (verb.verbProps.warmupTime * statValue).SecondsToTicks() * Mathf.RoundToInt((range - this.baseViewRange) / 2f);
							bool flag12 = num2 >= num3;
							if (flag12)
							{
								num = range * this.capacities.GetLevel(PawnCapacityDefOf.Sight);
							}
							else
							{
								int num4 = Mathf.RoundToInt((range - this.baseViewRange) * ((float)num2 / (float)num3));
								num = (this.baseViewRange + (float)num4) * this.capacities.GetLevel(PawnCapacityDefOf.Sight);
							}
						}
					}
				}
				bool flag13 = num == 0f;
				if (flag13)
				{
					num = this.baseViewRange * this.capacities.GetLevel(PawnCapacityDefOf.Sight);
				}
				bool flag14 = !forTargeting && flag2;
				if (flag14)
				{
					num *= 0.2f;
				}
				List<CompAffectVision> list = this.mapCompSeenFog.compAffectVisionGrid[position.z * this.mapSizeX + position.x];
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					num *= list[i].Props.fovMultiplier;
				}
				bool flag15 = !this.isMechanoid;
				if (flag15)
				{
					float num5 = this.glowGrid.GameGlowAt(position, false);
					bool flag16 = num5 != 1f;
					if (flag16)
					{
						float num6 = 0.6f;
						int count2 = this.hediffs.Count;
						for (int j = 0; j < count2; j++)
						{
							bool flag17 = this.hediffs[j].def == HediffDefOf.BionicEye;
							if (flag17)
							{
								num6 += 0.2f;
							}
						}
						bool flag18 = num6 < 1f;
						if (flag18)
						{
							num *= Mathf.Lerp(num6, 1f, num5);
						}
					}
					bool flag19 = !this.roofGrid.Roofed(position.x, position.z);
					if (flag19)
					{
						float curWeatherAccuracyMultiplier = this.weatherManager.CurWeatherAccuracyMultiplier;
						bool flag20 = curWeatherAccuracyMultiplier != 1f;
						if (flag20)
						{
							num *= Mathf.Lerp(0.5f, 1f, curWeatherAccuracyMultiplier);
						}
					}
				}
				bool flag21 = num < 1f;
				if (flag21)
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
			bool flag = !this.disabled && this.mapCompSeenFog != null;
			if (flag)
			{
				this.mapCompSeenFog.fowWatchers.Remove(this);
			}
			bool flag2 = this.lastFaction != null;
			if (flag2)
			{
				this.unseeSeenCells(this.lastFaction, this.lastFactionShownCells);
			}
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00007CB8 File Offset: 0x00005EB8
		public void calculateFoV(Thing thing, int intRadius, IntVec3[] peekDirections)
		{
			bool flag = !this.setupDone;
			if (!flag)
			{
				int num = this.mapSizeX;
				int num2 = this.mapSizeZ;
				bool[] array = this.viewMapSwitch ? this.viewMap1 : this.viewMap2;
				bool[] array2 = this.viewMapSwitch ? this.viewMap2 : this.viewMap1;
				IntVec3 position = thing.Position;
				Faction faction = this.lastFaction;
				short[] factionShownCells = this.lastFactionShownCells;
				int num3 = (peekDirections != null) ? (intRadius + 1) : intRadius;
				CellRect cellRect = thing.OccupiedRect();
				int num4 = Math.Min(position.x - num3, cellRect.minX);
				int num5 = Math.Max(position.x + num3, cellRect.maxX);
				int num6 = Math.Min(position.z - num3, cellRect.minZ);
				int num7 = Math.Max(position.z + num3, cellRect.maxZ);
				int num8 = num5 - num4 + 1;
				int num9 = num8 * (num7 - num6 + 1);
				int minZ = this.viewRect.minZ;
				int maxZ = this.viewRect.maxZ;
				int minX = this.viewRect.minX;
				int maxX = this.viewRect.maxX;
				int width = this.viewRect.Width;
				int area = this.viewRect.Area;
				bool flag2 = array2 == null || array2.Length < num9;
				if (flag2)
				{
					array2 = new bool[(int)((float)num9 * 1.2f)];
					bool flag3 = this.viewMapSwitch;
					if (flag3)
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
						array2[(j - num6) * num8 + (i - num4)] = true;
						bool flag4 = array == null || i < minX || j < minZ || i > maxX || j > maxZ;
						if (flag4)
						{
							this.mapCompSeenFog.incrementSeen(faction, factionShownCells, j * num + i);
						}
						else
						{
							int num10 = (j - minZ) * width + (i - minX);
							ref bool ptr = ref array[num10];
							bool flag5 = !ptr;
							if (flag5)
							{
								this.mapCompSeenFog.incrementSeen(faction, factionShownCells, j * num + i);
							}
							else
							{
								ptr = false;
							}
						}
					}
				}
				bool flag6 = intRadius > 0;
				if (flag6)
				{
					bool[] viewBlockerCells = this.mapCompSeenFog.viewBlockerCells;
					this.viewPositions[0] = position;
					bool flag7 = peekDirections == null;
					int num11;
					if (flag7)
					{
						num11 = 1;
					}
					else
					{
						num11 = 1 + peekDirections.Length;
						for (int k = 0; k < num11 - 1; k++)
						{
							this.viewPositions[1 + k] = position + peekDirections[k];
						}
					}
					int num12 = this.map.Size.x - 1;
					int num13 = this.map.Size.z - 1;
					for (int l = 0; l < num11; l++)
					{
						ref IntVec3 ptr2 = ref this.viewPositions[l];
						bool flag8 = ptr2.x >= 0 && ptr2.z >= 0 && ptr2.x <= num12 && ptr2.z <= num13 && (l == 0 || ptr2.IsInside(thing) || !viewBlockerCells[ptr2.z * num + ptr2.x]);
						if (flag8)
						{
							ShadowCaster.computeFieldOfViewWithShadowCasting(ptr2.x, ptr2.z, intRadius, viewBlockerCells, num, num2, true, this.mapCompSeenFog, faction, factionShownCells, array2, num4, num6, num8, array, minX, maxX, minZ, maxZ, width, byte.MaxValue, -1, -1);
						}
					}
				}
				bool flag9 = array != null;
				if (flag9)
				{
					for (int m = 0; m < area; m++)
					{
						ref bool ptr3 = ref array[m];
						bool flag10 = ptr3;
						if (flag10)
						{
							ptr3 = false;
							int num14 = minX + m % width;
							int num15 = minZ + m / width;
							bool flag11 = num15 >= 0 && num15 <= num2 && num14 >= 0 && num14 <= num;
							if (flag11)
							{
								this.mapCompSeenFog.decrementSeen(faction, factionShownCells, num15 * num + num14);
							}
						}
					}
				}
				this.viewMapSwitch = !this.viewMapSwitch;
				this.viewRect.maxX = num5;
				this.viewRect.minX = num4;
				this.viewRect.maxZ = num7;
				this.viewRect.minZ = num6;
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
					int num4;
					if (flag5)
					{
						num4 = 1;
					}
					else
					{
						num4 = 1 + array3.Length;
						for (int k = 0; k < num4 - 1; k++)
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
					for (int l = 0; l < num4; l++)
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
					for (int m = 0; m < num4; m++)
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
