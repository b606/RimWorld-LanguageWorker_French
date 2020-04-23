using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld_LanguageWorker_French
{
	public partial class LanguageWorker_French : LanguageWorker
	{
		#region IResolver Support
		private interface IResolver
		{
			string Resolve(string[] arguments);
		}

		private class ReplaceResolver : IResolver
		{
			// ^Replace('{0}', 'Мартомай'-'Мартомая', 'Июгуст'-'Июгуста', 'Сентоноябрь'-'Сентоноября', 'Декавраль'-'Декавраля')^
			private static readonly Regex _argumentRegex = new Regex(@"'(?<old>[^']*?)'-'(?<new>[^']*?)'", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if (arguments.Length == 0)
				{
					return null;
				}

				string input = arguments[0];

				if (arguments.Length == 1)
				{
					return input;
				}

				for (int i = 1; i < arguments.Length; ++i)
				{
					string argument = arguments[i];

					Match match = _argumentRegex.Match(argument);
					if (!match.Success)
					{
						return null;
					}

					string oldValue = match.Groups["old"].Value;
					string newValue = match.Groups["new"].Value;

					if (oldValue == input)
					{
						return newValue;
					}
					//Log.Message(string.Format("input: {0}, old: {1}, new: {2}", input, oldGroup.Captures[i].Value, newGroup.Captures[i].Value));
				}

				return input;
			}
		}

		private class NumberCaseResolver : IResolver
		{
			// '3.14': 1-'прошёл # день', 2-'прошло # дня', X-'прошло # дней'
			private static readonly Regex _numberRegex = new Regex(@"(?<floor>[0-9]+)(\.(?<frac>[0-9]+))?", RegexOptions.Compiled);

			public string Resolve(string[] arguments)
			{
				if (arguments.Length != 4)
				{
					return null;
				}

				string numberStr = arguments[0];
				Match numberMatch = _numberRegex.Match(numberStr);
				if (!numberMatch.Success)
				{
					return null;
				}

				bool hasFracPart = numberMatch.Groups["frac"].Success;

				string floorStr = numberMatch.Groups["floor"].Value;

				string formOne = arguments[1].Trim('\'');
				string formSeveral = arguments[2].Trim('\'');
				string formMany = arguments[3].Trim('\'');

				if (hasFracPart)
				{
					return formSeveral.Replace("#", numberStr);
				}

				int floor = int.Parse(floorStr);
				return GetFormForNumber(floor, formOne, formSeveral, formMany).Replace("#", numberStr);
			}

			private static string GetFormForNumber(int number, string formOne, string formSeveral, string formMany)
			{
				int firstPos = number % 10;
				int secondPos = number / 10 % 10;

				if (secondPos == 1)
				{
					return formMany;
				}

				switch (firstPos)
				{
					case 1:
						return formOne;
					case 2:
					case 3:
					case 4:
						return formSeveral;
					default:
						return formMany;
				}
			}
		}

		private static readonly ReplaceResolver replaceResolver = new ReplaceResolver();
		private static readonly NumberCaseResolver numberCaseResolver = new NumberCaseResolver();

		private static readonly Regex _languageWorkerResolverRegex = new Regex(@"\^(?<resolverName>\w+)\(\s*(?<argument>[^|]+?)\s*(\|\s*(?<argument>[^|]+?)\s*)*\)\^", RegexOptions.Compiled);

		private static string PostProcessResolver(string translation)
		{
			return _languageWorkerResolverRegex.Replace(translation, EvaluateResolver);
		}

		private static string EvaluateResolver(Match match)
		{
			string keyword = match.Groups["resolverName"].Value;

			Group argumentsGroup = match.Groups["argument"];

			string[] arguments = new string[argumentsGroup.Captures.Count];
			for (int i = 0; i < argumentsGroup.Captures.Count; ++i)
			{
				arguments[i] = argumentsGroup.Captures[i].Value.Trim();
			}

			IResolver resolver = GetResolverByKeyword(keyword);

			string result = resolver.Resolve(arguments);
			if (result == null)
			{
				try
				{
					Log.Error(string.Format("Error happened while resolving LW instruction: \"{0}\"", match.Value));
				}
				catch (MissingMethodException e)
				{
					// Unit test does not initialize Verse.Log for some reason
					Console.WriteLine("Log.Message: {0}", e.Message);
				}
				return match.Value;
			}

			return result;
		}

		private static IResolver GetResolverByKeyword(string keyword)
		{
			switch (keyword)
			{
				case "Replace":
					return replaceResolver;
				case "Number":
					return numberCaseResolver;
				default:
					return null;
			}
		}

		// Temporary resolver test
		// French language does not use this mechanism yet.
		public string TestResolver(string str)
		{
			return PostProcessResolver(str);
		}
		#endregion

		// in plural, replace "ail" with "aux"
		private static readonly HashSet<string> Exceptions_Plural_aux = new HashSet<string> {
			"bail",
			"corail",
			"émail",
			"gemmail",
			"soupirail",
			"travail",
			"vantail",
			"vitrail"
		};

		// lieu (fish) takes an "s", but does not exist in RimWorld
		private static readonly HashSet<string> Exceptions_Plural_s = new HashSet<string> {
			"bleu",
			"émeu",
			"landau",
			"pneu",
			"sarrau",
			"bal",
			"banal",
			"fatal",
			"final",
			"festival"
		};

		// lieu (area) takes an "x", it exists in RimWorld (ex. lieu d'assemblage-crafting spot)
		private static readonly HashSet<string> Exceptions_Plural_x = new HashSet<string> {
			"bijou",
			"caillou",
			"chou",
			"genou",
			"hibou",
			"joujou",
			"pou",
			"lieu"
		};

		// Words with aspirated h do not get elision (list only words in RimWorld)
		// Added no elision to "onze", "onzième" -- do not appear in RiWorld yet)
		private static readonly HashSet<string> Exceptions_No_Elision = new HashSet<string> {
			"hache",
			"hack",
			"haine",
			"hameau",
			"hampe",
			"hamster",
			"hanche",
			"hareng",
			"haricot",
			"harpe",
			"hasard",
			"hase", // "hases",
      "hât", // "hâte", "hâtif", "hâtive", "hâtivement",
      "haut", // "haute",
      "héron",
			"hérisson",
			"hêtre",
			"hibou",
			"holding",
			"homard",
			"honte",
			"horde",
			"hors", // "hors-la-loi",
      "houblon",
			"huit",
			"hunter",
			"hurl", // "hurler", "hurle", "hurlé", "hurlement",
      "husky",
			"hutte",
			"hyène",
			"onz" // , "onze", "onzième"
    };

		// For ToTitleCase: No uppercase if in the middle of the string
		private static HashSet<string> NonUppercaseWords = new HashSet<string>
		{
			"à", // "à la",
			"au",
			"avec",
			"de", // "de la",
			"des",
			"du",
			"en",
			"et",
			"la",
			"le",
			"les",
			"lez",
			"par",
			"pour",
			"sur", // "sur le", "sur la",
			"vers",
			"van",
			"von"
		};

		public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			// TODO: names with h do not get elision
			// TODO: short names (length < 5) with vowels do not get elision
			//Names don't get articles
			if (name)
				return str;

			if (plural)
				return "des " + str;

			return (gender == Gender.Female ? "une " : "un ") + str;
		}

		public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
		{
			if (str.NullOrEmpty())
				return str;

			//Names don't get articles
			if (name)
				return str;

			if (plural)
				return "les " + str;

			char first = str[0];

			if (IsVowel(first) && !str.StartsWith("onz", StringComparison.CurrentCulture))
			{
				// General rule for vowels not starting with "onz" (onze, onzième).
				// Unaspirated h are processed with elision rules
				return "l'" + str;
			}

			// General rule for consonants
			return (gender == Gender.Female ? "la " : "le ") + str;
		}

		public override string OrdinalNumber(int number, Gender gender = Gender.None)
		{
			return number == 1 ? number + "er" : number + "e";
		}

		#region StackTrace helpers
		// Detect if called from PawnBioAndNameGenerator.GeneratePawnName
		bool IsPawnName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(4).GetMethod();
			if ((method.Name == "GeneratePawnName")
					&& method.DeclaringType.Equals(typeof(RimWorld.PawnBioAndNameGenerator)))
				return true;
			method = callStack.GetFrame(5).GetMethod();
			if ((method.Name == "GeneratePawnName")
					&& method.DeclaringType.Equals(typeof(RimWorld.PawnBioAndNameGenerator)))
				return true;

			return false;
		}

		// Detect if called from  RimWorld.Planet.SettlementNameGenerator GenerateSettlementName
		bool IsSettlementName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(5).GetMethod();
			if (method.Name == "GenerateSettlementName")
				return true;

			return false;
		}

		// Detect if called from RimWorld.FeatureWorker AddFeature
		bool IsWorldFeatureName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(5).GetMethod();
			if (method.Name == "AddFeature")
				return true;

			return false;
		}

		// Detect if called from RimWorld.FactionGenerator RimWorld.Faction NewGeneratedFaction
		bool IsFactionName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(5).GetMethod();
			if (method.Name == "NewGeneratedFaction")
				return true;

			return false;
		}

		// Detect if called from RimWorld.QuestGen.QuestNode_ResolveQuestName GenerateName
		bool IsQuestName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(3).GetMethod();
			if ((method.Name == "GenerateName")
				&& method.DeclaringType.Equals(typeof(RimWorld.QuestGen.QuestNode_ResolveQuestName)))
				return true;

			return false;
		}

		// Detect if called from RimWorld.CompArt GenerateTitle
		bool IsArtName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(2).GetMethod();
			if ((method.Name == "GenerateTitle")
				&& method.DeclaringType.Equals(typeof(RimWorld.CompArt)))
				return true;

			return false;
		}

		// Detect if called from RimWorld.NameGenerator GenerateName
		bool IsName(StackTrace callStack)
		{
			MethodBase method = callStack.GetFrame(2).GetMethod();
			if (method.Name == "GenerateName")
				return true;

			return false;
		}
		#endregion // StackTrace helpers

		/// <summary>
		/// ToTitleCase for pawns, world features and settlements names.
		/// Pawn names may contain '<Nickname>'.
		/// The first word is always capitalized and all words are capitalized
		/// except those listed in NonUppercaseWords and "d'".
		/// N0TE: last name in NameTriple is not capitalized by the default
		/// LanguageWorker if generated from tribal words.
		/// </summary>
		/// <returns>The title case string for a proper name.</returns>
		/// <param name="str">String.</param>
		public string ToTitleCaseProperName(string str)
		{
			StartStatsLogging(new StackTrace());
			if (str.NullOrEmpty())
				return str;

			string[] array = str.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				string str2 = array[i];
				// Check if uppercase is needed, the first word is always capitalized.
				if ((i > 0) && NonUppercaseWords.Contains(str2.ToLower()))
				{
					array[i] = str2.ToLower();
					continue;
				}

				// Capitalize word: skip "'", "d'" and "l'"
				char firstChar = str2[0];
				switch (firstChar)
				{
					case '\'':
						array[i] = "'" + str2.Substring(1).CapitalizeHyphenated();
						break;
					default:
						if (str2.Length == 2)
						{
							array[i] = str2.CapitalizeHyphenated();
							break;
						}
						if (str2.StartsWith("d'", StringComparison.CurrentCulture)
							|| str2.StartsWith("D'", StringComparison.CurrentCulture)
							|| str2.StartsWith("l'", StringComparison.CurrentCulture)
							|| str2.StartsWith("L'", StringComparison.CurrentCulture))
						{
							// First word always capitalized
							array[i] = ((i == 0) ? str2[0].ToString().ToUpper() : str2[0].ToString().ToLower()) +
												"'" + str2.Substring(2).CapitalizeHyphenated();
						}
						else
						{
							// default rule
							array[i] = str2.CapitalizeHyphenated();
						}
						break;
				}
			}
			string processed_str = string.Join(" ", array);

			StopStatsLogging(str, processed_str);
			return processed_str;
		}

		/// <summary>
		/// ToTitleCase for other names, mostly starting with determinant (le, la, les),
		/// ex TradeShip and faction names.
		/// Business types and political unions do not follow the same Cap rules
		/// and have more complicated logic.
		/// Do not capitalize the first determinant for inclusion in generated sentences
		/// in the rulepack defs.
		/// </summary>
		/// <returns>The title case for other names.</returns>
		/// <param name="str">String.</param>
		public string ToTitleCaseOtherName(string str)
		{
			StartStatsLogging(new StackTrace());
			if (str.NullOrEmpty())
				return str;

			string[] array = str.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				string str2 = array[i];

				// if the first word is le/la/les/l'.
				if (i == 0)
				{
					char deter = str2[0];
					if ((deter == 'l') || (deter == 'L'))
					{
						string tmp = str2.ToLower();
						if (tmp.Equals("le") || tmp.Equals("la") || tmp.Equals("les"))
						{
							array[i] = tmp;
							continue;
						}

						if (str2.StartsWith("l'", StringComparison.CurrentCulture)
							|| str2.StartsWith("L'", StringComparison.CurrentCulture))
						{
							// First word always capitalized
							array[i] = "l'" + str2.Substring(2).CapitalizeHyphenated();
							continue;
						}
					}
				}

				// Continue processing
				if (NonUppercaseWords.Contains(str2.ToLower()))
				{
					array[i] = str2.ToLower();
					continue;
				}

				// Same as ToTitleCaseProperName
				// Capitalize word: skip "'", "d'" and "l'"
				char first = str2[0];
				switch (first)
				{
					case '\'':
						array[i] = "'" + str2.Substring(1).CapitalizeHyphenated();
						break;
					default:
						if (str2.Length == 2)
						{
							array[i] = str2.CapitalizeHyphenated();
							break;
						}

						if (str2.StartsWith("d'", StringComparison.CurrentCulture)
							|| str2.StartsWith("D'", StringComparison.CurrentCulture)
							|| str2.StartsWith("l'", StringComparison.CurrentCulture)
							|| str2.StartsWith("L'", StringComparison.CurrentCulture))
						{
							// First word always capitalized
							array[i] = ((i == 0) ? str2[0].ToString().ToUpper() : str2[0].ToString().ToLower()) +
												"'" + str2.Substring(2).CapitalizeHyphenated();
						}
						else
						{
							// default rule
							array[i] = str2.CapitalizeHyphenated();
						}
						break;
				}
			}
			string processed_str = string.Join(" ", array);

			StopStatsLogging(str, processed_str);
			return processed_str;
		}

		/// <summary>
		/// ToTitleCase for other categories: mostly quest titles.
		/// </summary>
		/// <returns>The title case other.</returns>
		/// <param name="str">String.</param>
		public string ToTitleCaseOther(string str)
		{
			// TODO: pas de capitalisation après "[uU]n", "[uU]ne"
			StartStatsLogging(new StackTrace());
			if (str.NullOrEmpty())
				return str;

			int num = str.FirstLetterBetweenTags();
			string str2 = (num == 0) ? str[num].ToString().ToUpper() : (str.Substring(0, num) + char.ToUpper(str[num]));
			string processed_str = str2 + str.Substring(num + 1);

			StopStatsLogging(str, processed_str);
			return processed_str;
		}

		public override string ToTitleCase(string str)
		{
			if (str.NullOrEmpty())
				return str;

			StackTrace callStack = new StackTrace();
			string processed_str;

#if DEBUG
			processed_str = Debug_ToTitleCase(str, callStack);
#else
			// Split name categories
			// NOTE: Tests order matters
			if (IsQuestName(callStack)
					|| IsArtName(callStack)
			)
			{
				// Capitalize only first letter (+ '\'')
				processed_str = ToTitleCaseOther(str);
			}
			else
			if (IsPawnName(callStack)
					|| IsSettlementName(callStack)
					|| IsWorldFeatureName(callStack)
				)
			{
				processed_str = ToTitleCaseProperName(str);
			}
			else
			if (IsName(callStack))
			{
				// Any other names ex. FactionName, RimWorld.TradeShip
				processed_str = ToTitleCaseOtherName(str);
			}
			else
			{
				// Normal title : capitalize first letter.
				processed_str = ToTitleCaseOther(str);
			}
#endif
			return processed_str;
		}

		public override string Pluralize(string str, Gender gender, int count = -1)
		{
			// FIXME: pluralize for compound words
			if (str.NullOrEmpty())
				return str;

			// Do not pluralize
			if (count == 1)
				return str;

			// Exceptions to general rules for plural
			string item = str.ToLower();
			if (Exceptions_Plural_aux.Contains(item))
			{
				return str.Substring(0, str.Length - 3) + "aux";
			}
			if (Exceptions_Plural_s.Contains(item))
			{
				return str + "s";
			}
			if (Exceptions_Plural_x.Contains(item))
			{
				return str + "x";
			}

			// words ending with "s", "x" or "z": do not change anything
			char last = str[str.Length - 1];
			if (last == 's' || last == 'x' || last == 'z')
				return str;

			// words ending with "al": replace "al" by "aux"
			if (str.EndsWith("al", StringComparison.CurrentCulture))
				return str.Substring(0, str.Length - 2) + "aux";

			// words ending with "au" or "eu": append "x"
			if (str.EndsWith("au", StringComparison.CurrentCulture) | str.EndsWith("eu", StringComparison.CurrentCulture))
				return str + "x";

			// general case: append s
			return str + "s";
		}

		public override string PostProcessed(string str)
		{
			//StartStatsLogging(new StackTrace());
			string processed_str = PostProcessedFrenchGrammar(base.PostProcessed(str));
			//StopStatsLogging(str, processed_str);
			return processed_str;
		}

		public override string PostProcessedKeyedTranslation(string translation)
		{
			//StartStatsLogging(new StackTrace());
			string processed_str = PostProcessedFrenchGrammar(base.PostProcessedKeyedTranslation(translation));
			//StopStatsLogging(translation, processed_str);
			return processed_str;
		}

		public bool IsVowel(char ch)
		{
			//Do not include [hH]
			return "aàâäæeéèêëiîïoôöœuùüûAÀÂÄÆEÉÈÊËIÎÏOÔÖŒUÙÜÛ".IndexOf(ch) >= 0;
		}

		// TODO: french typography, add space before [:;?!]
		// The Regex ([<][^>]*[>]|) component takes any XML tag into account,
		// ex. the name color tag <color=#D09B61FF> or <Name>
		private static readonly Regex WordsWithoutElision = new Regex(@"\b(h[^ <>]+|onz[^ <>]+)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		// NOTE: exception "lorsque aucun", "lorsque aucun", "lorsque avec", "lorsque <prenom>"
		private static readonly Regex ElisionE = new Regex(@"\b([cdjlmnst]|qu|quoiqu|lorsqu)e ([<][^>]*[>]|)([aàâäæeéèêëiîïoôöœuùüûh])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ElisionLa = new Regex(@"\b(l)a ([<][^>]*[>]|)([aàâäæeéèêëiîïoôöœuùüûh])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ElisionSi = new Regex(@"\b(s)i (ils?)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		// possessive + vowel/h muet. ex. instead of "sa épée" -> "son épée", "son/sa oreille" -> "son oreille"
		private static readonly Regex PossessiveVowel = new Regex(@"\b([mst])(on/[mst]|)a ([<][^>]*[>]|)([aàâäæeéèêëiîïoôöœuùüûh])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex DeLe = new Regex(@"\b(d)e ([<][^>]*[>]|)le ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex DeLes = new Regex(@"\b(d)e ([<][^>]*[>]|)l(es) ", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static readonly Regex ALe = new Regex(@"\bà les?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		private static string PostProcessedFrenchGrammar(string str)
		{
			// "[dD]e des" are generated by some rules of type "de [x_indefinite]" in plural
			str = str.Replace(" de des ", " des ")
				.Replace("De des ", "Des ");

			str = WordsWithoutElision.Replace(str, new MatchEvaluator(ReplaceNoElision));
			str = ElisionE.Replace(str, "$1'$2$3");
			str = ElisionLa.Replace(str, "$1'$2$3");
			str = ElisionSi.Replace(str, "$1'$2");
			str = PossessiveVowel.Replace(str, "$1on $3$4");
			str = DeLe.Replace(str, "$1u $2");
			str = DeLes.Replace(str, "$1$3 $2");
			str = ALe.Replace(str, new MatchEvaluator(ReplaceALe));

			// Clean out zero-width space
			return str.Replace("\u200B","");
		}

		private static string ReplaceALe(Match match)
		{
			switch (match.ToString())
			{
				case "à le": return "au";
				case "à les": return "aux";
				case "\u00c0 le": return "Au";
				case "\u00c0 les": return "Aux";
			}
			return match.ToString();
		}

		private static string ReplaceNoElision(Match match)
		{
			string item_raw = match.ToString();
			string item = item_raw.ToLower();
			foreach (var s in Exceptions_No_Elision)
			{
				if (item.StartsWith(s, StringComparison.CurrentCulture))
				{
					// Add zero-width space to foul the elision rules
					return ("\u200B" + item_raw);
				}
			}
			return item_raw;
		}
	}
}