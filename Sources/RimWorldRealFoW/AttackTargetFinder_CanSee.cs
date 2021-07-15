using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
	// Token: 0x02000002 RID: 2
	[HarmonyPatch(typeof(AttackTargetFinder), "CanSee")]
	internal class AttackTargetFinder_CanSee
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		[HarmonyPrefix]
		public static bool CanSeePreFix(ref bool __result, Thing seer, Thing target, Func<IntVec3, bool> validator = null)
		{
			//Utils.FoWThingUtils.fowIsVisible()
		
			__result = MapUtils.getMapComponentSeenFog(seer.Map).isShown(seer.Faction, target.Position);

			return __result;
		}

		
	}
}
