using System;
using NUnit.Framework;
using RimWorld_LanguageWorker_French;

namespace RimWorldLanguageWorker_French_NUnitTest
{
  [TestFixture]
  public class TestPostProcessed
  {
    [Test]
    public void TestElision()
    {
      LanguageWorker_French _lw = new LanguageWorker_French();

      // h aspirated words are prefixed with zero-width space
      string template = "azerty\n\nazerty Viande de husky\n\nazerty";
      Assert.AreEqual("azerty\n\nazerty Viande de \u200Bhusky\n\nazerty", _lw.PostProcessed(template));

      template = "azerty\n\nazerty le haut-de-forme\n\nazerty";
      Assert.AreEqual("azerty\n\nazerty le \u200Bhaut-de-forme\n\nazerty", _lw.PostProcessed(template));

      template = "azerty\n\nazerty la harpe azerty\n\nazerty";
      Assert.AreEqual("azerty\n\nazerty la \u200Bharpe azerty\n\nazerty", _lw.PostProcessed(template));

      template = "azerty\n\nazerty Viande de humain\n\nazerty";
      Assert.AreEqual("azerty\n\nazerty Viande d'humain\n\nazerty", _lw.PostProcessed(template));

      template = "azerty\n\nazerty un lit de hôpital\n\nazerty";
      Assert.AreEqual("azerty\n\nazerty un lit d'hôpital\n\nazerty", _lw.PostProcessed(template));

    }
  }
}
