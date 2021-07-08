using System;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW.Utils;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000018 RID: 24
	public static class _FertilityGrid
	{
		// Token: 0x06000077 RID: 119 RVA: 0x00008B9C File Offset: 0x00006D9C
		public static void CellBoolDrawerGetBoolInt_Postfix(int index, ref FertilityGrid __instance, ref bool __result)
		{
			bool flag = __result;
			if (flag)
			{
				Map value = Traverse.Create(__instance).Field("map").GetValue<Map>();
				MapComponentSeenFog mapComponentSeenFog = value.getMapComponentSeenFog();
				bool flag2 = mapComponentSeenFog != null;
				if (flag2)
				{
					__result = mapComponentSeenFog.knownCells[index];
				}
			}
		}
	}
}
