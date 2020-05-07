// <code-header>
//   <author>b606</author>
//   <summary>
//		libHarmony patches manager.
//	 </summary>
// </code-header>

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
#if DEBUG
			Harmony.DEBUG = true;
#endif
			try
			{
				Harmony harmony = new Harmony(id: "com.b606.mods.languageworker");
				Assembly assembly = Assembly.GetExecutingAssembly();

				LanguageWorker_French.LogMessage("Installing com.b606.mods.languageworker...");
				LanguageWorker_French.LogMessage(string.Format("Active language: {0}",
					LanguageDatabase.activeLanguage.FriendlyNameEnglish));

				harmony.PatchAll(assembly);
				InspectPatches(harmony);

				LanguageWorker_French.LogMessage("Done.");
			}
			catch (Exception e)
			{
				LanguageWorker_French.LogMessage("Mod installation failed.");
				LanguageWorker_French.LogMessage(string.Format("Exception: {0}", e));
			}
		}

		// Retrieve all patches
		[Conditional("DEBUG")]
		public static void InspectPatches(Harmony harmony)
		{
			try
			{
				LanguageWorker_French.LogMessage("Existing patches:");

				IEnumerable<MethodBase> myOriginalMethods = harmony.GetPatchedMethods();
				foreach (MethodBase method in myOriginalMethods)
				{
					Patches patches = Harmony.GetPatchInfo(method);
					if (patches != null)
					{
						foreach (var patch in patches.Prefixes)
						{
							// already patched
							LanguageWorker_French.LogMessage("index: " + patch.index);
							LanguageWorker_French.LogMessage("owner: " + patch.owner);
							LanguageWorker_French.LogMessage("patch method: " + patch.PatchMethod);
							LanguageWorker_French.LogMessage("priority: " + patch.priority);
							LanguageWorker_French.LogMessage("before: " + patch.before.Join());
							LanguageWorker_French.LogMessage("after: " + patch.after.Join());
						}
					}
				}
			}
			catch (Exception e)
			{
				LanguageWorker_French.LogMessage("Patches inspection failed.");
				LanguageWorker_French.LogMessage(string.Format("Exception: {0}", e));
			}
		}
	}
}
