using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace RimWorld_LanguageWorker_French
{

	[HarmonyPatch(typeof(GrammarUtility))]
	[HarmonyPatch("RulesForPawn")]
	[HarmonyPatch(new Type[] {
		typeof(string), typeof(Name),
		typeof(string), typeof(PawnKindDef), typeof(Gender), typeof(Faction),
		typeof(int), typeof(int), typeof(string),
		typeof(bool), typeof(bool),
		typeof(bool), typeof(List<RoyalTitle>),
		typeof(Dictionary<string, string>), typeof(bool) })]
	class RulesForPawnPatch
	{
		private class PhysicalCharacter
		{
			private string kindLabel;
			private Gender gender;

			public PhysicalCharacter(string label, Gender gender)
			{
				this.kindLabel = label;
				this.gender = gender;
			}

			public string KindLabel { get => kindLabel; set => kindLabel = value; }
			public Gender Gender { get => gender; set => gender = value; }
		}

		[HarmonyPrefix]
		static bool RulesforPawnPrefix(string pawnSymbol, Name name,
			string title, ref PawnKindDef kind, ref Gender gender, Faction faction,
			int age, int chronologicalAge, string relationInfo,
			bool everBeenColonistOrTameAnimal, bool everBeenQuestLodger,
			bool isFactionLeader, List<RoyalTitle> royalTitles,
			Dictionary<string, string> constants, bool addTags
			//, out PhysicalCharacter __state
			)
		{
			// Save the PhysicalCharacter because it might be overwritten in the patch
			// with "GrammaticalCharacter".
			//__state = new PhysicalCharacter(kind.label, gender);

			// if the current language is not the target, do nothing
			LanguageWorker_French.LogMessage("-----------");
			LanguageWorker_French.LogMessage(string.Format("RulesforPawnPrefix: {0}", LanguageDatabase.activeLanguage.FriendlyNameEnglish));
			if (!LanguageDatabase.activeLanguage.FriendlyNameEnglish.Equals(LanguageWorkerPatcher.__targetLanguage))
				return true;

			// change kind.label and gender according to the language grammar
			LanguageWorker_French.FixPawnGender(ref kind, ref gender, relationInfo);

			// continue to the original GrammarUtility.RulesforPawn
			return true;
		}

		[HarmonyPostfix]
		static IEnumerable<Rule> RulesforPawnPostfix(IEnumerable<Rule> __result
			//,ref PawnKindDef kind, ref Gender gender, PhysicalCharacter __state
			)
		{
			// Restore the PhysicalCharacter.
			//kind.label = __state.KindLabel;
			//gender = __state.Gender;

			if (__result != null)
			{
				LanguageWorker_French.LogMessage("result: " + __result);
				foreach (Rule r in __result)
					LanguageWorker_French.LogMessage(r.ToString());
			}
			else
				LanguageWorker_French.LogMessage("result == null");

			return __result;
		}
	}
}
