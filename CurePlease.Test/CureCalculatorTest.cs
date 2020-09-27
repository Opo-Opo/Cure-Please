using System.Collections.Generic;
using CurePlease.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CurePlease.Test
{
    [TestClass]
    public class CureCalculatorTest
    {
        [TestMethod]
        public void CureTier()
        {
            var cureTiers = new List<CureSpell>()
            {
                new CureSpell("Cure", false, 100),
                new CureSpell("Cure II", false, 300),
                new CureSpell("Cure III", true, 600),
                new CureSpell("Cure IV", true, 1100),
                new CureSpell("Cure V", true, 1300),
            };

            var sut = new CureCalculator(cureTiers);

            // Disabled Spells
            Assert.IsNull(sut.CureFor(100));
            Assert.IsNull(sut.CureFor(300));
            // Enabled Spells
            Assert.AreEqual("Cure III", sut.CureFor(600));
            Assert.AreEqual("Cure III", sut.CureFor(1099));
            Assert.AreEqual("Cure IV", sut.CureFor(1100));
            Assert.AreEqual("Cure IV", sut.CureFor(1299));
            Assert.AreEqual("Cure V", sut.CureFor(1300));
        }
    }
}
