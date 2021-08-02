using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RimWorldRealFoW
{
    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter",new Type[] {
        typeof(Letter),
        typeof(string)
    })]
    internal class LetterSuppress
    {
        [HarmonyPrefix]
        public static bool ReceiveLetterPrefix(ref Letter let)
        {
            
            if (let.def == LetterDefOf.NegativeEvent && RFOWSettings.hideEventNegative)
            {
                return false;
            }
            if (let.def == LetterDefOf.NeutralEvent && RFOWSettings.hideEventNeutral)
            {
                return false;
            }
            if (let.def == LetterDefOf.PositiveEvent && RFOWSettings.hideEventPositive)
            {
                return false;
            }
            if (let.def == LetterDefOf.ThreatBig && RFOWSettings.hideThreatBig)
            {
                return false;
            }
            if (let.def == LetterDefOf.ThreatSmall && RFOWSettings.hideThreatSmall)
            {
                return false;
            }

            return true;
        }
    }
}
