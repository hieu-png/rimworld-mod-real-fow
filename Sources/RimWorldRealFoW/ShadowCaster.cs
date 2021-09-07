﻿using System;
using System.Collections.Generic;
using RimWorld;
using RimWorldRealFoW.Utils;
using UnityEngine;
using Verse;

namespace RimWorldRealFoW
{
	// Token: 0x02000006 RID: 6
	public class ShadowCaster
	{
		// Token: 0x06000023 RID: 35 RVA: 0x00003E18 File Offset: 0x00002018
		public static void computeFieldOfViewWithShadowCasting(int startX, int startY, int radius, bool[] viewBlockerCells, int maxX, int maxY, bool handleSeenAndCache, MapComponentSeenFog mapCompSeenFog, Faction faction, short[] factionShownCells, bool[] fovGrid, int fovGridMinX, int fovGridMinY, int fovGridWidth, bool[] oldFovGrid, int oldFovGridMinX, int oldFovGridMaxX, int oldFovGridMinY, int oldFovGridMaxY, int oldFovGridWidth, byte specificOctant = 255, int targetX = -1, int targetY = -1, bool ignoreWalls = false, RoofGrid roofGrid = null)
		{
			int r_r = radius * radius;
			bool flag = specificOctant == byte.MaxValue;
			if (flag)
			{
				for (byte b = 0; b < 8; b += 1)
				{
					ShadowCaster.computeFieldOfViewInOctantZero(b, fovGrid, fovGridMinX, fovGridMinY, fovGridWidth, oldFovGrid, oldFovGridMinX, oldFovGridMaxX, oldFovGridMinY, oldFovGridMaxY, oldFovGridWidth, radius, r_r, startX, startY, maxX, maxY, viewBlockerCells, handleSeenAndCache, mapCompSeenFog, faction, factionShownCells, targetX, targetY, 0, 1, 1, 1, 0, ignoreWalls, roofGrid);
				}
			}
			else
			{
				ShadowCaster.computeFieldOfViewInOctantZero(specificOctant, fovGrid, fovGridMinX, fovGridMinY, fovGridWidth, oldFovGrid, oldFovGridMinX, oldFovGridMaxX, oldFovGridMinY, oldFovGridMaxY, oldFovGridWidth, radius, r_r, startX, startY, maxX, maxY, viewBlockerCells, handleSeenAndCache, mapCompSeenFog, faction, factionShownCells, targetX, targetY, 0, 1, 1, 1, 0, ignoreWalls, roofGrid);
			}
		}

