using System;
using RimWorldRealFoW.Utils;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW.Detours
{
	// Token: 0x02000028 RID: 40
	public static class _MouseoverReadout
	{
		// Token: 0x0600008A RID: 138 RVA: 0x00009230 File Offset: 0x00007430
		public static bool MouseoverReadoutOnGUI_Prefix(MouseoverReadout __instance)
		{
			bool flag = Event.current.type != EventType.Repaint;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = Find.MainTabsRoot.OpenTab != null;
				if (flag2)
				{
					result = true;
				}
				else
				{
					IntVec3 c = UI.MouseCell();
					bool flag3 = !c.InBounds(Find.CurrentMap);
					if (flag3)
					{
						result = true;
					}
					else
					{
						MapComponentSeenFog mapComponentSeenFog = Find.CurrentMap.getMapComponentSeenFog();
						bool flag4 = !c.Fogged(Find.CurrentMap) && mapComponentSeenFog != null && !mapComponentSeenFog.knownCells[Find.CurrentMap.cellIndices.CellToIndex(c)];
						if (flag4)
						{
							GenUI.DrawTextWinterShadow(new Rect(256f, (float)(UI.screenHeight - 256), -256f, 256f));
							Text.Font = GameFont.Small;
							GUI.color = new Color(1f, 1f, 1f, 0.8f);
							Rect rect = new Rect(_MouseoverReadout.BotLeft.x, (float)UI.screenHeight - _MouseoverReadout.BotLeft.y, 999f, 999f);
							Widgets.Label(rect, "NotVisible".Translate());
							GUI.color = Color.white;
							result = false;
						}
						else
						{
							result = true;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x04000089 RID: 137
		private static readonly Vector2 BotLeft = new Vector2(15f, 65f);
	}
}
