using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW.Sources.RimWorldRealFoW.Detours
{
    static class _JobGiver_AITrashBuildingsDistant // ALSO SEE IF FIND CLOSEST ENEMY COULD BE USED. IF JOBGIVER FINDS NO ENEMY, MOVE TO TOWER. IF STILL NO ENEMY, THEN CONTINUE
    {
        static void TryGiveJob_Postfix(this ThinkNode_JobGiver __instance, ref Job __result, Pawn pawn)
        {
            if (!pawn.HostileTo(Faction.OfPlayer))
            {
                return;
            }
            
            IAttackTargetSearcher attackTargetSearcher = pawn;
            if (attackTargetSearcher.CurrentEffectiveVerb != null && !attackTargetSearcher.CurrentEffectiveVerb.verbProps.IsMeleeAttack)
            {
                Job rangedJob = FoWUtilities.tryShootFromTower(pawn);
                if (rangedJob != null)
                {
                    __result = rangedJob;
                }
            }
        }
    }
}
