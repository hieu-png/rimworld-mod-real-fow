using System;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x020010F2 RID: 4338
	public class PlaceWorker_UnderRoof : PlaceWorker
	{
		// Token: 0x06006894 RID: 26772 RVA: 0x00246898 File Offset: 0x00244A98
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (!map.roofGrid.Roofed(loc))
			{
				return new AcceptanceReport("MustBeUnderRoof".Translate()); 


			}
			else return true;
		}
	}
}
