using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Rimefeller
{
	// Token: 0x02000003 RID: 3
	[StaticConstructorOnStartup]
	public class Building_AdminCameraController : Building
	{/*
		// Token: 0x06000016 RID: 22 RVA: 0x000028F8 File Offset: 0x00000AF8
		public override void ExposeData()
		{
			base.ExposeData();

		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00002950 File Offset: 0x00000B50


		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000018 RID: 24 RVA: 0x00002958 File Offset: 0x00000B58


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
		public Building_AdminCameraController()
		{

		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000299C File Offset: 0x00000B9C
		public virtual bool WorkingNow
		{
			get
			{
				return FlickUtility.WantsToBeOn(this) && (this.powerComp == null || this.powerComp.PowerOn) && (this.breakdownableComp == null || !this.breakdownableComp.BrokenDown);
			}
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000029D5 File Offset: 0x00000BD5
		public IEnumerable<CompProvideVisionPowered> ConnectedRefiners()
		{
			foreach (PipelineNet pipelineNet in this.comp.PipeNets)
			{
				foreach (CompRefinery compRefinery in pipelineNet.Refineries)
				{
					yield return compRefinery;
				}
				List<CompRefinery>.Enumerator enumerator = default(List<CompRefinery>.Enumerator);
			}
			PipelineNet[] array = null;
			yield break;
			yield break;
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000029E8 File Offset: 0x00000BE8
		public bool AnythingToCraft(Pawn crafter)
		{
			if (!this.WorkingNow)
			{
				return false;
			}
			int level = crafter.skills.GetSkill(SkillDefOf.Crafting).Level;
			if (level < this.OperatorSkillLimit.min)
			{
				JobFailReason.Is("UnderAllowedSkill".Translate(this.OperatorSkillLimit.min), this.Label);
				return false;
			}
			if (level > this.OperatorSkillLimit.max)
			{
				JobFailReason.Is("AboveAllowedSkill".Translate(this.OperatorSkillLimit.max), this.Label);
				return false;
			}
			foreach (PipelineNet pipelineNet in this.comp.PipeNets)
			{
				if (pipelineNet.OilStorage.Any((CompStorageTank s) => s.Storage > 0.0 && !s.DrainTank))
				{
					if (pipelineNet.FuelStorage.Any((CompStorageTank v) => v.space > 0f && !v.DrainTank))
					{
						if (pipelineNet.CrudeCrackers.Any((CompCrudeCracker k) => k.WorkingNow && k.CommandToPump))
						{
							return true;
						}
					}
				}
			}
			foreach (Bill bill2 in this.BillStack)
			{
				Bill_Refinery bill = (Bill_Refinery)bill2;
				if (bill.ShouldDoNow())
				{
					Predicate<CompRefinery> <> 9__5;
					foreach (PipelineNet pipelineNet2 in from p in this.comp.PipeNets
														 where p.FuelStorage.Any((CompStorageTank f) => f.Storage > 0.0 && !f.DrainTank)
														 select p)
					{
						List<CompRefinery> refineries = pipelineNet2.Refineries;
						Predicate<CompRefinery> predicate;
						if ((predicate = <> 9__5) == null)
						{
							predicate = (<> 9__5 = ((CompRefinery x) => x.Product == bill.product && x.Buffer < x.Props.BufferSize && x.WorkingNow));
						}
						if (refineries.Any(predicate))
						{
							return true;
						}
					}
				}
			}
			JobFailReason.Is("NoRefinerNeedsWork".Translate(), null);
			return false;
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
			this.comp = map.Rimefeller();
			if (this.billStack == null)
			{
				this.billStack = new BillStack(this);
			}
			foreach (Bill bill in this.billStack)
			{
				bill.ValidateSettings();
			}
			this.comp.RegisterConsole(this);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002DAC File Offset: 0x00000FAC
		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			this.comp.DeregisterConsole(this);
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002DC1 File Offset: 0x00000FC1
		public virtual void UsedThisTick()
		{
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002DC3 File Offset: 0x00000FC3
		public virtual bool CurrentlyUsableForBills()
		{
			return false;
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00002DC3 File Offset: 0x00000FC3
		public virtual bool UsableForBillsAfterFueling()
		{
			return false;
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002DC8 File Offset: 0x00000FC8
		public void DrawInterface(ref Rect row, ref Rect V)
		{
			this.OpenedOnce = true;
			if (MP.IsInMultiplayer)
			{
				MP.WatchBegin();
				MP.Watch(this, "OperatorSkillLimit", null);
			}
			this.comp.DrawInterface(ref row, ref V);
			Widgets.IntRange(V.ContractedBy(4f), row.y.GetHashCode(), ref this.OperatorSkillLimit, 0, 20, "AllowedCraftingSkill", 0);
			if (MP.IsInMultiplayer)
			{
				MP.WatchEnd();
			}
		}

		// Token: 0x04000009 RID: 9
		public BillStack billStack;

		// Token: 0x0400000A RID: 10
		public bool OpenedOnce;

		// Token: 0x0400000B RID: 11
		[SyncField(SyncContext.None)]
		public IntRange OperatorSkillLimit = new IntRange(0, 20);

		// Token: 0x0400000C RID: 12
		public CompPowerTrader powerComp;

		// Token: 0x0400000D RID: 13
		public CompBreakdownable breakdownableComp;

		// Token: 0x0400000E RID: 14
		public MapComponent_Rimefeller comp;

		// Token: 0x0400000F RID: 15
		public int lastTick;

		// Token: 0x04000010 RID: 16
		private Graphic offGraphic;*/
	}
}
