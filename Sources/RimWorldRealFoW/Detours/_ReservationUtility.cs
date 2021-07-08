using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000025 RID: 37
	public static class _ReservationUtility
	{
		// Token: 0x06000086 RID: 134 RVA: 0x000090DC File Offset: 0x000072DC
		public static void CanReserve_Postfix(this Pawn p, ref bool __result, LocalTargetInfo target)
		{
			bool flag = __result && p.Faction != null && p.Faction.IsPlayer && target.HasThing && target.Thing.def.category != ThingCategory.Pawn;
			if (flag)
			{
				__result = target.Thing.fowIsVisible(false);
			}
		}

		// Token: 0x06000087 RID: 135 RVA: 0x0000913C File Offset: 0x0000733C
		public static void CanReserveAndReach_Postfix(this Pawn p, bool __result, LocalTargetInfo target)
		{
			bool flag = __result && p.Faction != null && p.Faction.IsPlayer && target.HasThing && target.Thing.def.category != ThingCategory.Pawn;
			if (flag)
			{
				__result = target.Thing.fowIsVisible(false);
			}
		}
	}
}
