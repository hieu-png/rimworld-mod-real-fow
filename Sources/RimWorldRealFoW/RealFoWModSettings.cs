using System;
using System.Collections.Generic;
using RimWorldRealFoW;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
	public class RealFoWModSettings : ModSettings
	{
		public static void DoSettingsWindowContents(Rect rect)
		{
			Listing_Standard listing_Standard = new Listing_Standard(GameFont.Small);
			listing_Standard.ColumnWidth = rect.width;
			listing_Standard.Begin(rect);
			if (listing_Standard.ButtonTextLabeled("fogAlphaSetting_title".Translate(), ("fogAlphaSetting_" + RealFoWModSettings.fogAlpha).Translate()))
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
			addGap(listing_Standard);
			if (listing_Standard.ButtonTextLabeled("fogFadeSpeedSetting_title".Translate(), ("fogFadeSpeedSetting_" + RealFoWModSettings.fogFadeSpeed).Translate()))
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
			listing_Standard.Gap(12f);
			listing_Standard.GapLine(12f);


			listing_Standard.End();
		}
		public static void addGap(Listing_Standard listing_Standard, float value = 12f) {
			listing_Standard.Gap(value);
			listing_Standard.GapLine(value);
		}
		public static void applySettings()
		{
			SectionLayer_FoVLayer.prefFadeSpeedMult = (int)RealFoWModSettings.fogFadeSpeed;
			SectionLayer_FoVLayer.prefEnableFade = (RealFoWModSettings.fogFadeSpeed != RealFoWModSettings.FogFadeSpeedEnum.Disabled);
			SectionLayer_FoVLayer.prefFogAlpha = (byte)RealFoWModSettings.fogAlpha;
			if (Current.ProgramState == ProgramState.Playing)
			{
				foreach (Map map in Find.Maps)
				{
					if (map.mapDrawer != null)
					{
						map.mapDrawer.RegenerateEverythingNow();
					}
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<RealFoWModSettings.FogFadeSpeedEnum>(ref RealFoWModSettings.fogFadeSpeed, "fogFadeSpeed", RealFoWModSettings.FogFadeSpeedEnum.Medium, false);
			Scribe_Values.Look<RealFoWModSettings.FogAlpha>(ref RealFoWModSettings.fogAlpha, "fogAlpha", RealFoWModSettings.FogAlpha.Medium, false);
			RealFoWModSettings.applySettings();
		}

		public static RealFoWModSettings.FogFadeSpeedEnum fogFadeSpeed = RealFoWModSettings.FogFadeSpeedEnum.Medium;

		public static RealFoWModSettings.FogAlpha fogAlpha = RealFoWModSettings.FogAlpha.Medium;

		public enum FogFadeSpeedEnum
		{
			Slow = 5,
			Medium = 20,
			Fast = 40,
			Disabled = 100
		}

		// Token: 0x0200002D RID: 45
		public enum FogAlpha
		{
			Black = 255,
			NearlyBlack = 210,
			VeryVeryVeryDark = 180,	
			VeryVeryDark = 150,
			VeryDark = 120,
			Dark = 100,
			Medium = 80,
			Light = 60
		}
	}
}