		// Token: 0x06000024 RID: 36 RVA: 0x00003EBC File Offset: 0x000020BC
		private static void computeFieldOfViewInOctantZero(byte octant, bool[] fovGrid, int fovGridMinX, int fovGridMinY, int fovGridWidth, bool[] oldFovGrid, int oldFovGridMinX, int oldFovGridMaxX, int oldFovGridMinY, int oldFovGridMaxY, int oldFovGridWidth, int radius, int r_r, int startX, int startY, int maxX, int maxY, bool[] viewBlockerCells, bool handleSeenAndCache, MapComponentSeenFog mapCompSeenFog, Faction faction, short[] factionShownCells, int targetX, int targetY, int x, int topVectorX, int topVectorY, int bottomVectorX, int bottomVectorY, bool ignoreWalls = false, RoofGrid roofGrid = null)
		{
			int worldY = 0;
			int worldX = 0;
			bool flag = true;

            if (mapCompSeenFog != null && !UnderRoof(roofGrid, startX, startY))
            {
                HashSet<Vector2> towers = mapCompSeenFog.compAffectVisionList;

                foreach (Vector2 mapTower in towers)
                {
                    int towerX = (int) mapTower.x;
                    int towerY = (int) mapTower.y;
                    float xSub = towerX - startX;
                    float ySub = towerY - startY;
                    float distance = Mathf.Sqrt(Mathf.Pow(xSub, 2) + Mathf.Pow(ySub, 2));
                    bool clearSight = true;

                    if (distance < radius)
                    {
                        IEnumerable<Vector2> pointsToTower = GetPointsOnLine(startX, startY, towerX, towerY);

                        foreach (Vector2 point in pointsToTower)
                        {
                            int pointX = (int) point.x;
                            int pointY = (int) point.y;
                            if (UnderRoof(roofGrid, pointX, pointY))
                            {
                                clearSight = false;
                                break;
                            }
                        }

                        if (clearSight)
                        {
                            int posIndex = (towerY * maxX) + towerX;

                            int tFogGridIdx = ((towerY - fovGridMinY) * fovGridWidth) + (towerX - fovGridMinX);
                            if (!fovGrid[tFogGridIdx])
                            {
                                fovGrid[tFogGridIdx] = true;
                                if (handleSeenAndCache)
                                {
                                    if (oldFovGrid == null || towerX < oldFovGridMinX || towerY < oldFovGridMinY || towerX > oldFovGridMaxX || towerY > oldFovGridMaxY)
                                    {
                                        mapCompSeenFog.incrementSeen(faction, factionShownCells, posIndex);
                                    }
                                    else
                                    {
                                        int tOldFogGridIdx = ((towerY - oldFovGridMinY) * oldFovGridWidth) + (towerX - oldFovGridMinX);
                                        if (!oldFovGrid[tOldFogGridIdx])
                                        {
                                            // Old cell was not visible. Increment seen counter in global grid.
                                            mapCompSeenFog.incrementSeen(faction, factionShownCells, posIndex);
                                        }
                                        else
                                        {
                                            // Old cell was already visible. Mark it to not be unseen.
                                            oldFovGrid[tOldFogGridIdx] = false;
                                        }
                                    }
                                }
                            }
						}
                    }
                }
			}

            while (flag || !ShadowCaster.queue.Empty())
			{
				bool flag2 = !flag;
				if (flag2)
				{
					ref ShadowCaster.ColumnPortion ptr = ref ShadowCaster.queue.Dequeue();
					x = ptr.x;
					topVectorX = ptr.topVectorX;
					topVectorY = ptr.topVectorY;
					bottomVectorX = ptr.bottomVectorX;
					bottomVectorY = ptr.bottomVectorY;
				}
				else
				{
					flag = false;
				}
				while (x <= radius)
				{
					int twoX = 2 * x;
					int xSqrd = x * x;
					bool flag3 = x == 0;
					int num5;
					if (flag3)
					{
						num5 = 0;
					}
					else
					{
						int num6 = (twoX + 1) * topVectorY / (2 * topVectorX);
						int num7 = (twoX + 1) * topVectorY % (2 * topVectorX);
						bool flag4 = num7 > topVectorX;
						if (flag4)
						{
							num5 = num6 + 1;
						}
						else
						{
							num5 = num6;
						}
					}
					bool flag5 = x == 0;
					int num8;
					if (flag5)
					{
						num8 = 0;
					}
					else
					{
						int num6 = (twoX - 1) * bottomVectorY / (2 * bottomVectorX);
						int num7 = (twoX - 1) * bottomVectorY % (2 * bottomVectorX);
						bool flag6 = num7 >= bottomVectorX;
						if (flag6)
						{
							num8 = num6 + 1;
						}
						else
						{
							num8 = num6;
						}
					}
					bool flag7 = false;
					bool flag8 = false;
					bool flag9 = octant == 1 || octant == 2;
					if (flag9)
					{
						worldY = startY + x;
					}
					else
					{
						bool flag10 = octant == 3 || octant == 4;
						if (flag10)
						{
							worldX = startX - x;
						}
						else
						{
							bool flag11 = octant == 5 || octant == 6;
							if (flag11)
							{
								worldY = startY - x;
							}
							else
							{
								worldX = startX + x;
							}
						}
					}
					for (int y = num5; y >= num8; y--)
					{
						bool flag12 = octant == 1 || octant == 6;
						if (flag12)
						{
							worldX = startX + y;
						}
						else
						{
							bool flag13 = octant == 2 || octant == 5;
							if (flag13)
							{
								worldX = startX - y;
							}
							else
							{
								bool flag14 = octant == 4 || octant == 7;
								if (flag14)
								{
									worldY = startY - y;
								}
								else
								{
									worldY = startY + y;
								}
							}
						}
						int worldIdx = worldY * maxX + worldX;
						
						bool inRadius = xSqrd + y * y < r_r;
						bool flag16 = inRadius && worldX >= 0 && worldY >= 0 && worldX < maxX && worldY < maxY;
                        if (flag16)
                        {
                            bool flag17 = targetX == -1;
                            if (flag17)
                            {
                                int num10 = (worldY - fovGridMinY) * fovGridWidth + (worldX - fovGridMinX);
                                bool flag18 = !fovGrid[num10];
                                if (flag18)
                                {
                                    fovGrid[num10] = true;
                                    if (handleSeenAndCache)
                                    {
                                        bool flag19 = oldFovGrid == null || worldX < oldFovGridMinX ||
                                                      worldY < oldFovGridMinY || worldX > oldFovGridMaxX ||
                                                      worldY > oldFovGridMaxY;
                                        if (flag19)
                                        {
                                            mapCompSeenFog.incrementSeen(faction, factionShownCells, worldIdx);
                                        }
                                        else
                                        {
                                            int num11 = (worldY - oldFovGridMinY) * oldFovGridWidth +
                                                        (worldX - oldFovGridMinX);
                                            bool flag20 = !oldFovGrid[num11];
                                            if (flag20)
                                            {
                                                mapCompSeenFog.incrementSeen(faction, factionShownCells, worldIdx);
                                            }
                                            else
                                            {
                                                oldFovGrid[num11] = false;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool flag21 = targetX == worldX && targetY == worldY;
                                if (flag21)
                                {
                                    fovGrid[0] = true;
                                    return;
                                }
                            }
                        }
                        bool flag22 = (!inRadius || worldX < 0 || worldY < 0 || worldX >= maxX || worldY >= maxY || (viewBlockerCells[worldIdx] && !ignoreWalls));

                        if (!flag22 && roofGrid != null)
                        {
                            flag22 = ignoreWalls && roofGrid.Roofed(worldIdx);
                        }
						
						bool flag23 = flag8;
						if (flag23)
						{
							bool flag24 = flag22;
							if (flag24)
							{
								bool flag25 = !flag7;
								if (flag25)
								{
									ref ShadowCaster.ColumnPortion ptr2 = ref ShadowCaster.queue.Enqueue();
									ptr2.x = x + 1;
									ptr2.topVectorX = topVectorX;
									ptr2.topVectorY = topVectorY;
									ptr2.bottomVectorX = twoX - 1;
									ptr2.bottomVectorY = 2 * y + 1;
								}
							}
							else
							{
								bool flag26 = flag7;
								if (flag26)
								{
									topVectorX = twoX + 1;
									topVectorY = 2 * y + 1;
								}
							}
						}
						flag8 = true;
						flag7 = flag22;
					}
					bool flag27 = flag8 && !flag7;
					if (!flag27)
					{
						break;
					}
					x++;
				}
			}
		}

        private static bool UnderRoof(RoofGrid roofGrid, int x, int y)
        {
            return roofGrid != null && roofGrid.Roofed(x,y);
        }

		//Using Bresenham's line algorithm to find all coordinates between 2 points
        private static IEnumerable<Vector2> GetPointsOnLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return new Vector2(steep ? y : x, steep ? x : y);
                error -= dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
        }

        //Alternative method for Bresenham's line algorithm. Plan to compare performance at a later date
		static IEnumerable<Vector2> GetLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (; ; )
            {
                yield return new Vector2(x0, y0);
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
            }
        }

		// Token: 0x0400001C RID: 28
		private static ShadowCaster.ColumnPortionQueue queue = new ShadowCaster.ColumnPortionQueue(64);

		// Token: 0x02000030 RID: 48
		private class ColumnPortionQueue
		{
			// Token: 0x06000095 RID: 149 RVA: 0x0000974C File Offset: 0x0000794C
			public ColumnPortionQueue(int size)
			{
				this.nodes = new ShadowCaster.ColumnPortion[size];
				this.currentPos = 0;
				this.nextInsertPos = 0;
			}

			// Token: 0x06000096 RID: 150 RVA: 0x00009770 File Offset: 0x00007970
			public ref ShadowCaster.ColumnPortion Enqueue()
			{
				int num = this.nextInsertPos;
				this.nextInsertPos = num + 1;
				int num2 = num;
				bool flag = this.nextInsertPos >= this.nodes.Length;
				if (flag)
				{
					this.nextInsertPos = 0;
				}
				bool flag2 = this.nextInsertPos == this.currentPos;
				if (flag2)
				{
					ShadowCaster.ColumnPortion[] array = new ShadowCaster.ColumnPortion[this.nodes.Length * 2];
					bool flag3 = this.nextInsertPos == 0;
					if (flag3)
					{
						this.nextInsertPos = this.nodes.Length;
						Array.Copy(this.nodes, array, this.nodes.Length);
					}
					else
					{
						Array.Copy(this.nodes, 0, array, 0, this.nextInsertPos);
						Array.Copy(this.nodes, this.currentPos, array, array.Length - (this.nodes.Length - this.currentPos), this.nodes.Length - this.currentPos);
						this.currentPos = array.Length - (this.nodes.Length - this.currentPos);
					}
					this.nodes = array;
				}
				return ref this.nodes[num2];
			}

			// Token: 0x06000097 RID: 151 RVA: 0x00009894 File Offset: 0x00007A94
			public ref ShadowCaster.ColumnPortion Dequeue()
			{
				int num = this.currentPos;
				this.currentPos = num + 1;
				int num2 = num;
				bool flag = this.currentPos >= this.nodes.Length;
				if (flag)
				{
					this.currentPos = 0;
				}
				return ref this.nodes[num2];
			}

			// Token: 0x06000098 RID: 152 RVA: 0x000098E4 File Offset: 0x00007AE4
			public void Clear()
			{
				this.currentPos = 0;
				this.nextInsertPos = 0;
			}

			// Token: 0x06000099 RID: 153 RVA: 0x000098F8 File Offset: 0x00007AF8
			public bool Empty()
			{
				return this.currentPos == this.nextInsertPos;
			}

			// Token: 0x04000095 RID: 149
			private ShadowCaster.ColumnPortion[] nodes;

			// Token: 0x04000096 RID: 150
			private int currentPos;

			// Token: 0x04000097 RID: 151
			private int nextInsertPos;
		}

		// Token: 0x02000031 RID: 49
		private struct ColumnPortion
		{
			// Token: 0x04000098 RID: 152
			public int x;

			// Token: 0x04000099 RID: 153
			public int topVectorX;

			// Token: 0x0400009A RID: 154
			public int topVectorY;

			// Token: 0x0400009B RID: 155
			public int bottomVectorX;

			// Token: 0x0400009C RID: 156
			public int bottomVectorY;
		}
	}
}
