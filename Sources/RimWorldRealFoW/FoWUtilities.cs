using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorldRealFoW.Utils;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorldRealFoW.Sources.RimWorldRealFoW
{
    public static class FoWUtilities
    {
        private static bool debugMode = true;

        public static Job tryShootFromTower(Pawn pawn)
        {
            IntVec3 intVec = pawn.mindState.duty.focus.Cell;
            MapComponentSeenFog mapComponentSeenFog = pawn.Map.getMapComponentSeenFog();
            
            if (intVec.IsValid && (float)intVec.DistanceToSquared(pawn.Position) < 100f && intVec.GetRoom(pawn.Map) == pawn.GetRoom(RegionType.Set_All) && intVec.WithinRegions(pawn.Position, pawn.Map, 9, TraverseMode.NoPassClosedDoors, RegionType.Set_Passable))
            {
                pawn.GetLord().Notify_ReachedDutyLocation(pawn);
                return null;
            }

            IAttackTarget attackTarget;

            if (!intVec.IsValid)
            {
                if (!(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
                      where !x.ThreatDisabled(pawn) && x.Thing.Faction == Faction.OfPlayer && pawn.CanReach(x.Thing, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.PassAllDestroyableThings)
                      select x).TryRandomElement(out attackTarget))
                {
                    return null;
                }
                intVec = attackTarget.Thing.Position;
            }
            Pawn targetPawn = intVec.GetFirstPawn(pawn.Map);
            bool allowManualCastWeapons = !pawn.IsColonist;
            IntVec3 dest;

            if (targetPawn == null)
            {
                return null;
            }

            Verb verb = pawn.TryGetAttackVerb(targetPawn, allowManualCastWeapons);
            if (verb == null)
            {
                return null;
            }

            CastPositionRequest cpR = new CastPositionRequest
            {
                caster = pawn,
                target = targetPawn,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = (verb.verbProps.range > 5f)
            };

            if (CastPositionFinder.TryFindCastPosition(cpR, out dest))
            {
                float distance = 999;
                IntVec3 towerPos = dest;
                CompMainComponent compMain = (CompMainComponent) pawn.TryGetComp(CompMainComponent.COMP_DEF);

                if (!onTower(pawn.Position, compMain))
                {
                    HashSet<Vector2> towerList = mapComponentSeenFog.compAffectVisionList;
                    foreach (Vector2 tower in towerList)
                    {
                        IntVec3 vec = new IntVec3(tower);
                        float towerDist = vec.DistanceTo(dest);

                        ThingWithComps towerThing = vec.GetFirstThingWithComp<CompAffectVision>(pawn.Map);
                        bool towerBurning = towerThing != null && towerThing.IsBurning();

                        if (towerDist < distance && !towerBurning)
                        {
                            distance = towerDist;
                            towerPos = vec;
                        }
                    }

                    if (distance < 6)
                    {
                        cpR.preferredCastPosition = towerPos;
                        CastPositionFinder.TryFindCastPosition(cpR, out dest);
                    }
                }

                ThingWithComps currentTower = pawn.Position.GetFirstThingWithComp<CompAffectVision>(pawn.Map);
                bool currentTowerBurning = currentTower != null && currentTower.IsBurning();
                if (dest == pawn.Position || (onTower(pawn.Position, compMain) && hasShootingTarget(pawn, verb) && !currentTowerBurning)) // Might only need dest==pawn.Position. Should consider changing currentTowerBurning to have job return null instead of wait_combat
                {
                    return JobMaker.MakeJob(JobDefOf.Wait_Combat, JobGiver_AIFightEnemy.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
                }

                return JobMaker.MakeJob(JobDefOf.Goto, dest);
            }

            return null;
        }

        private static bool onTower(IntVec3 sourceSq, CompMainComponent compMain)
        {
            CompFieldOfViewWatcher compFoV = compMain?.compFieldOfViewWatcher;

            if (compFoV == null)
                return false;

            return compFoV.OnTower(sourceSq);
        }

        private static bool hasShootingTarget(Pawn pawn, Verb verb)
        {
            TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
            if (verb.IsIncendiary())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }

            Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(pawn, targetScanFlags);

            return thing != null;
        }
    }
}
