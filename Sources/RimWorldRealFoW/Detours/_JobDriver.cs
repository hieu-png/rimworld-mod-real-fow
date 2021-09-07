using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace RimWorldRealFoW.Sources.RimWorldRealFoW.Detours
{
    public static class _JobDriver
    {
        public static void SetupToils_Postfix(JobDriver __instance) // LIKELY BETTER USE WITH PreTickAction. WILL REVISIT IF ISSUES FOUND
        {
            if (!(__instance is JobDriver_AttackMelee))
            {
                return;
            }

            JobDriver_AttackMelee jobDriver = (JobDriver_AttackMelee) __instance;

            List<Toil> toils = Traverse.Create(jobDriver).Field("toils").GetValue<List<Toil>>();

            if (toils.Count() > 0)
            {
                Toil toil = toils.ElementAt(2);

                Pawn pawn = jobDriver.pawn;

                if (pawn == null) return;

                bool hasRangedWeapon = !pawn.CurrentEffectiveVerb.IsMeleeAttack;
                

                toil.AddPreTickAction(delegate
                {
                    if (pawn.IsHashIntervalTick(30) &&
                        !pawn.IsBurning() &&
                        jobDriver.job.targetA.Pawn == null &&
                        !pawn.CurrentEffectiveVerb.IsMeleeAttack &&
                        (pawn.Drafted || !pawn.IsColonist) && !pawn.Downed)
                    {
                        bool hasTargetablePawn =
                            !(from x in pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn)
                                where !x.ThreatDisabled(pawn) && x.Thing.Faction == Faction.OfPlayer && MapUtils
                                    .getMapComponentSeenFog(pawn.Map).isShown(pawn.Faction, x.Thing.Position)
                                select x).EnumerableNullOrEmpty();

                        if (hasTargetablePawn)
                        {
                            __instance.EndJobWith(JobCondition.InterruptForced); // Experiment with ways to end job
                        }
                    }
                });

            }
        }
    }
}
