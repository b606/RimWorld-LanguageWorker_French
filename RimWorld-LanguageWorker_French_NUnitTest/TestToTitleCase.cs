using NUnit.Framework;
using RimWorld_LanguageWorker_French;
using System;

namespace RimWorldLanguageWorker_French_NUnitTest
{
	[TestFixture()]
	public class TestToTitleCase
	{
		[Test()]
		public void TestPawnNames()
		{
			LanguageWorker_French _lw = new LanguageWorker_French();

			// Simple name
			string template = "lloraga crica";
			Assert.AreEqual("Lloraga Crica", _lw.ToTitleCasePawnName(template));

			// Name triple
			template = "cambiar 'tortue' legua";
			Assert.AreEqual("Cambiar 'Tortue' Legua", _lw.ToTitleCasePawnName(template));

			template = "Cagoguaxo éléphante de mer";
			// TODO: Should be "Cagoguaxo Éléphante de mer"
			Assert.AreEqual("Cagoguaxo Éléphante de Mer", _lw.ToTitleCasePawnName(template));

			template = "charles de gaulle";
			Assert.AreEqual("Charles de Gaulle", _lw.ToTitleCasePawnName(template));

			template = "charles De gaulle";
			Assert.AreEqual("Charles de Gaulle", _lw.ToTitleCasePawnName(template));

			template = "de gaulle";
			Assert.AreEqual("De Gaulle", _lw.ToTitleCasePawnName(template));

			template = "d'Autriche";
			Assert.AreEqual("D'Autriche", _lw.ToTitleCasePawnName(template));

			template = "werner von braun";
			Assert.AreEqual("Werner von Braun", _lw.ToTitleCasePawnName(template));

			template = "gérard D'aboville";
			Assert.AreEqual("Gérard d'Aboville", _lw.ToTitleCasePawnName(template));

			template = "antoine-françois gérard";
			Assert.AreEqual("Antoine-François Gérard", _lw.ToTitleCasePawnName(template));

			template = "marie-Thérèse d'autriche";
			Assert.AreEqual("Marie-Thérèse d'Autriche", _lw.ToTitleCasePawnName(template));

			template = "l'appel de cthulhu";
			Assert.AreEqual("L'Appel de Cthulhu", _lw.ToTitleCasePawnName(template));
		}
	}
}
