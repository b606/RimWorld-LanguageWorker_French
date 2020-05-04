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

				FileLog.Log("Installing com.b606.mods.languageworker...");
				FileLog.Log(string.Format("Active language: {0}",
					LanguageDatabase.activeLanguage.FriendlyNameEnglish));

				harmony.PatchAll(assembly);
				InspectPatches(harmony);

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
		public static void InspectPatches(Harmony harmony)
		{
			try
			{
				FileLog.Log("Existing patches:");

				IEnumerable<MethodBase> myOriginalMethods = harmony.GetPatchedMethods();
				foreach (MethodBase method in myOriginalMethods)
				{
					Patches patches = Harmony.GetPatchInfo(method);
					if (patches != null)
					{
						foreach (var patch in patches.Prefixes)
						{
							// already patched
							FileLog.Log("index: " + patch.index);
							FileLog.Log("owner: " + patch.owner);
							FileLog.Log("patch method: " + patch.PatchMethod);
							FileLog.Log("priority: " + patch.priority);
							FileLog.Log("before: " + patch.before.Join());
							FileLog.Log("after: " + patch.after.Join());
						}
					}
				}
			}
			catch (Exception e)
			{
				FileLog.Log("Patches inspection failed.");
				FileLog.Log(string.Format("Exception: {0}", e));
			}
		}
	}
}
