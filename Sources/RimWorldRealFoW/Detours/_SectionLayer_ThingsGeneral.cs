﻿using System;
using RimWorldRealFoW;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000023 RID: 35
	public static class _SectionLayer_ThingsGeneral
	{
		// Token: 0x06000084 RID: 132 RVA: 0x0000908C File Offset: 0x0000728C
		public static bool TakePrintFrom_Prefix(Thing t)
		{
			return t.fowIsVisible(true);
		}
	}
}
