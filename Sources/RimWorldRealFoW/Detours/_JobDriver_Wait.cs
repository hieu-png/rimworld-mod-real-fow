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
        static void CheckForAutoAttack_Postfix(JobDriver __instance) // NOTE FOR LATER: CHANGE ONTOWER CHECK TO BE AROUND BESTSHOOTTARGETFROMCURRENTPOSITION OR TRYSTARTATTACK SO THAT A TARGET CAN BE IDENTIFIED FOR IF THE ENEMY IS ONTOWER
        {
            bool shootingAtTower = false;

            CompMainComponent compMain = (CompMainComponent)__instance.pawn.TryGetComp(CompMainComponent.COMP_DEF);

            CompFieldOfViewWatcher compFoV = compMain?.compFieldOfViewWatcher;

            if (compFoV == null)
                return;

            if ((__instance.pawn.story == null ||
                 !__instance.pawn.story.DisabledWorkTagsBackstoryAndTraits.HasFlag(WorkTags.Violent))
                && __instance.pawn.Faction != null
                && (__instance.pawn.drafter == null || __instance.pawn.drafter.FireAtWill))
            {
                Verb currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
                if (currentEffectiveVerb == null || currentEffectiveVerb.verbProps.IsMeleeAttack) return;

                TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat;
                if (currentEffectiveVerb.IsIncendiary())
                {
                    targetScanFlags |= TargetScanFlags.NeedNonBurning;
                }

                Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, targetScanFlags, null, 0f, 9999f);
                if (thing == null) return;

                if (!onTower(thing.Position, compFoV) &&
                    !onTower(__instance.pawn.Position, compFoV)) return;
                
                __instance.pawn.TryStartAttack(thing);
                __instance.collideWithPawns = true;
            }
        }

        private static bool onTower(IntVec3 sourceSq, CompFieldOfViewWatcher compFoV)
        {
            return compFoV.OnTower(sourceSq);
        }

        private static bool isUnderRoof(IntVec3 sourceSq, CompFieldOfViewWatcher compFoV)
        {
            return compFoV.IsUnderRoof(sourceSq);
        }
    }
}
