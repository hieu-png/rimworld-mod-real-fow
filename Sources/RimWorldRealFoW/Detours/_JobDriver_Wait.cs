using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using RimWorldRealFoW.Utils;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW.Sources.RimWorldRealFoW.Detours
{
    static class _JobDriver_Wait
    {
        static void CheckForAutoAttack_Postfix(JobDriver __instance)
        {
            if (!onTower(__instance.pawn.Position, __instance.pawn))
            {
                return;
            }

            if ((__instance.pawn.story == null ||
                 !__instance.pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                && __instance.pawn.Faction != null
                && (__instance.pawn.drafter == null || __instance.pawn.drafter.FireAtWill))
            {
                Verb currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
                if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
                {
                    TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
                    if (currentEffectiveVerb.IsIncendiary())
                    {
                        targetScanFlags |= TargetScanFlags.NeedNonBurning;
                    }
                    Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, targetScanFlags, null, 0f, 9999f);
                    if (thing != null)
                    {
                        __instance.pawn.TryStartAttack(thing);
                        __instance.collideWithPawns = true;
                        return;
                    }
                }
            }
        }

        private static bool onTower(IntVec3 sourceSq, Thing thing)
        {
            CompMainComponent compMain = (CompMainComponent)thing.TryGetComp(CompMainComponent.COMP_DEF);
            CompFieldOfViewWatcher compFoV = compMain.compFieldOfViewWatcher;


            return compFoV.OnTower(sourceSq);
        }
    }
}
