using System;
using Verse;

namespace RimWorldRealFoW.Utils
{
	// Token: 0x02000008 RID: 8
	public static class MapUtils
	{
		// Token: 0x0600002D RID: 45 RVA: 0x000051F0 File Offset: 0x000033F0
		public static MapComponentSeenFog getMapComponentSeenFog(this Map map)
		{
			MapComponentSeenFog mapComponentSeenFog = map.GetComponent<MapComponentSeenFog>();
			bool flag = mapComponentSeenFog == null;
			if (flag)
			{
				mapComponentSeenFog = new MapComponentSeenFog(map);
				map.components.Add(mapComponentSeenFog);
			}
			return mapComponentSeenFog;
		}
	}
}
