using System.Diagnostics;
using HarmonyLib;
using Verse;
using Verse.Grammar;

namespace RimWorld_LanguageWorker_French
{
	[HarmonyPatch(typeof(BattleLogEntry_RangedImpact))]
	[HarmonyPatch("GenerateGrammarRequest")]
	public class InspectionPatch
	{
		[HarmonyPostfix]
		static void Postfix(GrammarRequest __result)
		{
#if DEBUG
			LanguageWorker_French.LogMessage("--InspectionPatch called...");
			LanguageWorker_French.LogMessage("result: " + __result);
			foreach (Rule r in __result.Rules)
			{
				LanguageWorker_French.LogMessage(r.ToString());
			}
			LanguageWorker_French.LogMessage("constants: " + __result.Constants);
			if (__result.Constants != null)
			{
				foreach (var c in __result.Constants)
					LanguageWorker_French.LogMessage(c.Key + "=" + c.Value);
			}
#endif
		}
	}
}
