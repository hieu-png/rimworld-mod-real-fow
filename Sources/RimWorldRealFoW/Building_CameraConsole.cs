using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
	[StaticConstructorOnStartup]
	public class Building_CameraConsole : Building
	{
		public override void ExposeData()
		{
			base.ExposeData();

		} 


		public bool Manned
		{
			get
			{
				return Find.TickManager.TicksGame < this.lastTick + 100;
			}
		}

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
		public bool needWatcher()
        {
			//return mapComp.SurveillanceCameraCount() >= 1;
			//Turret need the console to work so just keep it like this
			return true;
        }


		public override void Draw()
		{
			base.Draw();
			if (this.Manned)
			{
				int cameraCount = Mathf.Min(mapComp.SurveillanceCameraCount(), 12);
				this.workingGraphics[cameraCount].Draw(this.DrawPos + new Vector3(0f, 1f, 0f), base.Rotation, this, 0f);
			}
		}

		public void Used()
		{
			this.lastTick = Find.TickManager.TicksGame;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.powerComp = base.GetComp<CompPowerTrader>();
			this.breakdownableComp = base.GetComp<CompBreakdownable>();
			this.mapComp = MapUtils.getMapComponentSeenFog(map);
			this.mapComp.RegisterCameraConsole(this);
			//12 Possible graphic so
			for (int i = 0; i <= 12; i++)
			{
				workingGraphics.Add(GraphicDatabase.Get(
					this.def.graphicData.graphicClass,
					this.def.graphicData.texPath + "_FX" + (i).ToString(), ShaderDatabase.MoteGlow,
					this.def.graphicData.drawSize,
					this.DrawColor,
					this.DrawColorTwo
					));
			}
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002DAC File Offset: 0x00000FAC
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			this.mapComp.DeregisterCameraConsole(this);
		}




		public bool OpenedOnce;



		public CompPowerTrader powerComp;

		public CompBreakdownable breakdownableComp;

		public MapComponentSeenFog mapComp;

		public int lastTick;

		private List<Graphic> workingGraphics = new List<Graphic>();
		
	}
}
