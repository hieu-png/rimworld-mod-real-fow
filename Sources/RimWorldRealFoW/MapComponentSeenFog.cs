using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000004 RID: 4
	public class MapComponentSeenFog : MapComponent
	{
		// Token: 0x06000008 RID: 8 RVA: 0x000023F4 File Offset: 0x000005F4
		public MapComponentSeenFog(Map map) : base(map)
		{
			this.mapCellLength = map.cellIndices.NumGridCells;
			this.mapSizeX = map.Size.x;
			this.mapSizeZ = map.Size.z;
			this.fogGrid = map.fogGrid;
			this.thingGrid = map.thingGrid;
			this.mapDrawer = map.mapDrawer;
			this.fowWatchers = new List<CompFieldOfViewWatcher>(1000);
			this.maxFactionLoadId = 0;
			foreach (Faction faction in Find.World.factionManager.AllFactionsListForReading)
			{
				this.maxFactionLoadId = Math.Max(this.maxFactionLoadId, faction.loadID);
			}
			this.factionsShownCells = new short[this.maxFactionLoadId + 1][];
			this.knownCells = new bool[this.mapCellLength];
			this.viewBlockerCells = new bool[this.mapCellLength];
			this.playerVisibilityChangeTick = new int[this.mapCellLength];
			this.mineDesignationGrid = new Designation[this.mapCellLength];
			this.idxToCellCache = new IntVec3[this.mapCellLength];
			this.compHideFromPlayerGrid = new List<CompHideFromPlayer>[this.mapCellLength];
			this.compHideFromPlayerGridCount = new byte[this.mapCellLength];
			this.compAffectVisionGrid = new List<CompAffectVision>[this.mapCellLength];
			for (int i = 0; i < this.mapCellLength; i++)
			{
				this.idxToCellCache[i] = CellIndicesUtility.IndexToCell(i, this.mapSizeX);
				this.compHideFromPlayerGrid[i] = new List<CompHideFromPlayer>(16);
				this.compHideFromPlayerGridCount[i] = 0;
				this.compAffectVisionGrid[i] = new List<CompAffectVision>(16);
				this.playerVisibilityChangeTick[i] = 0;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002608 File Offset: 0x00000808
		public override void MapComponentTick()
		{
			this.currentGameTick = Find.TickManager.TicksGame;
			bool flag = !this.initialized;
			if (flag)
			{
				this.initialized = true;
				this.init();
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002644 File Offset: 0x00000844
		public short[] getFactionShownCells(Faction faction)
		{
			bool flag = faction == null;
			short[] result;
			if (flag)
			{
				result = null;
			}
			else
			{
				bool flag2 = this.maxFactionLoadId < faction.loadID;
				if (flag2)
				{
					this.maxFactionLoadId = faction.loadID + 1;
					Array.Resize<short[]>(ref this.factionsShownCells, this.maxFactionLoadId + 1);
				}
				bool flag3 = this.factionsShownCells[faction.loadID] == null;
				if (flag3)
				{
					this.factionsShownCells[faction.loadID] = new short[this.mapCellLength];
				}
				result = this.factionsShownCells[faction.loadID];
			}
			return result;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000026D4 File Offset: 0x000008D4
		public bool isShown(Faction faction, IntVec3 cell)
		{
			return this.isShown(faction, cell.x, cell.z);
		}

		// Token: 0x0600000C RID: 12 RVA: 0x000026FC File Offset: 0x000008FC
		public bool isShown(Faction faction, int x, int z)
		{
			return this.getFactionShownCells(faction)[z * this.mapSizeX + x] != 0;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002724 File Offset: 0x00000924
		public void registerCompHideFromPlayerPosition(CompHideFromPlayer comp, int x, int z)
		{
			bool flag = x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ;
			if (flag)
			{
				int num = z * this.mapSizeX + x;
				this.compHideFromPlayerGrid[num].Add(comp);
				byte[] array = this.compHideFromPlayerGridCount;
				int num2 = num;
				array[num2] += 1;
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002784 File Offset: 0x00000984
		public void deregisterCompHideFromPlayerPosition(CompHideFromPlayer comp, int x, int z)
		{
			bool flag = x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ;
			if (flag)
			{
				int num = z * this.mapSizeX + x;
				this.compHideFromPlayerGrid[num].Remove(comp);
				byte[] array = this.compHideFromPlayerGridCount;
				int num2 = num;
				array[num2] -= 1;
			}
		}

		// Token: 0x0600000F RID: 15 RVA: 0x000027E4 File Offset: 0x000009E4
		public void registerCompAffectVisionPosition(CompAffectVision comp, int x, int z)
		{
			bool flag = x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ;
			if (flag)
			{
				this.compAffectVisionGrid[z * this.mapSizeX + x].Add(comp);
			}
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002830 File Offset: 0x00000A30
		public void deregisterCompAffectVisionPosition(CompAffectVision comp, int x, int z)
		{
			bool flag = x >= 0 && z >= 0 && x < this.mapSizeX && z < this.mapSizeZ;
			if (flag)
			{
				this.compAffectVisionGrid[z * this.mapSizeX + x].Remove(comp);
			}
		}

		// Token: 0x06000011 RID: 17 RVA: 0x0000287C File Offset: 0x00000A7C
		public void registerMineDesignation(Designation des)
		{
			IntVec3 cell = des.target.Cell;
			this.mineDesignationGrid[cell.z * this.mapSizeX + cell.x] = des;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000028B4 File Offset: 0x00000AB4
		public void deregisterMineDesignation(Designation des)
		{
			IntVec3 cell = des.target.Cell;
			this.mineDesignationGrid[cell.z * this.mapSizeX + cell.x] = null;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000028EC File Offset: 0x00000AEC
		private void init()
		{
			Section[,] array = (Section[,])Traverse.Create(this.mapDrawer).Field("sections").GetValue();
			this.sectionsSizeX = array.GetLength(0);
			this.sectionsSizeY = array.GetLength(1);
			this.sections = new Section[this.sectionsSizeX * this.sectionsSizeY];
			for (int i = 0; i < this.sectionsSizeY; i++)
			{
				for (int j = 0; j < this.sectionsSizeX; j++)
				{
					this.sections[i * this.sectionsSizeX + j] = array[j, i];
				}
			}
			List<Designation> allDesignations = this.map.designationManager.allDesignations;
			for (int k = 0; k < allDesignations.Count; k++)
			{
				Designation designation = allDesignations[k];
				bool flag = designation.def == DesignationDefOf.Mine && !designation.target.HasThing;
				if (flag)
				{
					this.registerMineDesignation(designation);
				}
			}
			bool flag2 = this.map.IsPlayerHome && this.map.mapPawns.ColonistsSpawnedCount == 0;
			if (flag2)
			{
				IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
				int radius = Mathf.RoundToInt(DefDatabase<RealFoWModDefaultsDef>.GetNamed(RealFoWModDefaultsDef.DEFAULT_DEF_NAME, true).baseViewRange);
				ShadowCaster.computeFieldOfViewWithShadowCasting(playerStartSpot.x, playerStartSpot.z, radius, this.viewBlockerCells, this.map.Size.x, this.map.Size.z, false, null, null, null, this.knownCells, 0, 0, this.mapSizeX, null, 0, 0, 0, 0, 0, byte.MaxValue, -1, -1);
				for (int l = 0; l < this.mapCellLength; l++)
				{
					bool flag3 = this.knownCells[l];
					if (flag3)
					{
						IntVec3 c = CellIndicesUtility.IndexToCell(l, this.mapSizeX);
						foreach (Thing this2 in this.map.thingGrid.ThingsListAtFast(c))
						{
							CompMainComponent compMainComponent = (CompMainComponent)this2.TryGetComp(CompMainComponent.COMP_DEF);
							bool flag4 = compMainComponent != null && compMainComponent.compHideFromPlayer != null;
							if (flag4)
							{
								compMainComponent.compHideFromPlayer.forceSeen();
							}
						}
					}
				}
			}
			foreach (Thing thing in this.map.listerThings.AllThings)
			{
				bool spawned = thing.Spawned;
				if (spawned)
				{
					CompMainComponent compMainComponent2 = (CompMainComponent)thing.TryGetComp(CompMainComponent.COMP_DEF);
					bool flag5 = compMainComponent2 != null;
					if (flag5)
					{
						bool flag6 = compMainComponent2.compComponentsPositionTracker != null;
						if (flag6)
						{
							compMainComponent2.compComponentsPositionTracker.updatePosition();
						}
						bool flag7 = compMainComponent2.compFieldOfViewWatcher != null;
						if (flag7)
						{
							compMainComponent2.compFieldOfViewWatcher.updateFoV(false);
						}
						bool flag8 = compMainComponent2.compHideFromPlayer != null;
						if (flag8)
						{
							compMainComponent2.compHideFromPlayer.updateVisibility(true, false);
						}
					}
				}
			}
			this.mapDrawer.RegenerateEverythingNow();
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00002C68 File Offset: 0x00000E68
		public override void ExposeData()
		{
			base.ExposeData();
			DataExposeUtility.BoolArray(ref this.knownCells, this.map.Size.x * this.map.Size.z, "revealedCells");
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00002CA4 File Offset: 0x00000EA4
		public void revealCell(int idx)
		{
			bool flag = !this.knownCells[idx];
			if (flag)
			{
				ref IntVec3 ptr = ref this.idxToCellCache[idx];
				this.knownCells[idx] = true;
				Designation designation = this.mineDesignationGrid[idx];
				bool flag2 = designation != null && ptr.GetFirstMineable(this.map) == null;
				if (flag2)
				{
					designation.Delete();
				}
				bool flag3 = this.initialized;
				if (flag3)
				{
					this.setMapMeshDirtyFlag(idx);
					this.map.fertilityGrid.Drawer.SetDirty();
					this.map.roofGrid.Drawer.SetDirty();
					this.map.terrainGrid.Drawer.SetDirty();
				}
				bool flag4 = this.compHideFromPlayerGridCount[idx] > 0;
				if (flag4)
				{
					List<CompHideFromPlayer> list = this.compHideFromPlayerGrid[idx];
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						list[i].updateVisibility(true, false);
					}
				}
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00002DB4 File Offset: 0x00000FB4
		public void incrementSeen(Faction faction, short[] factionShownCells, int idx)
		{
			short num = (short)(factionShownCells[idx] + 1);
			factionShownCells[idx] = num;
			bool flag = num == 1 && faction.def.isPlayer;
			if (flag)
			{
				ref IntVec3 ptr = ref this.idxToCellCache[idx];
				bool flag2 = !this.knownCells[idx];
				if (flag2)
				{
					this.knownCells[idx] = true;
					bool flag3 = this.initialized;
					if (flag3)
					{
						this.map.fertilityGrid.Drawer.SetDirty();
						this.map.roofGrid.Drawer.SetDirty();
						this.map.terrainGrid.Drawer.SetDirty();
					}
					Designation designation = this.mineDesignationGrid[idx];
					bool flag4 = designation != null && ptr.GetFirstMineable(this.map) == null;
					if (flag4)
					{
						designation.Delete();
					}
				}
				bool flag5 = this.initialized;
				if (flag5)
				{
					this.setMapMeshDirtyFlag(idx);
				}
				bool flag6 = this.compHideFromPlayerGridCount[idx] > 0;
				if (flag6)
				{
					List<CompHideFromPlayer> list = this.compHideFromPlayerGrid[idx];
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						list[i].updateVisibility(true, false);
					}
				}
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00002F04 File Offset: 0x00001104
		public void decrementSeen(Faction faction, short[] factionShownCells, int idx)
		{
			short num = (short)(factionShownCells[idx] - 1);
			factionShownCells[idx] = num;
			bool flag = num == 0 && faction.def.isPlayer;
			if (flag)
			{
				this.playerVisibilityChangeTick[idx] = this.currentGameTick;
				bool flag2 = this.initialized;
				if (flag2)
				{
					this.setMapMeshDirtyFlag(idx);
				}
				bool flag3 = this.compHideFromPlayerGridCount[idx] > 0;
				if (flag3)
				{
					List<CompHideFromPlayer> list = this.compHideFromPlayerGrid[idx];
					int count = list.Count;
					for (int i = 0; i < count; i++)
					{
						list[i].updateVisibility(true, false);
					}
				}
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00002FA8 File Offset: 0x000011A8
		private void setMapMeshDirtyFlag(int idx)
		{
			int num = idx % this.mapSizeX;
			int num2 = idx / this.mapSizeX;
			int num3 = num / 17;
			int num4 = num2 / 17;
			int num5 = Math.Max(0, num - 1);
			int num6 = Math.Min(num2 + 2, this.mapSizeZ);
			int num7 = Math.Min(num + 2, this.mapSizeX) - num5;
			for (int i = Math.Max(0, num2 - 1); i < num6; i++)
			{
				int num8 = i * this.mapSizeX + num5;
				for (int j = 0; j < num7; j++)
				{
					this.playerVisibilityChangeTick[num8 + j] = this.currentGameTick;
				}
			}
			this.sections[num4 * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
			int num9 = num % 17;
			int num10 = num2 % 17;
			bool flag = num9 == 0;
			if (flag)
			{
				bool flag2 = num3 != 0;
				if (flag2)
				{
					this.sections[num4 * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
					bool flag3 = num10 == 0;
					if (flag3)
					{
						bool flag4 = num4 != 0;
						if (flag4)
						{
							this.sections[(num4 - 1) * this.sectionsSizeX + (num3 - 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
						}
					}
					else
					{
						bool flag5 = num10 == 16;
						if (flag5)
						{
							bool flag6 = num4 < this.sectionsSizeY;
							if (flag6)
							{
								this.sections[(num4 + 1) * this.sectionsSizeX + (num3 - 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
							}
						}
					}
				}
			}
			else
			{
				bool flag7 = num9 == 16;
				if (flag7)
				{
					bool flag8 = num3 < this.sectionsSizeX;
					if (flag8)
					{
						this.sections[num4 * this.sectionsSizeX + (num3 + 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
						bool flag9 = num10 == 0;
						if (flag9)
						{
							bool flag10 = num4 != 0;
							if (flag10)
							{
								this.sections[(num4 - 1) * this.sectionsSizeX + (num3 + 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
							}
						}
						else
						{
							bool flag11 = num10 == 16;
							if (flag11)
							{
								bool flag12 = num4 < this.sectionsSizeY;
								if (flag12)
								{
									this.sections[(num4 + 1) * this.sectionsSizeX + (num3 + 1)].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
								}
							}
						}
					}
				}
			}
			bool flag13 = num10 == 0;
			if (flag13)
			{
				bool flag14 = num4 != 0;
				if (flag14)
				{
					this.sections[(num4 - 1) * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
				}
			}
			else
			{
				bool flag15 = num10 == 16;
				if (flag15)
				{
					bool flag16 = num4 < this.sectionsSizeY;
					if (flag16)
					{
						this.sections[(num4 + 1) * this.sectionsSizeX + num3].dirtyFlags |= SectionLayer_FoVLayer.mapMeshFlag;
					}
				}
			}
		}

		// Token: 0x04000005 RID: 5
		public short[][] factionsShownCells = null;

		// Token: 0x04000006 RID: 6
		public bool[] knownCells = null;

		// Token: 0x04000007 RID: 7
		public int[] playerVisibilityChangeTick = null;

		// Token: 0x04000008 RID: 8
		public bool[] viewBlockerCells = null;

		// Token: 0x04000009 RID: 9
		private IntVec3[] idxToCellCache;

		// Token: 0x0400000A RID: 10
		private List<CompHideFromPlayer>[] compHideFromPlayerGrid;

		// Token: 0x0400000B RID: 11
		private byte[] compHideFromPlayerGridCount;

		// Token: 0x0400000C RID: 12
		public List<CompAffectVision>[] compAffectVisionGrid;

		// Token: 0x0400000D RID: 13
		private Designation[] mineDesignationGrid;

		// Token: 0x0400000E RID: 14
		private int maxFactionLoadId;

		// Token: 0x0400000F RID: 15
		private int mapCellLength;

		// Token: 0x04000010 RID: 16
		public int mapSizeX;

		// Token: 0x04000011 RID: 17
		public int mapSizeZ;

		// Token: 0x04000012 RID: 18
		private FogGrid fogGrid;

		// Token: 0x04000013 RID: 19
		private MapDrawer mapDrawer;

		// Token: 0x04000014 RID: 20
		private ThingGrid thingGrid;

		// Token: 0x04000015 RID: 21
		public bool initialized = false;

		// Token: 0x04000016 RID: 22
		public List<CompFieldOfViewWatcher> fowWatchers;

		// Token: 0x04000017 RID: 23
		private Section[] sections = null;

		// Token: 0x04000018 RID: 24
		private int sectionsSizeX;

		// Token: 0x04000019 RID: 25
		private int sectionsSizeY;

		// Token: 0x0400001A RID: 26
		private int currentGameTick = 0;
	}
}
