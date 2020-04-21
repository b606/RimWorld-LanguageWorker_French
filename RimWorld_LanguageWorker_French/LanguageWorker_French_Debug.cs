using System;
using System.Diagnostics;
using System.Reflection;
using Verse;

namespace RimWorld_LanguageWorker_French
{
	public partial class LanguageWorker_French
	{
		// General purpose logger
		public static Logger LogLanguageWorker = new Logger("LanguageWorker_French.log");

		// Light weight loggers, for diffs
		private static Logger logLanguageWorkerIn = new Logger("LanguageWorkerIn.log");
		private static Logger logLanguageWorkerOut = new Logger("LanguageWorkerOut.log");

		// Heavy logger, for stats and CPU usage
		private StatsLogger logStats = new StatsLogger();
		public StatsLogger LogStats { get => logStats; set => logStats = value; }

		[Conditional("DEBUG")]
		private void RecordInString(string a_str)
		{
			logLanguageWorkerIn.Message(a_str);
		}

		[Conditional("DEBUG")]
		private void RecordOutString(string a_str)
		{
			logLanguageWorkerOut.Message(a_str);
		}

		[Conditional("DEBUG")]
		private void StartStatsLogging(StackTrace callStack, string acontext = null)
		{
			LogStats.StartLogging(callStack, acontext);
		}

		[Conditional("DEBUG")]
		private void StopStatsLogging(string original, string processed_str)
		{
			LogStats.StopLogging(original, processed_str);
		}

		/// <summary>
		/// Temporary hacks:
		/// Ensure that the correct frame indices are used to to detect the string categories.
		/// Scan the callStack of ToTitleCase once during Debug and assert that
		/// the released functions bool Is*Name() use these indices.
		/// If not, the game code has changed.
		/// </summary>
		/// <returns>List of frame indices of the detected methods</returns>
		/// <param name="callStack">Call stack.</param>
		//NOTE: verified for version up to 1.1.2609 rev633
		[Conditional("DEBUG")]
		private void Debug_NameCategory_StackFrame(StackTrace callStack)
		{
			bool detectedPawnName = false;
			bool detectedGenerateName = false;

			for (int i = 0; i < callStack.FrameCount; i++)
			{
				StackFrame frame = callStack.GetFrame(i);
				MethodBase method = frame.GetMethod();

				// Detect if called from PawnBioAndNameGenerator.GeneratePawnName
				// bool IsPawnName() uses callStack.GetFrame(4) or callStack.GetFrame(5)
				if ((method.Name == "GeneratePawnName")
					&& method.DeclaringType.Equals(typeof(RimWorld.PawnBioAndNameGenerator)))
				{
					Debug.Assert((!detectedPawnName && i == 4)
							|| (detectedPawnName && i == 5));
					if (i == 4) { detectedPawnName = true; }
				}

				// Detect if called from  RimWorld.Planet.SettlementNameGenerator GenerateSettlementName
				// bool IsSettlementName() uses callStack.GetFrame(5)
				if (method.Name == "GenerateSettlementName")
				{
					Debug.Assert(i == 5);
				}

				// Detect if called from RimWorld.FeatureWorker AddFeature (WorldFeature names)
				// bool IsWorldFeatureName() uses callStack.GetFrame(5)
				if (method.Name == "AddFeature")
				{
					Debug.Assert(i == 5);
				}

				// Detect if called from RimWorld.FactionGenerator RimWorld.Faction NewGeneratedFaction
				// bool IsFactionName() uses callStack.GetFrame(5)
				if (method.Name == "NewGeneratedFaction")
				{
					Debug.Assert(i == 5);
				}

				// Detect if called from RimWorld.QuestGen.QuestNode_ResolveQuestName GenerateName
				// Need to test the DeclaringType
				// bool IsQuestName() uses callStack.GetFrame(3)
				if ((method.Name == "GenerateName")
					&& method.DeclaringType.Equals(typeof(RimWorld.QuestGen.QuestNode_ResolveQuestName)))
				{
					Debug.Assert(i == 3);
				}

				// Detect if called from RimWorld.CompArt GenerateTitle
				// bool IsArtName() uses callStack.GetFrame(2)
				if ((method.Name == "GenerateTitle")
					&& method.DeclaringType.Equals(typeof(RimWorld.CompArt)))
				{
					Debug.Assert(i == 2);
				}

				// Detect if called from RimWorld.NameGenerator GenerateName(Verse.Grammar.GrammarRequest,...)
				// Misleading: detect other types of names first.
				// bool IsName() uses callStack.GetFrame(2)
				if (method.Name == "GenerateName")
				{
					Debug.Assert(!detectedGenerateName && i == 2);
					if (i == 2) { detectedGenerateName = true; }
				}
			}
		}

		public string Debug_ToTitleCase(string str)
		{
			if (str.NullOrEmpty())
				return str;

			StackTrace callStack = new StackTrace();
			//Debug_NameCategory_StackFrame has [Conditional("DEBUG")] attribute
			Debug_NameCategory_StackFrame(callStack);

			string processed_str;

			// Split name categories for debugging purpose
			// NOTE: Tests order matters
			if (IsQuestName(callStack))
			{
				// The fastest to detect: callStack.GetFrame(3)
				// Capitalize only first letter (+ '\'')
				RecordInString("ToTitleCase(QuestName): " + str);
				processed_str = ToTitleCaseOther(str);
				RecordOutString("ToTitleCase(QuestName): " + processed_str);
			}
			else
			if (IsPawnName(callStack))
			{
				// callStack.GetFrame(4) or callStack.GetFrame(5)
				RecordInString("ToTitleCase(PawnName): " + str);
				processed_str = ToTitleCaseProperName(str);
				RecordOutString("ToTitleCase(PawnName): " + processed_str);
			}
			else
			if (IsSettlementName(callStack))
			{
				// callStack.GetFrame(5)
				RecordInString("ToTitleCase(SettlementName): " + str);
				processed_str = ToTitleCaseProperName(str);
				RecordOutString("ToTitleCase(SettlementName): " + processed_str);
			}
			else
			if (IsWorldFeatureName(callStack))
			{
				// callStack.GetFrame(5)
				RecordInString("ToTitleCase(WorldFeatureName): " + str);
				processed_str = ToTitleCaseProperName(str);
				RecordOutString("ToTitleCase(WorldFeatureName): " + processed_str);
			}
			else
			if (IsFactionName(callStack))
			{
				// callStack.GetFrame(5)
				RecordInString("ToTitleCase(FactionName): " + str);
				processed_str = ToTitleCaseProperName(str);
				RecordOutString("ToTitleCase(FactionName): " + processed_str);
			}
			else
			if (IsArtName(callStack))
			{
				// callStack.GetFrame(2)
				RecordInString("ToTitleCase(ArtName): " + str);
				processed_str = ToTitleCaseProperName(str);
				RecordOutString("ToTitleCase(ArtName): " + processed_str);
			}
			else
			if (IsName(callStack))
			{
				// Any other names generated by RimWorld.NameGenerator: callStack.GetFrame(2)
				// RimWorld.TradeShip
				RecordInString("ToTitleCase(OtherName): " + str);
				processed_str = ToTitleCaseOtherName(str);
				RecordOutString("ToTitleCase(OtherName): " + processed_str);
			}
			else
			{
				// Normal title : capitalize first letter.
				RecordInString("ToTitleCase(OtherTitle): " + str);
				processed_str = ToTitleCaseOther(str);
				RecordOutString("ToTitleCase(OtherTitle): " + processed_str);
			}

			return processed_str;
		}

	}
}
