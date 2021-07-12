using System;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x020010F2 RID: 4338
	public class PlaceWorker_NextToWall : PlaceWorker
	{
		// Token: 0x06006894 RID: 26772 RVA: 0x00246898 File Offset: 0x00244A98
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (
                map.coverGrid[loc+new IntVec3(1,0,0)].def.fillPercent == 1
                ||map.coverGrid[loc+new IntVec3(-1,0,0)].def.fillPercent == 1
                ||map.coverGrid[loc+new IntVec3(0,0,1)].def.fillPercent == 1
                ||map.coverGrid[loc+new IntVec3(0,0,-1)].def.fillPercent == 1
                )
			{
                

				return true; 


			}
			else return new AcceptanceReport("MustBeNextToWall".Translate());
		}
	}
}
