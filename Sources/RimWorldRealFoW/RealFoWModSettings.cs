using System;
using System.Collections.Generic;
using RimWorldRealFoW.SectionLayers;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000003 RID: 3
	public class RealFoWModSettings : ModSettings
	{
		// Token: 0x06000003 RID: 3 RVA: 0x00002070 File Offset: 0x00000270
		public static void DoSettingsWindowContents(Rect rect)
		{
			Listing_Standard listing_Standard = new Listing_Standard(GameFont.Small);
			listing_Standard.ColumnWidth = rect.width;
			listing_Standard.Begin(rect);
			bool flag = listing_Standard.ButtonTextLabeled("fogAlphaSetting_title".Translate(), ("fogAlphaSetting_" + RealFoWModSettings.fogAlpha).Translate());
			if (flag)
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (object obj in Enum.GetValues(typeof(RealFoWModSettings.FogAlpha)))
				{
					RealFoWModSettings.FogAlpha localValue3 = (RealFoWModSettings.FogAlpha)obj;
					RealFoWModSettings.FogAlpha localValue = localValue3;
					list.Add(new FloatMenuOption(("fogAlphaSetting_" + localValue).Translate(), delegate ()
					{
						RealFoWModSettings.fogAlpha = localValue;
						RealFoWModSettings.applySettings();
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("fogAlphaSetting_desc".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.Gap(12f);
			listing_Standard.GapLine(12f);
			bool flag2 = listing_Standard.ButtonTextLabeled("fogFadeSpeedSetting_title".Translate(), ("fogFadeSpeedSetting_" + RealFoWModSettings.fogFadeSpeed).Translate());
			if (flag2)
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (object obj2 in Enum.GetValues(typeof(RealFoWModSettings.FogFadeSpeedEnum)))
				{
					RealFoWModSettings.FogFadeSpeedEnum localValue2 = (RealFoWModSettings.FogFadeSpeedEnum)obj2;
					RealFoWModSettings.FogFadeSpeedEnum localValue = localValue2;
					list2.Add(new FloatMenuOption(("fogFadeSpeedSetting_" + localValue).Translate(), delegate ()
					{
						RealFoWModSettings.fogFadeSpeed = localValue;
						RealFoWModSettings.applySettings();
					}, MenuOptionPriority.Default, null, null, 0f, null, null));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			Text.Font = GameFont.Tiny;
			listing_Standard.Label("fogFadeSpeedSetting_desc".Translate(), -1f, null);
			Text.Font = GameFont.Small;
			listing_Standard.End();
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002304 File Offset: 0x00000504
		public static void applySettings()
		{
			SectionLayer_FoVLayer.prefFadeSpeedMult = (int)RealFoWModSettings.fogFadeSpeed;
			SectionLayer_FoVLayer.prefEnableFade = (RealFoWModSettings.fogFadeSpeed != RealFoWModSettings.FogFadeSpeedEnum.Disabled);
			SectionLayer_FoVLayer.prefFogAlpha = (byte)RealFoWModSettings.fogAlpha;
			bool flag = Current.ProgramState == ProgramState.Playing;
			if (flag)
			{
				foreach (Map map in Find.Maps)
				{
					bool flag2 = map.mapDrawer != null;
					if (flag2)
					{
						map.mapDrawer.RegenerateEverythingNow();
					}
				}
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000023A4 File Offset: 0x000005A4
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<RealFoWModSettings.FogFadeSpeedEnum>(ref RealFoWModSettings.fogFadeSpeed, "fogFadeSpeed", RealFoWModSettings.FogFadeSpeedEnum.Medium, false);
			Scribe_Values.Look<RealFoWModSettings.FogAlpha>(ref RealFoWModSettings.fogAlpha, "fogAlpha", RealFoWModSettings.FogAlpha.Medium, false);
			RealFoWModSettings.applySettings();
		}

		// Token: 0x04000003 RID: 3
		public static RealFoWModSettings.FogFadeSpeedEnum fogFadeSpeed = RealFoWModSettings.FogFadeSpeedEnum.Medium;

		// Token: 0x04000004 RID: 4
		public static RealFoWModSettings.FogAlpha fogAlpha = RealFoWModSettings.FogAlpha.Medium;

		// Token: 0x0200002C RID: 44
		public enum FogFadeSpeedEnum
		{
			// Token: 0x0400008B RID: 139
			Slow = 5,
			// Token: 0x0400008C RID: 140
			Medium = 20,
			// Token: 0x0400008D RID: 141
			Fast = 40,
			// Token: 0x0400008E RID: 142
			Disabled = 100
		}

		// Token: 0x0200002D RID: 45
		public enum FogAlpha
		{
			// Token: 0x04000090 RID: 144
			Dark = 127,
			// Token: 0x04000091 RID: 145
			Medium = 86,
			// Token: 0x04000092 RID: 146
			Light = 64
		}
	}
}
