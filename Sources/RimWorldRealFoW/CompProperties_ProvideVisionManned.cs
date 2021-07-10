using System;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x0200000E RID: 14
	public class CompProperties_ProvideVisionManned : CompProperties
	{
		// Token: 0x06000043 RID: 67 RVA: 0x00005A20 File Offset: 0x00003C20
		public CompProperties_ProvideVisionManned()
		{
			this.compClass = typeof(CompProvideVision);
		}

		// Token: 0x04000036 RID: 54
		public float viewRadius;
		public float viewRadiusWhenOn;
		public float powerWhenOn = 50;
		

	}
}
