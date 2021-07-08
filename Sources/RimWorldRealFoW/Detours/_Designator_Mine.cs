using System;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x0200001E RID: 30
	public static class _Designator_Mine
	{
		// Token: 0x0600007E RID: 126 RVA: 0x00008EA4 File Offset: 0x000070A4
		public static void CanDesignateCell_Postfix(IntVec3 c, ref Designator __instance, ref AcceptanceReport __result)
		{
			bool flag = !__result.Accepted;
			if (flag)
			{
				Map value = Traverse.Create(__instance).Property("Map", null).GetValue<Map>();
				bool flag2 = value.designationManager.DesignationAt(c, DesignationDefOf.Mine) == null;
				if (flag2)
				{
					MapComponentSeenFog mapComponentSeenFog = value.getMapComponentSeenFog();
					bool flag3 = mapComponentSeenFog != null && c.InBounds(value) && !mapComponentSeenFog.knownCells[value.cellIndices.CellToIndex(c)];
					if (flag3)
					{
						__result = true;
					}
				}
			}
		}
	}
}
