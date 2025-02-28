using Newtonsoft.Json;
using Xanadu.KorabliChsMod.DI;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Xanadu.Test.KorabliChsMod
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class LgcIntegratorTest
    {
        /// <summary>
        /// 
        /// </summary>
        public TestContext TestContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void LgcTest()
        {
            var lgc = new LgcIntegrator();
            lgc.Load();
            this.TestContext.WriteLine(JsonConvert.SerializeObject(lgc));
        }
    }
}
