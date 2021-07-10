using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
	// Token: 0x02000003 RID: 3
	[StaticConstructorOnStartup]
	public class Building_CameraConsole : Building
	{
		// Token: 0x06000016 RID: 22 RVA: 0x000028F8 File Offset: 0x00000AF8
		public override void ExposeData()
		{
			base.ExposeData();

		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002960 File Offset: 0x00000B60
		public bool Manned
		{
			get
			{
				return Find.TickManager.TicksGame < this.lastTick + 250;
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x0000297A File Offset: 0x00000B7A
		public Building_CameraConsole()
		{

		}


		public virtual bool WorkingNow
		{
			get
			{
				return FlickUtility.WantsToBeOn(this) && (this.powerComp == null || this.powerComp.PowerOn) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
			}
		}
		//To do: check if these is any need for camera
		public bool needWatcher()
        {
			return true;
        }


		// Token: 0x0600001E RID: 30 RVA: 0x00002C50 File Offset: 0x00000E50
		public override void Draw()
		{
			base.Draw();
			if (this.Manned)
			{
				if (this.offGraphic == null)
				{
					this.offGraphic = GraphicDatabase.Get(this.def.graphicData.graphicClass, this.def.graphicData.texPath + "_FX", ShaderDatabase.MoteGlow, this.def.graphicData.drawSize, this.DrawColor, this.DrawColorTwo);
				}
				this.offGraphic.Draw(this.DrawPos + new Vector3(0f, 1f, 0f), base.Rotation, this, 0f);
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002D02 File Offset: 0x00000F02
		public void Used()
		{
			this.lastTick = Find.TickManager.TicksGame;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002D14 File Offset: 0x00000F14
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = base.GetComp<CompPowerTrader>();
			this.breakdownableComp = base.GetComp<CompBreakdownable>();
			this.mapComp = MapUtils.getMapComponentSeenFog(map);

			this.mapComp.RegisterCameraConsole(this);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002DAC File Offset: 0x00000FAC
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			this.mapComp.DeRegisterCameraConsole(this);
		}




		public bool OpenedOnce;

		// Token: 0x0400000B RID: 11


		// Token: 0x0400000C RID: 12
		public CompPowerTrader powerComp;

		// Token: 0x0400000D RID: 13
		public CompBreakdownable breakdownableComp;

		// Token: 0x0400000E RID: 14
		public MapComponentSeenFog mapComp;

		// Token: 0x0400000F RID: 15
		public int lastTick;

		// Token: 0x04000010 RID: 16
		private Graphic offGraphic;
	}
}
