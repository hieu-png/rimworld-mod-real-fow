using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
	// Token: 0x02000011 RID: 17
	internal class JobDriver_SurveilCameraConsole : JobDriver
	{
		// Token: 0x06000067 RID: 103 RVA: 0x00004754 File Offset: 0x00002954
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			LocalTargetInfo targetA = this.job.targetA;
			return this.pawn.Reserve(targetA, this.job, 1, -1, null, errorOnFailed);
			/*
			if (!this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed))
			{
				return false;
			}
			this.pawn.ReserveAsManyAsPossible(this.job.GetTargetQueue(TargetIndex.B), this.job, 1, -1, null);
			return true;*/
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00004783 File Offset: 0x00002983
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(() => !(this.job.targetA.Thing as Building_CameraConsole).needWatcher());

			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);

			Toil work = new Toil();

			work.tickAction = delegate ()
			{
				Pawn actor = work.GetActor();
				Building_CameraConsole building_CameraConsole = this.job.targetA.Thing as Building_CameraConsole;
				if (building_CameraConsole != null)
				{
					building_CameraConsole.Used();
				}
				actor.GainComfortFromCellIfPossible(true);
			};
			work.defaultCompleteMode = ToilCompleteMode.Never;
			work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			work.activeSkill = (() => SkillDefOf.Intellectual);
			yield return work;
			yield break;
		}
	}
}
