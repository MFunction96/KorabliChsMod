using Xanadu.KorabliChsMod.Core.Models;

namespace Xanadu.Test.KorabliChsMod.Core.Models
{
    [TestClass]
    public class KorabliConfigModelTest
    {
        [TestMethod]
        public void ProxyConfigModel_Equals_ShouldBeTrue_WhenSameValues()
        {
            var a = new ProxyConfigModel { Enabled = true, Address = "127.0.0.1:1080", Username = "user", Password = "pass" };
            var b = new ProxyConfigModel { Enabled = true, Address = "127.0.0.1:1080", Username = "user", Password = "pass" };
            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void ProxyConfigModel_Equals_ShouldBeFalse_WhenDifferentValues()
        {
            var a = new ProxyConfigModel { Enabled = true, Address = "127.0.0.1:1080" };
            var b = new ProxyConfigModel { Enabled = false, Address = "127.0.0.1:1080" };
            Assert.IsFalse(a.Equals(b));
        }

        [TestMethod]
        public void KorabliConfigModel_Equals_ShouldBeTrue_WhenSameValues()
        {
            var a = new KorabliConfigModel { GameFolder = "C:/Games/Ship", Proxy = new ProxyConfigModel { Enabled = true, Address = "1.1.1.1" } };
            var b = new KorabliConfigModel { GameFolder = "C:/Games/Ship", Proxy = new ProxyConfigModel { Enabled = true, Address = "1.1.1.1" } };
            Assert.IsTrue(a.Equals(b));
        }

        [TestMethod]
        public void KorabliConfigModel_Equals_ShouldBeFalse_WhenDifferentGameFolder()
        {
            var a = new KorabliConfigModel { GameFolder = "C:/A" };
            var b = new KorabliConfigModel { GameFolder = "C:/B" };
            Assert.IsFalse(a.Equals(b));
        }
    }

}
