using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW.Detours;
using RimWorldRealFoW.ThingComps;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW
{
	// Token: 0x02000005 RID: 5
	[StaticConstructorOnStartup]
	public class RealFoWModStarter : Mod
	{
		// Token: 0x06000019 RID: 25 RVA: 0x000032A5 File Offset: 0x000014A5
		static RealFoWModStarter()
		{
			RealFoWModStarter.injectDetours();
			RealFoWModStarter.harmony = null;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000032C3 File Offset: 0x000014C3
		public RealFoWModStarter(ModContentPack content) : base(content)
		{
			LongEventHandler.QueueLongEvent(new Action(RealFoWModStarter.injectComponents), "Real Fog of War - Init.", false, null, true);
			base.GetSettings<RealFoWModSettings>();
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000032F0 File Offset: 0x000014F0
		public override string SettingsCategory()
		{
			return base.Content.Name;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000330D File Offset: 0x0000150D
		public override void DoSettingsWindowContents(Rect rect)
		{
			RealFoWModSettings.DoSettingsWindowContents(rect);
		}

		// Token: 0x0600001D RID: 29 RVA: 0x00003318 File Offset: 0x00001518
		public static void injectComponents()
		{
			foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
			{
				ThingCategory category = thingDef.category;
				bool flag = typeof(ThingWithComps).IsAssignableFrom(thingDef.thingClass) && (category == ThingCategory.Pawn || category == ThingCategory.Building || category == ThingCategory.Item || category == ThingCategory.Filth || category == ThingCategory.Gas || thingDef.IsBlueprint);
				if (flag)
				{
					RealFoWModStarter.addComponentAsFirst(thingDef, CompMainComponent.COMP_DEF);
				}
			}
		}

		// Token: 0x0600001E RID: 30 RVA: 0x000033B4 File Offset: 0x000015B4
		public static void addComponentAsFirst(ThingDef def, CompProperties compProperties)
		{
			bool flag = !def.comps.Contains(compProperties);
			if (flag)
			{
				def.comps.Insert(0, compProperties);
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x000033E8 File Offset: 0x000015E8
		public static void injectDetours()
		{
			RealFoWModStarter.patchMethod(typeof(Verb), typeof(_Verb), "CanHitCellFromCellIgnoringRange");
			RealFoWModStarter.patchMethod(typeof(Selector), typeof(_Selector), "Select");
			RealFoWModStarter.patchMethod(typeof(MouseoverReadout), typeof(_MouseoverReadout), "MouseoverReadoutOnGUI");
			RealFoWModStarter.patchMethod(typeof(BeautyUtility), typeof(_BeautyUtility), "FillBeautyRelevantCells");
			RealFoWModStarter.patchMethod(typeof(MainTabWindow_Wildlife), typeof(_MainTabWindow_Wildlife), "get_Pawns");
			RealFoWModStarter.patchMethod(typeof(Pawn), typeof(_Pawn), "DrawGUIOverlay");
			RealFoWModStarter.patchMethod(typeof(GenMapUI), typeof(_GenMapUI), "DrawThingLabel", new Type[]
			{
				typeof(Thing),
				typeof(string),
				typeof(Color)
			});
			RealFoWModStarter.patchMethod(typeof(SectionLayer_ThingsGeneral), typeof(_SectionLayer_ThingsGeneral), "TakePrintFrom");
			RealFoWModStarter.patchMethod(typeof(SectionLayer_ThingsPowerGrid), typeof(_SectionLayer_ThingsPowerGrid), "TakePrintFrom");
			RealFoWModStarter.patchMethod(typeof(ReservationUtility), typeof(_ReservationUtility), "CanReserve");
			RealFoWModStarter.patchMethod(typeof(ReservationUtility), typeof(_ReservationUtility), "CanReserveAndReach");
			RealFoWModStarter.patchMethod(typeof(HaulAIUtility), typeof(_HaulAIUtility), "HaulToStorageJob");
			RealFoWModStarter.patchMethod(typeof(EnvironmentStatsDrawer), typeof(_EnvironmentStatsDrawer), "ShouldShowWindowNow");
			RealFoWModStarter.patchMethod(typeof(Messages), typeof(_Messages), "Message", new Type[]
			{
				typeof(string),
				typeof(LookTargets),
				typeof(MessageTypeDef),
				typeof(bool)
			});
			RealFoWModStarter.patchMethod(typeof(LetterStack), typeof(_LetterStack), "ReceiveLetter", new Type[]
			{
				typeof(TaggedString),
				typeof(TaggedString),
				typeof(LetterDef),
				typeof(LookTargets),
				typeof(Faction),
				typeof(Quest),
				typeof(List<ThingDef>),
				typeof(string)
			});
			RealFoWModStarter.patchMethod(typeof(MoteBubble), typeof(_MoteBubble), "Draw", new Type[0]);
			RealFoWModStarter.patchMethod(typeof(GenView), typeof(_GenView), "ShouldSpawnMotesAt", new Type[]
			{
				typeof(IntVec3),
				typeof(Map)
			});
			RealFoWModStarter.patchMethod(typeof(FertilityGrid), typeof(_FertilityGrid), "CellBoolDrawerGetBoolInt");
			RealFoWModStarter.patchMethod(typeof(TerrainGrid), typeof(_TerrainGrid), "CellBoolDrawerGetBoolInt");
			RealFoWModStarter.patchMethod(typeof(RoofGrid), typeof(_RoofGrid), "GetCellBool");
			RealFoWModStarter.patchMethod(typeof(Designator_AreaBuildRoof), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_AreaNoRoof), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_ZoneAdd_Growing), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_ZoneAddStockpile), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Claim), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Claim), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_Deconstruct), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Deconstruct), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_Haul), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Haul), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_Hunt), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Hunt), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_Plants), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Plants), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_PlantsHarvest), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_PlantsHarvestWood), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_RemoveFloor), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_SmoothSurface), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Tame), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Tame), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_Uninstall), typeof(_Designator_Prefix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Uninstall), typeof(_Designator_Prefix), "CanDesignateThing");
			RealFoWModStarter.patchMethod(typeof(Designator_Build), typeof(_Designator_Place_Postfix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Install), typeof(_Designator_Place_Postfix), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designator_Mine), typeof(_Designator_Mine), "CanDesignateCell");
			RealFoWModStarter.patchMethod(typeof(Designation), typeof(_Designation), "Notify_Added");
			RealFoWModStarter.patchMethod(typeof(Designation), typeof(_Designation), "Notify_Removing");
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00003AA2 File Offset: 0x00001CA2
		public static void patchMethod(Type sourceType, Type targetType, string methodName)
		{
			RealFoWModStarter.patchMethod(sourceType, targetType, methodName, null);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00003AB0 File Offset: 0x00001CB0
		public static void patchMethod(Type sourceType, Type targetType, string methodName, params Type[] types)
		{
			bool flag = types != null;
			MethodInfo method;
			if (flag)
			{
				method = sourceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
			}
			else
			{
				method = sourceType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			}
			bool flag2 = sourceType != method.DeclaringType;
			if (flag2)
			{
				Log.Message(string.Concat(new object[]
				{
					"Inconsistent method declaring type for method ",
					methodName,
					": expected ",
					sourceType,
					" but found ",
					method.DeclaringType
				}), false);
			}
			bool flag3 = method != null;
			if (flag3)
			{
				MethodInfo methodInfo = null;
				bool flag4 = types != null;
				if (flag4)
				{
					methodInfo = targetType.GetMethod(methodName + "_Prefix", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
					bool flag5 = methodInfo == null;
					if (flag5)
					{
						methodInfo = targetType.GetMethod(methodName + "_Prefix", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
						{
							sourceType
						}.Concat(types).ToArray<Type>(), null);
					}
				}
				bool flag6 = methodInfo == null;
				if (flag6)
				{
					methodInfo = targetType.GetMethod(methodName + "_Prefix", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}
				MethodInfo methodInfo2 = null;
				bool flag7 = types != null;
				if (flag7)
				{
					methodInfo2 = targetType.GetMethod(methodName + "_Postfix", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
					bool flag8 = methodInfo2 == null;
					if (flag8)
					{
						methodInfo2 = targetType.GetMethod(methodName + "_Postfix", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[]
						{
							sourceType
						}.Concat(types).ToArray<Type>(), null);
					}
				}
				bool flag9 = methodInfo2 == null;
				if (flag9)
				{
					methodInfo2 = targetType.GetMethod(methodName + "_Postfix", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}
				bool flag10 = methodInfo != null || methodInfo2 != null;
				if (flag10)
				{
					bool flag11 = RealFoWModStarter.patchWithHarmony(method, methodInfo, methodInfo2);
					if (flag11)
					{
						Log.Message(string.Concat(new object[]
						{
							"Patched method ",
							method.ToString(),
							" from source ",
							sourceType,
							" to ",
							targetType,
							"."
						}), false);
					}
					else
					{
						Log.Warning(string.Concat(new object[]
						{
							"Unable to patch method ",
							method.ToString(),
							" from source ",
							sourceType,
							" to ",
							targetType,
							"."
						}), false);
					}
				}
				else
				{
					Log.Warning(string.Concat(new object[]
					{
						"Target method prefix or suffix ",
						methodName,
						" not found for patch from source ",
						sourceType,
						" to ",
						targetType,
						"."
					}), false);
				}
			}
			else
			{
				Log.Warning(string.Concat(new object[]
				{
					"Source method ",
					methodName,
					" not found for patch from source ",
					sourceType,
					" to ",
					targetType,
					"."
				}), false);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00003D94 File Offset: 0x00001F94
		public static bool patchWithHarmony(MethodInfo original, MethodInfo prefix, MethodInfo postfix)
		{
			bool result;
			try
			{
				HarmonyMethod prefix2 = (prefix != null) ? new HarmonyMethod(prefix) : null;
				HarmonyMethod postfix2 = (postfix != null) ? new HarmonyMethod(postfix) : null;
				RealFoWModStarter.harmony.Patch(original, prefix2, postfix2, null, null);
				result = true;
			}
			catch (Exception ex)
			{
				Log.Warning("Error patching with Harmony: " + ex.Message, false);
				Log.Warning(ex.StackTrace, false);
				result = false;
			}
			return result;
		}

		// Token: 0x0400001B RID: 27
		private static Harmony harmony = new Harmony("com.github.lukakama.rimworldmodrealfow");
	}
}
