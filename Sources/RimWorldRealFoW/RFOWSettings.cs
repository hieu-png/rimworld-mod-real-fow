using System;
using System.Collections.Generic;
using RimWorldRealFoW;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
    public class RFOWSettings : ModSettings
    {
        public static void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard row = new Listing_Standard(GameFont.Small);
            row.ColumnWidth = rect.width;
            row.Begin(rect);

            if (row.ButtonTextLabeled("fogAlphaSetting_title".Translate(), ("fogAlphaSetting_" + RFOWSettings.fogAlpha).Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                foreach (object obj in Enum.GetValues(typeof(RFOWSettings.FogAlpha)))
                {
                    RFOWSettings.FogAlpha localValue3 = (RFOWSettings.FogAlpha)obj;
                    RFOWSettings.FogAlpha localValue = localValue3;
                    list.Add(new FloatMenuOption(("fogAlphaSetting_" + localValue).Translate(), delegate ()
                    {
                        RFOWSettings.fogAlpha = localValue;
                        RFOWSettings.applySettings();
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }

            Text.Font = GameFont.Tiny;
            row.Label("fogAlphaSetting_desc".Translate(), -1f, null);
            Text.Font = GameFont.Small;
            addGap(row);

            if (row.ButtonTextLabeled("fogFadeSpeedSetting_title".Translate(), ("fogFadeSpeedSetting_" + RFOWSettings.fogFadeSpeed).Translate()))
            {
                List<FloatMenuOption> list2 = new List<FloatMenuOption>();
                foreach (object obj2 in Enum.GetValues(typeof(RFOWSettings.FogFadeSpeedEnum)))
                {
                    RFOWSettings.FogFadeSpeedEnum localValue2 = (RFOWSettings.FogFadeSpeedEnum)obj2;
                    RFOWSettings.FogFadeSpeedEnum localValue = localValue2;
                    list2.Add(new FloatMenuOption(("fogFadeSpeedSetting_" + localValue).Translate(), delegate ()
                    {
                        RFOWSettings.fogFadeSpeed = localValue;
                        RFOWSettings.applySettings();
                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                }
                Find.WindowStack.Add(new FloatMenu(list2));
            }

            Text.Font = GameFont.Tiny;
            row.Label("fogFadeSpeedSetting_desc".Translate(), -1f, null);
            Text.Font = GameFont.Small;
            addGap(row);
            row.Label("baseViewRangeDesc".Translate() + " (need save reload to be changed): " + baseViewRange.ToString(), -1f, null);
            baseViewRange = (int)row.Slider((float)RFOWSettings.baseViewRange, 10f, 100);
            addGap(row);
            row.Label("buildingVisionModDesc".Translate() + " (the range show will not be accurate): " + Math.Round(buildingVisionModifier,2).ToString(), -1f, null);
            buildingVisionModifier = row.Slider(buildingVisionModifier, 0.2f, 2);
            row.Label("animalVisionModDesc".Translate() + ": " + Math.Round(animalVisionModifier,2).ToString(), -1f, null);
            animalVisionModifier = row.Slider(animalVisionModifier, 0.2f, 2);
            row.Label("turretVisionModDesc".Translate()+ ": " + Math.Round(turretVisionModifier,2).ToString(), -1f, null);
            turretVisionModifier = row.Slider(turretVisionModifier, 0.2f, 2);
            addGap(row);
            row.CheckboxLabeled("wildLifeTabVisible".Translate(), ref RFOWSettings.wildLifeTabVisible,"wildLifeTabVisibleDesc".Translate() );
            row.CheckboxLabeled("NeedWatcher".Translate(), ref RFOWSettings.needWatcher, "NeedWatcherDesc".Translate());
            //if (needWatcher)
            //    row.CheckboxLabeled("NeedStorage".Translate(), ref RFOWSettings.needMemoryStorage, "NeedStorageDesc".Translate());


            row.End();
        }
        public static void addGap(Listing_Standard listing_Standard, float value = 12f)
        {
            listing_Standard.Gap(value);
            listing_Standard.GapLine(value);
        }
        public static void applySettings()
        {
            SectionLayer_FoVLayer.prefFadeSpeedMult = (int)RFOWSettings.fogFadeSpeed;
            SectionLayer_FoVLayer.prefEnableFade = (RFOWSettings.fogFadeSpeed != RFOWSettings.FogFadeSpeedEnum.Disabled);
            SectionLayer_FoVLayer.prefFogAlpha = (byte)RFOWSettings.fogAlpha;
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
            Scribe_Values.Look<RFOWSettings.FogFadeSpeedEnum>(ref RFOWSettings.fogFadeSpeed, "fogFadeSpeed", RFOWSettings.FogFadeSpeedEnum.Medium, false);
            Scribe_Values.Look<RFOWSettings.FogAlpha>(ref RFOWSettings.fogAlpha, "fogAlpha", RFOWSettings.FogAlpha.Medium, false);
            Scribe_Values.Look<int>(ref RFOWSettings.baseViewRange, "baseViewRange", 60, false);

			Scribe_Values.Look<float>(ref RFOWSettings.buildingVisionModifier, "buildingVisionMod", 1, false);
			Scribe_Values.Look<float>(ref RFOWSettings.animalVisionModifier, "animalVisionMod", 0.5f, false);
			Scribe_Values.Look<float>(ref RFOWSettings.turretVisionModifier, "turretVisionMod", 0.7f, false);

            Scribe_Values.Look<bool>(ref RFOWSettings.wildLifeTabVisible, "wildLifeTabVisible", true, false);

            Scribe_Values.Look<bool>(ref RFOWSettings.needWatcher, "needWatcher", true, false);
            Scribe_Values.Look<bool>(ref RFOWSettings.needMemoryStorage, "needMemoryStorage", true, false);

            RFOWSettings.applySettings();
        }

        public static RFOWSettings.FogFadeSpeedEnum fogFadeSpeed = RFOWSettings.FogFadeSpeedEnum.Medium;

        public static RFOWSettings.FogAlpha fogAlpha = RFOWSettings.FogAlpha.Medium;

        public static int baseViewRange = 60;

        public static float buildingVisionModifier = 1;

        public static float turretVisionModifier = 0.7f;

        public static float animalVisionModifier = 0.5f;

        public static bool needWatcher = true;

        public static bool wildLifeTabVisible = true;
        public static bool needMemoryStorage = true;
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
