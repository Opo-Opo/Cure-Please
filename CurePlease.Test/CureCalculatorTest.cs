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
                new CureSpell("Cure", true, 30),
                new CureSpell("Cure III", true, 90),
                new CureSpell("Cure II", true, 60),
            };

            var sut = new CureCalculator(cureTiers);

            Assert.IsNull(sut.CureFor(20));
            Assert.AreEqual("Cure", sut.CureFor(50));
            Assert.AreEqual("Cure II", sut.CureFor(60));
            Assert.AreEqual("Cure II", sut.CureFor(70));
            Assert.AreEqual("Cure III", sut.CureFor(90));
            Assert.AreEqual("Cure III", sut.CureFor(1000));
        }
    }
}
