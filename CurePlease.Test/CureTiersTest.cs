using CurePlease.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CurePlease.Test
{
    [TestClass]
    public class CureTiersTest
    {
        [TestMethod]
        public void CastCure6()
        {
            var sut = new CureTiers(s => true, false, false, false);
            Assert.AreEqual("Cure VI", sut.Tier("Cure VI"));
        }
        
        [TestMethod]
        public void CastCure6_OverCure()
        {
            var sut = new CureTiers(s => false, true, false, false);
            Assert.IsNull(sut.Tier("Cure VI"));
        }
        
        [TestMethod]
        public void CastCure6_UnderCure()
        {
            var sut = new CureTiers(s => false, false, true, false);
            Assert.AreEqual("Cure V", sut.Tier("Cure VI"));
        }

        [TestMethod]
        public void CastCure6_OverAndUnderCure()
        {
            var sut = new CureTiers(s => false, true, true, false);
            Assert.AreEqual("Cure V", sut.Tier("Cure VI"));
        }

        [TestMethod]
        public void CastCure4_OverCurePriority_PriorityPlayer()
        {
            var sut = new CureTiers(s => false, false, false, true);
            Assert.AreEqual("Cure V", sut.Tier("Cure IV", true));
        }

        [TestMethod]
        public void CastCure4_OverCurePriority_NonePriorityPlayer()
        {
            var sut = new CureTiers(s => false, false, false, true);
            Assert.IsNull(sut.Tier("Cure IV", false));
        }

        [TestMethod]
        public void CastCure5()
        {
            var sut = new CureTiers(s => true, false, false, false);
            Assert.AreEqual("Cure V", sut.Tier("Cure V"));
        }

        [TestMethod]
        public void CastCure5_OverCure()
        {
            var sut = new CureTiers(s => false, true, false, false);
            Assert.IsNull(sut.Tier("Cure VI"));
        }

        [TestMethod]
        public void CastCure5_UnderCure()
        {
            var sut = new CureTiers(s => false, false, true, false);
            Assert.AreEqual("Cure IV", sut.Tier("Cure V"));
        }

        [TestMethod]
        public void CastCure()
        {
            var sut = new CureTiers(s => true, false, false, false);
            Assert.AreEqual("Cure", sut.Tier("Cure"));
        }

        [TestMethod]
        public void CastCure_UnderCure()
        {
            var sut = new CureTiers(s => false, false, true, false);
            Assert.IsNull(sut.Tier("Cure"));
        }
    }
}
