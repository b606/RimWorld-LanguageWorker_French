using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Grammar;

namespace RimWorld_LanguageWorker_French
{
	public static class LanguageWorkerPatcher
	{
		public const string __targetLanguage = "French";

		public static void DoPatching()
		{
			try
			{
				var harmony = new Harmony(id: "com.b606.mods.languageworker");
				var assembly = Assembly.GetExecutingAssembly();

				FileLog.Log("Installing com.b606.mods.languageworker...");
				FileLog.Log(string.Format("Active language: {0}",
					LanguageDatabase.activeLanguage.FriendlyNameEnglish));

				harmony.PatchAll(assembly);
				InspectPatches();

				FileLog.Log("Done.");
			}
			catch (Exception e)
			{
				FileLog.Log("Mod installation failed.");
				FileLog.Log(string.Format("Exception: {0}", e));
			}
		}

		// Retrieve all patches
		[Conditional("DEBUG")]
		public static void InspectPatches()
		{
			try
			{
				MethodInfo original = typeof(GrammarUtility).GetMethod("RulesForPawn",
					new Type[] {
						typeof(string), typeof(Name),
						typeof(string), typeof(PawnKindDef), typeof(Gender), typeof(Faction),
						typeof(int), typeof(int), typeof(string),
						typeof(bool), typeof(bool),
						typeof(bool), typeof(List<RoyalTitle>),
						typeof(Dictionary<string, string>), typeof(bool)
					}
				);
				var patches = Harmony.GetPatchInfo(original);
				if (patches != null)
				{
					FileLog.Log("Existing patches:");

					foreach (var patch in patches.Prefixes)
					{
						// already patched
						FileLog.Log("index: " + patch.index);
						FileLog.Log("owner: " + patch.owner);
						FileLog.Log("patch method: " + patch.PatchMethod);
						FileLog.Log("priority: " + patch.priority);
						FileLog.Log("before: " + patch.before);
						FileLog.Log("after: " + patch.after);
					}
				}
			}
			catch (Exception e)
			{
				FileLog.Log("Patch inspection failed.");
				FileLog.Log(string.Format("Exception: {0}", e));
			}
		}
	}
}
