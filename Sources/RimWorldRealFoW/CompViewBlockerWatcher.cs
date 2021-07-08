using System;
using System.Collections.Generic;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000011 RID: 17
	public class CompViewBlockerWatcher : ThingSubComp
	{
		// Token: 0x06000052 RID: 82 RVA: 0x000060BC File Offset: 0x000042BC
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			this.blockLight = this.parent.def.blockLight;
			this.b = (this.parent as Building);
			bool flag = this.blockLight && this.b != null;
			if (flag)
			{
				this.updateIsViewBlocker();
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x0000611C File Offset: 0x0000431C
		public override void ReceiveCompSignal(string signal)
		{
			base.ReceiveCompSignal(signal);
			bool flag = this.blockLight && this.b != null;
			if (flag)
			{
				this.updateIsViewBlocker();
			}
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00006154 File Offset: 0x00004354
		public override void CompTick()
		{
			base.CompTick();
			bool flag = this.blockLight && this.b != null && Find.TickManager.TicksGame % 30 == 0;
			if (flag)
			{
				this.updateIsViewBlocker();
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x0000619C File Offset: 0x0000439C
		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			bool flag = this.lastIsViewBlocker;
			if (flag)
			{
				bool flag2 = this.map != map;
				if (flag2)
				{
					this.map = map;
					this.mapCompSeenFog = map.getMapComponentSeenFog();
				}
				this.updateViewBlockerCells(false);
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x000061EC File Offset: 0x000043EC
		private void updateIsViewBlocker()
		{
			bool flag = this.blockLight && !this.b.CanBeSeenOver();
			bool flag2 = this.lastIsViewBlocker != flag;
			if (flag2)
			{
				this.lastIsViewBlocker = flag;
				bool flag3 = this.map != this.parent.Map;
				if (flag3)
				{
					this.map = this.parent.Map;
					this.mapCompSeenFog = this.map.getMapComponentSeenFog();
				}
				this.updateViewBlockerCells(flag);
			}
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00006274 File Offset: 0x00004474
		private void updateViewBlockerCells(bool blockView)
		{
			bool[] viewBlockerCells = this.mapCompSeenFog.viewBlockerCells;
			int z = this.map.Size.z;
			int x = this.map.Size.x;
			CellRect cellRect = this.parent.OccupiedRect();
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					bool flag = i >= 0 && j >= 0 && i <= x && j <= z;
					if (flag)
					{
						viewBlockerCells[j * z + i] = blockView;
					}
				}
			}
			IntVec3 position = this.parent.Position;
			bool flag2 = Current.ProgramState == ProgramState.Playing;
			if (flag2)
			{
				bool flag3 = this.map != null;
				if (flag3)
				{
					List<CompFieldOfViewWatcher> fowWatchers = this.mapCompSeenFog.fowWatchers;
					for (int k = 0; k < fowWatchers.Count; k++)
					{
						CompFieldOfViewWatcher compFieldOfViewWatcher = fowWatchers[k];
						int lastSightRange = compFieldOfViewWatcher.lastSightRange;
						bool flag4 = lastSightRange > 0;
						if (flag4)
						{
							IntVec3 position2 = compFieldOfViewWatcher.parent.Position;
							int num = position.x - position2.x;
							int num2 = position.z - position2.z;
							bool flag5 = num * num + num2 * num2 < lastSightRange * lastSightRange;
							if (flag5)
							{
								compFieldOfViewWatcher.refreshFovTarget(ref position);
							}
						}
					}
				}
			}
		}

		// Token: 0x04000045 RID: 69
		private Map map;

		// Token: 0x04000046 RID: 70
		private MapComponentSeenFog mapCompSeenFog;

		// Token: 0x04000047 RID: 71
		private bool lastIsViewBlocker = false;

		// Token: 0x04000048 RID: 72
		private bool blockLight = false;

		// Token: 0x04000049 RID: 73
		private Building b = null;
	}
}
