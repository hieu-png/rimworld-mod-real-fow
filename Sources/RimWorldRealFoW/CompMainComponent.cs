using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x0200000A RID: 10
	public class CompMainComponent : ThingComp
	{
		// Token: 0x06000034 RID: 52 RVA: 0x000054AC File Offset: 0x000036AC
		private void performSetup()
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.setup = true;
				ThingCategory category = this.parent.def.category;
				this.compComponentsPositionTracker = new CompComponentsPositionTracker();
				this.compComponentsPositionTracker.parent = this.parent;
				this.compComponentsPositionTracker.mainComponent = this;
				this.compHiddenable = new CompHiddenable();
				this.compHiddenable.parent = this.parent;
				this.compHiddenable.mainComponent = this;
				this.compHideFromPlayer = new CompHideFromPlayer();
				this.compHideFromPlayer.parent = this.parent;
				this.compHideFromPlayer.mainComponent = this;
				bool flag2 = category == ThingCategory.Building;
				if (flag2)
				{
					this.compViewBlockerWatcher = new CompViewBlockerWatcher();
					this.compViewBlockerWatcher.parent = this.parent;
					this.compViewBlockerWatcher.mainComponent = this;
				}
				bool flag3 = category == ThingCategory.Pawn || category == ThingCategory.Building;
				if (flag3)
				{
					this.compFieldOfViewWatcher = new CompFieldOfViewWatcher();
					this.compFieldOfViewWatcher.parent = this.parent;
					this.compFieldOfViewWatcher.mainComponent = this;
				}
			}
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000055C8 File Offset: 0x000037C8
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.PostSpawnSetup(respawningAfterLoad);
			this.compHiddenable.PostSpawnSetup(respawningAfterLoad);
			this.compHideFromPlayer.PostSpawnSetup(respawningAfterLoad);
			bool flag2 = this.compViewBlockerWatcher != null;
			if (flag2)
			{
				this.compViewBlockerWatcher.PostSpawnSetup(respawningAfterLoad);
			}
			bool flag3 = this.compFieldOfViewWatcher != null;
			if (flag3)
			{
				this.compFieldOfViewWatcher.PostSpawnSetup(respawningAfterLoad);
			}
		}

		// Token: 0x06000036 RID: 54 RVA: 0x0000564C File Offset: 0x0000384C
		public override void CompTick()
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.CompTick();
			this.compHiddenable.CompTick();
			this.compHideFromPlayer.CompTick();
			bool flag2 = this.compViewBlockerWatcher != null;
			if (flag2)
			{
				this.compViewBlockerWatcher.CompTick();
			}
			bool flag3 = this.compFieldOfViewWatcher != null;
			if (flag3)
			{
				this.compFieldOfViewWatcher.CompTick();
			}
		}

		// Token: 0x06000037 RID: 55 RVA: 0x000056CC File Offset: 0x000038CC
		public override void CompTickRare()
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.CompTickRare();
			this.compHiddenable.CompTickRare();
			this.compHideFromPlayer.CompTickRare();
			bool flag2 = this.compViewBlockerWatcher != null;
			if (flag2)
			{
				this.compViewBlockerWatcher.CompTickRare();
			}
			bool flag3 = this.compFieldOfViewWatcher != null;
			if (flag3)
			{
				this.compFieldOfViewWatcher.CompTickRare();
			}
		}

		// Token: 0x06000038 RID: 56 RVA: 0x0000574C File Offset: 0x0000394C
		public override void ReceiveCompSignal(string signal)
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.ReceiveCompSignal(signal);
			this.compHiddenable.ReceiveCompSignal(signal);
			this.compHideFromPlayer.ReceiveCompSignal(signal);
			bool flag2 = this.compViewBlockerWatcher != null;
			if (flag2)
			{
				this.compViewBlockerWatcher.ReceiveCompSignal(signal);
			}
			bool flag3 = this.compFieldOfViewWatcher != null;
			if (flag3)
			{
				this.compFieldOfViewWatcher.ReceiveCompSignal(signal);
			}
		}

		// Token: 0x06000039 RID: 57 RVA: 0x000057D0 File Offset: 0x000039D0
		public override void PostDeSpawn(Map map)
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.PostDeSpawn(map);
			this.compHiddenable.PostDeSpawn(map);
			this.compHideFromPlayer.PostDeSpawn(map);
			bool flag2 = this.compViewBlockerWatcher != null;
			if (flag2)
			{
				this.compViewBlockerWatcher.PostDeSpawn(map);
			}
			bool flag3 = this.compFieldOfViewWatcher != null;
			if (flag3)
			{
				this.compFieldOfViewWatcher.PostDeSpawn(map);
			}
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00005854 File Offset: 0x00003A54
		public override void PostExposeData()
		{
			bool flag = !this.setup;
			if (flag)
			{
				this.performSetup();
			}
			this.compComponentsPositionTracker.PostExposeData();
			this.compHiddenable.PostExposeData();
			this.compHideFromPlayer.PostExposeData();
			bool flag2 = this.compViewBlockerWatcher != null;
			if (flag2)
			{
				this.compViewBlockerWatcher.PostExposeData();
			}
			bool flag3 = this.compFieldOfViewWatcher != null;
			if (flag3)
			{
				this.compFieldOfViewWatcher.PostExposeData();
			}
			bool savingForDebug = Scribe.saver.savingForDebug;
			if (savingForDebug)
			{
				bool flag4 = this.compComponentsPositionTracker != null;
				bool flag5 = this.compHiddenable != null;
				bool flag6 = this.compHideFromPlayer != null;
				bool flag7 = this.compViewBlockerWatcher != null;
				bool flag8 = this.compFieldOfViewWatcher != null;
				Scribe_Values.Look<bool>(ref flag4, "hasCompComponentsPositionTracker", false, false);
				Scribe_Values.Look<bool>(ref flag5, "hasCompHiddenable", false, false);
				Scribe_Values.Look<bool>(ref flag6, "hasCompHideFromPlayer", false, false);
				Scribe_Values.Look<bool>(ref flag7, "hasCompViewBlockerWatcher", false, false);
				Scribe_Values.Look<bool>(ref flag8, "hasCompFieldOfViewWatcher", false, false);
			}
		}

		// Token: 0x0400002A RID: 42
		public static readonly CompProperties COMP_DEF = new CompProperties(typeof(CompMainComponent));

		// Token: 0x0400002B RID: 43
		private bool setup = false;

		// Token: 0x0400002C RID: 44
		public CompComponentsPositionTracker compComponentsPositionTracker = null;

		// Token: 0x0400002D RID: 45
		public CompFieldOfViewWatcher compFieldOfViewWatcher = null;

		// Token: 0x0400002E RID: 46
		public CompHiddenable compHiddenable = null;

		// Token: 0x0400002F RID: 47
		public CompHideFromPlayer compHideFromPlayer = null;

		// Token: 0x04000030 RID: 48
		public CompViewBlockerWatcher compViewBlockerWatcher = null;
	}
}
