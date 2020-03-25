using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld_LanguageWorker_French
{
	public class LanguageWorker_French : LanguageWorker
	{
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
				if(arguments.Length == 0)
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

					if(oldValue == input)
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

		private static string PostProcess(string translation)
		{
			return _languageWorkerResolverRegex.Replace(translation, EvaluateResolver);
		}

		private static string EvaluateResolver(Match match)
		{
			string keyword = match.Groups["resolverName"].Value;

			Group argumentsGroup = match.Groups["argument"];

			string[] arguments = new string[argumentsGroup.Captures.Count];
			for(int i = 0; i < argumentsGroup.Captures.Count; ++i)
			{
				arguments[i] = argumentsGroup.Captures[i].Value.Trim();
			}

			IResolver resolver = GetResolverByKeyword(keyword);

			string result = resolver.Resolve(arguments);
			if(result == null)
			{
				Log.Error(string.Format("Error happened while resolving LW instruction: \"{0}\"", match.Value));
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

    public override string WithIndefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
    {
      //Names don't get articles
      if( name )
        return str;

      if( plural )
        return "des " + str;

      return (gender == Gender.Female ? "une " : "un ") + str;
    }

    public override string WithDefiniteArticle(string str, Gender gender, bool plural = false, bool name = false)
    {
      if( str.NullOrEmpty() )
        return str;

      //Names don't get articles
      if( name )
        return str;

      if( plural )
        return "les " + str;

      char first = str[0];

      if( IsVowel(first) )
        return "l'" + str;

      return (gender == Gender.Female ? "la " : "le ") + str;
    }

    public override string OrdinalNumber(int number, Gender gender = Gender.None)
    {
      return number == 1 ? number + "er" : number + "e";
    }

    public override string ToTitleCase(string str)
    {
        // Do nothing, capitalize only the first word
        return str;
    }

    public override string Pluralize(string str, Gender gender, int count = -1)
    {
      if( str.NullOrEmpty() )
        return str;

      // TODO: should str be lowercased before the tests?

      // words ending with "s", "x" or "z": do not change anything
      char last = str[str.Length - 1];
      if( last == 's' || last == 'x' || last == 'z' )
        return str;

      // exceptions to the next rules; only test words that could possibly be found in Rimworld
      switch( str ) {
        // "bail", "corail", "émail", "gemmail", "soupirail", "travail", "vantail", "vitrail": replace "ail" by "aux"
        case "travail":
          return str.Substring(0, str.Length - 3) + "aux";
        // "bleu", "émeu", "landau", "lieu", "pneu", "sarrau", "bal", "banal", "fatal", "final", "festival": append "s"
        case "bleu":
        case "émeu":
        // lieu : fish does not exist in RimWorld
        // case "lieu":
        case "banal":
        case "fatal":
        case "final":
          return str + "s";
        // "bijou", "caillou", "chou", "genou", "hibou", "joujou", "pou": append "x"
        case "bijou":
        case "caillou":
        case "genou":
        // lieu : area, takes an "x"
        case "lieu":
            return str + "x";
      }

      // words ending with "al": replace "al" by "aux"
      if( str.EndsWith("al") )
        return str.Substring(0, str.Length - 2) + "aux";

      // words ending with "au" or "eu": append "x"
      if( str.EndsWith("au") | str.EndsWith("eu") )
        return str + "x";

      // general case: append s
      return str + "s";
    }

    public override string PostProcessed(string str)
    {
      return PostProcessedInt(base.PostProcessed(str));
    }

    public override string PostProcessedKeyedTranslation(string translation)
    {
      return PostProcessedInt(base.PostProcessedKeyedTranslation(translation));
    }

    public bool IsVowel(char ch)
    {
      return "aàâäæeéèêëiîïoôöœuùüûhAÀÂÄÆEÉÈÊËIÎÏOÔÖŒUÙÜÛH".IndexOf(ch) >= 0;
    }

    private Regex ElisionE = new Regex(@"\b([cdjlmnst]|qu|quoiqu|lorsqu)e ([aàâäeéèêëiîïoôöuùüûh])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private Regex ElisionLa = new Regex(@"\b(l)a ([aàâäeéèêëiîïoôöuùüûh])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private Regex ElisionSi = new Regex(@"\b(s)i (ils?)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private Regex DeLe = new Regex(@"\b(d)e l(es?)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private Regex ALe = new Regex(@"\bà les?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private string PostProcessedInt(string str)
    {
      str = ElisionE.Replace(str, "$1'$2");
      str = ElisionLa.Replace(str, "$1'$2");
      str = ElisionSi.Replace(str, "$1'$2");
      str = DeLe.Replace(str, "$1$2");
      str = ALe.Replace(str, new MatchEvaluator(ReplaceALe));

      return str;
    }

    private string ReplaceALe(Match match) {
      switch (match.ToString()) {
        case "à le": return "au";
        case "à les": return "aux";
        case "\u00c0 le": return "Au";
        case "\u00c0 les": return "Aux";
      }
      return match.ToString();
    }
	}

}